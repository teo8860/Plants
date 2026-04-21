using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Plants;

internal class Rendering
{
    public static PixelCamera camera = new(GameProperties.windowWidth, GameProperties.windowHeight, (float)GameProperties.windowWidth / (float)GameProperties.viewWidth);

    // Texture intermedia alla risoluzione logica (windowWidth x windowHeight).
    // L'intero frame viene disegnato qui, poi upscalato alla finestra fisica.
    // Questo permette all'opzione "scala" di ingrandire tutto senza toccare le coordinate GUI.
    private static RenderTexture2D? finalTexture;
    private static bool finalTextureReady = false;

    private static void EnsureFinalTexture()
    {
        if (finalTextureReady) return;
        finalTexture = RenderTexture2D.Load(GameProperties.windowWidth, GameProperties.windowHeight);
        finalTexture.Value.Texture.SetFilter(TextureFilter.Point);
        finalTextureReady = true;
    }
    
    /// <summary>
    /// Loop di rendering per la modalità minigioco standalone.
    /// </summary>
    public static void InitMinigame()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        while (!Window.ShouldClose())
        {
            camera.Update();
            var elements = GameElement.GetList();
            elements = elements.FindAll((o) => o.active == true);

            foreach (var item in elements)
                item.Update();

            var layerGui = elements.FindAll((o) => (o.guiLayer == true && o.active == true));
            layerGui.Sort((GameElement a, GameElement b) => b.depth - a.depth);

            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);

            foreach (var item in layerGui)
                item.Draw();

            GameFunctions.DrawSprite(AssetLoader.spriteLeaf, new Vector2(Input.GetMouseX(), Input.GetMouseY()), 0, 1, Color.White, 1);

            Graphics.EndDrawing();
        }

        Window.Close();
        Environment.Exit(0);
    }

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        EnsureFinalTexture();

        while (true)
        {
            
            if (Window.IsMinimized() || Window.IsHidden())
            {
                
                Window.SetState(ConfigFlags.HiddenWindow);
                //Window.Close();
                //CopperImGui.Shutdown();
               // break;
            }

            if (Window.ShouldClose())
            {
                if (GameConfig.get().CloseOnX)
                {
                    Program.ExitGame();
                    return;
                }
                Window.SetState(ConfigFlags.HiddenWindow);
            }

            Program.trayIcon?.LoopEventRender();
            
            camera.Update();
            var elements = GameElement.GetList();
            elements = elements.FindAll((o)=> o.active == true);


            foreach (var item in elements)
            {
                item.Update();
            }


            var layerBase = elements.FindAll((o)=> (o.guiLayer == false && o.active == true));
            layerBase.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            var layerGui = elements.FindAll((o)=> (o.guiLayer == true && o.active == true));
            layerGui.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);

            // Fase mondo: disegna nella renderTexture virtuale (100x125) — non toccata dallo scaling
            camera.BeginWorldMode();
            foreach (var item in layerBase)
            {
                item.Draw();
            }
            camera.EndWorldMode();

            // Passa a disegnare sulla texture intermedia in coordinate logiche (400x500)
            Graphics.BeginTextureMode(finalTexture.Value);
            Graphics.ClearBackground(Color.Black);

            // Upscale della texture mondo virtuale sulla texture logica
            camera.DrawWorld();

            // GUI: coordinate pixel logiche, invariate
            foreach (var item in layerGui)
            {
                item.Draw();
            }

            // Debug console (update + draw sopra tutto)
            DebugConsole.Update();
            DebugConsole.Draw();

            if (!DebugConsole.IsOpen)
                GameFunctions.DrawSprite(AssetLoader.spriteLeaf, new Vector2( Input.GetMouseX(), Input.GetMouseY()), 0, 1, Color.White, 1);

            Graphics.EndTextureMode();

            // Blit finale sulla finestra fisica con upscaling Point (niente blur) in base a uiScale
            int physW = GameProperties.physicalWindowWidth;
            int physH = GameProperties.physicalWindowHeight;
            Graphics.DrawTexturePro(
                finalTexture.Value.Texture,
                new Raylib_CSharp.Transformations.Rectangle(0, 0, GameProperties.windowWidth, -GameProperties.windowHeight),
                new Raylib_CSharp.Transformations.Rectangle(0, 0, physW, physH),
                Vector2.Zero,
                0f,
                Color.White
            );

            //Graphics.DrawFPS(0,0);
			Graphics.EndDrawing();


            if (Window.IsMinimized() || Window.IsHidden())
            {
                
                Window.SetState(ConfigFlags.HiddenWindow);
                //Window.Close();
                //CopperImGui.Shutdown();
               // break;
            }

            if (Window.ShouldClose())
            {
                if (GameConfig.get().CloseOnX)
                {
                    Program.ExitGame();
                    return;
                }
                Window.SetState(ConfigFlags.HiddenWindow);
            }
        }
    }

}
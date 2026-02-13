using CopperDevs.DearImGui;
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
    
    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);


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
                
                Window.SetState(ConfigFlags.HiddenWindow);
                //Window.Close();
                //CopperImGui.Shutdown();
               // break;
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

            // World rendering phase
            camera.BeginWorldMode();
            foreach (var item in layerBase)
            {
                item.Draw();
            }
            camera.EndWorldMode();

            // Draw world render texture to screen
            camera.DrawWorld();

            // GUI rendering phase
            foreach (var item in layerGui)
            {
                item.Draw();
            }

			CopperImGui.Render();
            

            GameFunctions.DrawSprite(AssetLoader.spriteLeaf, new Vector2( Input.GetMouseX(), Input.GetMouseY()), 0, 1, Color.White, 1);    
			
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
                
                Window.SetState(ConfigFlags.HiddenWindow);
                //Window.Close();
                //CopperImGui.Shutdown();
               // break;
            }
        }
    }

}
using System;
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Rendering.Gl;
using Raylib_CSharp.Textures;

namespace Plants;

/// <summary>
/// Gestisce l'inizializzazione e il game loop su Android.
/// Sostituisce Program.Main() + Rendering.Init() per la piattaforma mobile.
/// </summary>
public static class AndroidGameInit
{
    private static int screenWidth;
    private static int screenHeight;
    private static bool initialized = false;
    private static int frameCount = 0;
    private static float virtualScale;
    private static float offsetX;
    private static float offsetY;
    private static RenderTexture2D testRT;

    /// <summary>
    /// Inizializza il gioco su Android (dopo che EGL + rlgl sono pronti)
    /// </summary>
    public static void Initialize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;

        // Il gioco usa risoluzione virtuale 100x125
        // Calcoliamo la scala per adattare al display mantenendo aspect ratio
        float scaleX = (float)width / GameProperties.viewWidth;
        float scaleY = (float)height / GameProperties.viewHeight;
        virtualScale = Math.Min(scaleX, scaleY);

        // Offset per centrare il gioco sullo schermo
        offsetX = (width - GameProperties.viewWidth * virtualScale) / 2f;
        offsetY = (height - GameProperties.viewHeight * virtualScale) / 2f;

        // Inizializza la camera con dimensioni DESKTOP (400x500, ratio=4)
        // per mantenere le stesse proporzioni e gameplay del PC
        Rendering.camera = new PixelCamera(
            GameProperties.windowWidth,    // 400
            GameProperties.windowHeight,   // 500
            (float)GameProperties.windowWidth / GameProperties.viewWidth  // 4.0
        );

        // Carica assets (embedded resources)
        AssetLoader.LoadAll();

        // Inizializza le notifiche (no-op su Android per ora)
        NotificationManager.Initialize();

        // Inizializza il gioco
        Game.Init();

        initialized = true;
        Console.WriteLine($"[Android] Gioco inizializzato: {width}x{height}, scala={virtualScale:F2}");
        Console.WriteLine($"[Android] Camera: screenW={Rendering.camera.screenWidth}, screenH={Rendering.camera.screenHeight}, ratio={Rendering.camera.virtualRatio}");
        Console.WriteLine($"[Android] Viewport rlgl: {RlGl.GetFramebufferWidth()}x{RlGl.GetFramebufferHeight()}");
    }

    /// <summary>
    /// Esegue un frame del gioco: update + draw
    /// </summary>
    public static void Frame(float deltaTime)
    {
        if (!initialized) return;

        try
        {
            frameCount++;

            // Poll input events (cycles previous/current touch state for press detection)
            AndroidBridge.PollInputEvents();

            // Map touch input from screen space to game space (400x500)
            int touchX = AndroidBridge.GetTouchX();
            int touchY = AndroidBridge.GetTouchY();
            bool touchPressed = AndroidBridge.IsTouchPressed();

            // Convert screen coords → game coords (400x500)
            // The viewport maps full screen to the 400x500 ortho projection
            int gameX = (int)(touchX * (float)GameProperties.windowWidth / screenWidth);
            int gameY = (int)(touchY * (float)GameProperties.windowHeight / screenHeight);

            AndroidBridge.SetTouchInput(gameX, gameY, touchPressed);

            // Clear
            AndroidBridge.TestFrame_ClearOnly();

            // Disabilita culling (necessario per il flip Y)
            RlGl.DisableBackfaceCulling();

            // Proiezione: coordinate virtuali con zoom camera
            float viewW = Rendering.camera.view.X;
            float viewH = Rendering.camera.view.Y;
            RlGl.MatrixMode(MatrixMode.Projection);
            RlGl.LoadIdentity();
            RlGl.Ortho(0, viewW, 0, viewH, 0.0, 1.0);
            RlGl.MatrixMode(MatrixMode.ModelView);
            RlGl.LoadIdentity();

            // Update
            var elements = GameElement.GetList();
            elements = elements.FindAll(o => o.active);
            foreach (var item in elements)
            {
                item.Update();
            }

            // Sort
            var layerBase = elements.FindAll(o => !o.guiLayer && o.active);
            layerBase.Sort((a, b) => b.depth - a.depth);

            // Crea RT una volta sola
            if (testRT.Id == 0)
            {
                testRT = RenderTexture2D.Load(102, 216);
                testRT.Texture.SetFilter(TextureFilter.Point);
                Console.WriteLine($"[Android] RT: fbo={testRT.Id}, tex={testRT.Texture.Id}, {testRT.Texture.Width}x{testRT.Texture.Height}");
            }

            // === Render nel FBO manualmente (senza BeginTextureMode) ===
            RlGl.DrawRenderBatchActive(); // flush prima di cambiare FBO
            RlGl.EnableFramebuffer(testRT.Id);
            RlGl.Viewport(0, 0, testRT.Texture.Width, testRT.Texture.Height);
            RlGl.MatrixMode(MatrixMode.Projection);
            RlGl.LoadIdentity();
            RlGl.Ortho(0, Rendering.camera.view.X, 0, Rendering.camera.view.Y, 0.0, 1.0);
            RlGl.MatrixMode(MatrixMode.ModelView);
            RlGl.LoadIdentity();
            RlGl.DisableBackfaceCulling();

            Graphics.ClearBackground(new Color(10, 10, 10, 255));

            foreach (var item in layerBase)
            {
                item.Draw();
            }

            RlGl.DrawRenderBatchActive(); // flush nel FBO
            RlGl.DisableFramebuffer(); // torna a FBO 0
            RlGl.Viewport(0, 0, screenWidth, screenHeight);

            // === Disegna FBO + GUI con proiezione desktop (400x500) ===
            // Proiezione unica 400x500 per FBO e GUI
            RlGl.MatrixMode(MatrixMode.Projection);
            RlGl.LoadIdentity();
            RlGl.Ortho(0, GameProperties.windowWidth, GameProperties.windowHeight, 0, 0.0, 1.0);
            RlGl.MatrixMode(MatrixMode.ModelView);
            RlGl.LoadIdentity();

            // Disegna la texture FBO scalata a 400x500
            RlGl.SetTexture(testRT.Texture.Id);
            RlGl.Begin(DrawMode.Quads);
                RlGl.Color4F(1f, 1f, 1f, 1f);
                RlGl.TexCoord2F(0, 1); RlGl.Vertex2F(0, 0);
                RlGl.TexCoord2F(0, 0); RlGl.Vertex2F(0, GameProperties.windowHeight);
                RlGl.TexCoord2F(1, 0); RlGl.Vertex2F(GameProperties.windowWidth, GameProperties.windowHeight);
                RlGl.TexCoord2F(1, 1); RlGl.Vertex2F(GameProperties.windowWidth, 0);
            RlGl.End();
            RlGl.SetTexture(0);

            // GUI layer sopra (stessa proiezione 400x500)
            var layerGui = elements.FindAll(o => o.guiLayer && o.active);
            layerGui.Sort((a, b) => b.depth - a.depth);
            foreach (var item in layerGui)
            {
                item.Draw();
            }

            RlGl.DrawRenderBatchActive();

            if (frameCount % 120 == 1)
                Console.WriteLine($"[Android] Frame #{frameCount}, base={layerBase.Count}, gui={layerGui.Count}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Android] Errore nel frame: {ex.Message}");
        }
    }
}

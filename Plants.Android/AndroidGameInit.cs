using System;
using System.Numerics;
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

    /// <summary>
    /// Inizializza il gioco su Android (dopo che EGL + rlgl sono pronti)
    /// </summary>
    public static void Initialize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;

        // Aggiorna le proprietà del gioco per lo schermo Android
        // Il gioco usa risoluzione virtuale 100x125, scalata 4x
        // Su Android adattiamo la scala allo schermo
        float scaleX = (float)width / GameProperties.viewWidth;
        float scaleY = (float)height / GameProperties.viewHeight;
        float scale = Math.Min(scaleX, scaleY);

        // Inizializza la camera con le dimensioni Android
        Rendering.camera = new PixelCamera(width, height, scale);

        // Carica assets (embedded resources)
        AssetLoader.LoadAll();

        // Inizializza le notifiche (no-op su Android per ora)
        NotificationManager.Initialize();

        // Inizializza il gioco
        Game.Init();

        initialized = true;
        Console.WriteLine($"[Android] Gioco inizializzato: {width}x{height}, scala={scale:F2}");
    }

    /// <summary>
    /// Esegue un frame del gioco: update + draw
    /// </summary>
    public static void Frame(float deltaTime)
    {
        if (!initialized) return;

        try
        {
            // Clear screen
            AndroidBridge.ClearBuffers();

            // Aggiorna tutti gli elementi attivi
            var elements = GameElement.GetList();
            elements = elements.FindAll(o => o.active);

            foreach (var item in elements)
            {
                item.Update();
            }

            // Separa layer mondo e GUI
            var layerBase = elements.FindAll(o => !o.guiLayer && o.active);
            layerBase.Sort((a, b) => b.depth - a.depth);

            var layerGui = elements.FindAll(o => o.guiLayer && o.active);
            layerGui.Sort((a, b) => b.depth - a.depth);

            // Render mondo
            Rendering.camera.BeginWorldMode();
            foreach (var item in layerBase)
            {
                item.Draw();
            }
            Rendering.camera.EndWorldMode();

            // Disegna il mondo sullo schermo
            Rendering.camera.DrawWorld();

            // Render GUI
            foreach (var item in layerGui)
            {
                item.Draw();
            }

            // Flush del batch di rendering
            AndroidBridge.FlushBatch();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Android] Errore nel frame: {ex.Message}");
        }
    }
}

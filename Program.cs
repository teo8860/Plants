using NotificationIconSharp;
using Raylib_CSharp.Windowing;
using System;
using System.Drawing;
using Plants;

namespace Plants;
     


internal static class Program
{
	static NativeTrayIcon trayIcon;

    
    public static void Main()
    {
        //SetupIcon();

        Window.Init(GameProperties.windowWidth, GameProperties.windowHeight, "Plants");

        // Avvia il render ed il loop
        Game.Init();
        Rendering.Init();

        // Load saved game if exists (dopo l'inizializzazione degli oggetti)
        Console.WriteLine($"[PROGRAM] Controllo esistenza file salvataggio...");
        var saveData = GameSaveManager.LoadGame();
        if (saveData != null)
        {
            Console.WriteLine($"[PROGRAM] File salvataggio trovato - Mondo: {saveData.CurrentWorld}, Difficoltà: {saveData.CurrentDifficulty}, Weather: {saveData.CurrentWeather}, Phase: {saveData.CurrentPhase}");
            Console.WriteLine($"[PROGRAM] Ripristino stato gioco...");
            GameSaveManager.RestoreGameState(saveData);
            Console.WriteLine($"[PROGRAM] Stato gioco ripristinato - Mondo corrente: {WorldManager.GetCurrentWorld()}");
        }
        else
        {
            Console.WriteLine($"[PROGRAM] Nessun file di salvataggio trovato - avvio nuovo gioco");
        }

    }

    private static void SetupIcon()
    {
        // Carica icona nella barra delle applicazioni
        Icon icon = Utility.LoadIconFromEmbedded("icon.ico", "assets");
        trayIcon = new NativeTrayIcon(icon, "Tooltip icona");
        
        trayIcon.OnClickLeft += () =>
        {
           Window.ClearState(ConfigFlags.HiddenWindow);
           var m = MouseHelper.GetMousePosition();

           Window.SetPosition((int)m.X-100, (int)m.Y-400);
        };

        trayIcon.OnExit  += () =>
        {
             // Auto-save on exit
             GameSaveManager.SaveGame(
                 WorldManager.GetCurrentWorld(),
                 WorldManager.GetCurrentWorldDifficulty(),
                 WeatherManager.GetCurrentWeather(),
                 FaseGiorno.GetCurrentPhase()
             );
             trayIcon.Dispose();
             Window.Close();
        };
        
        trayIcon.LoopEvent();
       
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            trayIcon.Dispose();
        };
    }
}


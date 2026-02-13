using System;
using System.Runtime.InteropServices;
using Raylib_CSharp.Windowing;

namespace Plants;

/// <summary>
/// Gestisce le azioni eseguite quando l'utente clicca sulle notifiche
/// </summary>
public static class NotificationActionHandler
{
    /// <summary>
    /// Inizializza il gestore delle azioni
    /// </summary>
    public static void Initialize()
    {
        // L'handler degli eventi è già registrato in NotificationManager
        // Qui possiamo aggiungere ulteriore logica se necessario
    }

    /// <summary>
    /// Esegue l'azione richiesta dalla notifica
    /// </summary>
    public static void HandleAction(string action)
    {
        Console.WriteLine($"Azione notifica ricevuta: {action}");

        switch (action)
        {
            case "water":
                WaterPlant();
                break;

            case "cure":
                CureParasites();
                break;

            case "rescue":
                RescuePlant();
                break;

            case "travel":
                TravelToNextWorld();
                break;

            case "open":
                OpenGame();
                break;

            default:
                Console.WriteLine($"Azione sconosciuta: {action}");
                break;
        }
    }

    /// <summary>
    /// Annaffia la pianta
    /// </summary>
    private static void WaterPlant()
    {
        if (Game.pianta == null) return;

        // Annaffia la pianta al 50%
        Game.pianta.proprieta.Annaffia(0.5f);

        Console.WriteLine("Pianta annaffiata da notifica!");

        // Mostra la finestra del gioco
       //OpenGame();
    }

    /// <summary>
    /// Cura i parassiti
    /// </summary>
    private static void CureParasites()
    {
        if (Game.pianta == null) return;

        // Cura completamente i parassiti
        Game.pianta.proprieta.CuraParassiti(1.0f);

        Console.WriteLine("Parassiti curati da notifica!");

        // Mostra la finestra del gioco
        OpenGame();
    }

    /// <summary>
    /// Salva la pianta in condizioni critiche
    /// </summary>
    private static void RescuePlant()
    {
        if (Game.pianta == null) return;

        // Ripristina salute al 30%
        float healAmount = 0.3f - Game.pianta.Stats.Salute;
        if (healAmount > 0)
        {
            Game.pianta.proprieta.Rigenera(healAmount);
        }

        // Annaffia
        if (Game.pianta.Stats.Idratazione < 0.3f)
        {
            Game.pianta.proprieta.Annaffia(0.3f);
        }

        // Fornisci ossigeno
        if (Game.pianta.Stats.Ossigeno < 0.3f)
        {
            Game.pianta.proprieta.FornisciOssigeno(0.3f);
        }

        Console.WriteLine("Pianta salvata da notifica!");

        // Mostra la finestra del gioco
        OpenGame();
    }

    /// <summary>
    /// Viaggia verso il prossimo mondo
    /// </summary>
    private static void TravelToNextWorld()
    {
        // Cambia mondo
        WorldManager.SetNextWorld();

        // Reset della pianta
        if (Game.pianta != null)
        {
            Game.pianta.Reset();

            // Aggiorna i colori della pianta
            Game.pianta.SetNaturalColors(WorldManager.GetCurrentWorld());
        }

        Console.WriteLine($"Viaggio verso {WorldManager.GetWorldName(WorldManager.GetCurrentWorld())}!");

        // Mostra la finestra del gioco
        OpenGame();
    }

    /// <summary>
    /// Apre/ripristina la finestra del gioco e la porta in primo piano
    /// </summary>
    private static void OpenGame()
    {
        try
        {
            if (Window.IsReady())
            {
                // Rimuovi lo stato nascosto
                Window.ClearState(ConfigFlags.HiddenWindow);

                // Porta la finestra in primo piano usando SetForegroundWindow
                IntPtr hwnd = Window.GetHandle();
                if (hwnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hwnd);
                    ShowWindow(hwnd, SW_RESTORE); // Ripristina se minimizzata
                    BringWindowToTop(hwnd);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nell'apertura del gioco: {ex.Message}");
        }
    }

    // P/Invoke per portare la finestra in primo piano
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    private const int SW_RESTORE = 9;
}
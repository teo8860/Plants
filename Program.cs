﻿using NotificationIconSharp;
using Plants;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Windowing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
namespace Plants;



internal static class Program
{
	public static NativeTrayIcon trayIcon;
     
        // Const per stili finestra
    const int GWL_STYLE = -16;
    const uint WS_MINIMIZEBOX = 0x00020000;

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    public static bool IsMinigameMode = false;
    public static TipoMinigioco MinigameType;

    public static void Main(string[] args)
    {
        // Initialize crash logging FIRST before anything else
        string exeDir = AppContext.BaseDirectory;
        CrashLogger.Init(exeDir);
        CrashLogger.SetupGlobalHandlers();
        CrashLogger.LogInfo("Program", $"Plants starting - Version: {typeof(Program).Assembly.GetName().Version}");
        CrashLogger.LogInfo("Program", $"Executable: {exeDir}");

        // Windows Toast Notifications require an AUMID. Set it on the process and
        // install a per-user Start Menu shortcut so the system can route toast activations
        // back to this exe. Both steps are no-admin and idempotent.
        const string AUMID = "Plants.Game";
        ShortcutInstaller.SetProcessAumid(AUMID);
        string exePath = Environment.ProcessPath ?? Path.Combine(exeDir, "Plants.exe");
        ShortcutInstaller.EnsureShortcut(AUMID, "Plants", exePath);
        
        try
        {
            // Controlla se avviato in modalità minigioco standalone
            if (args.Length >= 2 && args[0] == "--minigioco")
            {
                if (Enum.TryParse<TipoMinigioco>(args[1], out var tipo))
                {
                    IsMinigameMode = true;
                    MinigameType = tipo;
                    AvviaMinigiocoStandalone(tipo);
                    return;
                }
            }

            // Carica config prima di creare la finestra per applicare la scala
            var cfg = GameConfig.get();
            GameProperties.uiScale = cfg.UiScale;

            Window.Init(GameProperties.physicalWindowWidth, GameProperties.physicalWindowHeight, "Plants");
            Window.ClearState(ConfigFlags.ResizableWindow);

            // Scala input mouse in modo che GetMouseX/Y restituiscano coordinate logiche
            float invMul = 1f / GameProperties.uiScaleMultiplier;
            Input.SetMouseScale(invMul, invMul);

            // rimuove la possibilità di minimizzare
            IntPtr hwnd = Window.GetHandle();
            uint style = GetWindowLong(hwnd, GWL_STYLE);
            style &= ~WS_MINIMIZEBOX;
            SetWindowLong(hwnd, GWL_STYLE, style);


            SetupIcon();
            Window.SetPosition(Window.GetMonitorWidth(0) - GameProperties.physicalWindowWidth - 20, Window.GetMonitorHeight(0) - GameProperties.physicalWindowHeight - 50);

            // Avvio nascosto nella tray se l'utente ha attivato l'opzione
            if (cfg.StartHidden)
                Window.SetState(ConfigFlags.HiddenWindow);

            Input.HideCursor();


            Game.Init();
            Rendering.Init();
        }
        catch (Exception ex)
        {
            CrashLogger.LogFatal("Main", ex, true);
            CrashLogger.DumpGameState("Unhandled exception in Main");
            // Don't use Environment.Exit here - let it crash naturally so the log is written
        }
	}

    private static void AvviaMinigiocoStandalone(TipoMinigioco tipo)
    {
        int miniHeight = GameProperties.windowHeight - 80;
        Window.Init(GameProperties.windowWidth, miniHeight, $"Plants - Minigioco");
        Window.ClearState(ConfigFlags.ResizableWindow);

        IntPtr hwnd = Window.GetHandle();
        uint style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~WS_MINIMIZEBOX;
        SetWindowLong(hwnd, GWL_STYLE, style);

        // Centra la finestra
        int monW = Window.GetMonitorWidth(0);
        int monH = Window.GetMonitorHeight(0);
        Window.SetPosition((monW - GameProperties.windowWidth) / 2, (monH - miniHeight) / 2);

        // Aggiorna la camera per le dimensioni ridotte
        Rendering.camera = new PixelCamera(GameProperties.windowWidth, miniHeight, (float)GameProperties.windowWidth / (float)GameProperties.viewWidth);

        Input.HideCursor();

        AssetLoader.LoadAll();
        ManagerMinigames.InitStandalone(tipo);
        Rendering.InitMinigame();
    }

    public static void ExitGame()
    {
        // Non salvare durante la selezione del seme: il save e' stato appena
        // cancellato dalla morte e salverebbe dati stale della vecchia pianta
        if (!Game.IsModalitaPiantaggio)
            GameSave.get().Save();
        NotificationManager.Cleanup();
        trayIcon?.Dispose();
        Window.Close();
        Environment.Exit(0);
    }

    private static void SetupIcon()
    {
        // Carica icona nella barra delle applicazioni
        Icon icon = Utility.LoadIconFromEmbedded("icon.ico", "Assets");
        trayIcon = new NativeTrayIcon(icon, "Plants");
        
        trayIcon.OnClickLeft += ()=>
        {
            Window.ClearState(ConfigFlags.HiddenWindow);
            Window.SetPosition(Window.GetMonitorWidth(0) - GameProperties.physicalWindowWidth - 20, Window.GetMonitorHeight(0) - GameProperties.physicalWindowHeight - 50);
        };

        trayIcon.OnExit  += () =>
        {
            ExitGame();
        };
       
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            trayIcon.Dispose();
        };
    }
}


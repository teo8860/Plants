#if WINDOWS
using NotificationIconSharp;
#endif
using Plants;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Windowing;
using System;
using System.Runtime.InteropServices;

namespace Plants;


internal static class Program
{
#if WINDOWS
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
#endif

    public static bool IsMinigameMode = false;
    public static TipoMinigioco MinigameType;

    public static void Main(string[] args)
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

        Window.Init(GameProperties.windowWidth, GameProperties.windowHeight, "Plants");
        Window.ClearState(ConfigFlags.ResizableWindow);

#if WINDOWS
        // rimuove la possibilità di minimizzare
        IntPtr hwnd = Window.GetHandle();
		uint style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~WS_MINIMIZEBOX;
        SetWindowLong(hwnd, GWL_STYLE, style);

        SetupIcon();
        Window.SetPosition(Window.GetMonitorWidth(0) - GameProperties.windowWidth - 20, Window.GetMonitorHeight(0) - GameProperties.windowHeight - 50);
#endif

        Input.HideCursor();

		Game.Init();
        Rendering.Init();
	}

    private static void AvviaMinigiocoStandalone(TipoMinigioco tipo)
    {
        int miniHeight = GameProperties.windowHeight - 80;
        Window.Init(GameProperties.windowWidth, miniHeight, $"Plants - Minigioco");
        Window.ClearState(ConfigFlags.ResizableWindow);

#if WINDOWS
        IntPtr hwnd = Window.GetHandle();
        uint style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~WS_MINIMIZEBOX;
        SetWindowLong(hwnd, GWL_STYLE, style);

        // Centra la finestra
        int monW = Window.GetMonitorWidth(0);
        int monH = Window.GetMonitorHeight(0);
        Window.SetPosition((monW - GameProperties.windowWidth) / 2, (monH - miniHeight) / 2);
#endif

        // Aggiorna la camera per le dimensioni ridotte
        Rendering.camera = new PixelCamera(GameProperties.windowWidth, miniHeight, (float)GameProperties.windowWidth / (float)GameProperties.viewWidth);

        Input.HideCursor();

        AssetLoader.LoadAll();
        ManagerMinigames.InitStandalone(tipo);
        Rendering.InitMinigame();
    }

#if WINDOWS
    private static void SetupIcon()
    {
        // Carica icona nella barra delle applicazioni
        System.Drawing.Icon icon = Utility.LoadIconFromEmbedded("icon.ico", "Assets");
        trayIcon = new NativeTrayIcon(icon, "Plants");

        trayIcon.OnClickLeft += ()=>
        {
            Window.ClearState(ConfigFlags.HiddenWindow);
            Window.SetPosition(Window.GetMonitorWidth(0) - GameProperties.windowWidth - 20, Window.GetMonitorHeight(0) - GameProperties.windowHeight - 50);
        };

        trayIcon.OnExit  += () =>
        {
            GameSave.get().Save();
            NotificationManager.Cleanup();
            trayIcon.Dispose();
            Window.Close();
        };

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            trayIcon.Dispose();
        };
    }
#endif
}

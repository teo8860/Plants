using NotificationIconSharp;
using Raylib_CSharp.Windowing;
using System;
using System.Drawing;
using Plants;
using System.Collections.Generic;
using Raylib_CSharp.Interact;

namespace Plants;
     


internal static class Program
{
	static NativeTrayIcon trayIcon;

    
    public static void Main()
    {
        //SetupIcon();

        Window.Init(GameProperties.windowWidth, GameProperties.windowHeight, "Plants");

        // Avvia il render ed il loop
        Input.HideCursor();

        Game.Init();
        Rendering.Init();

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
            GameSave.get().Save();
            NotificationManager.Cleanup();
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


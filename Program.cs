using NotificationIconSharp;
using Raylib_CSharp.Windowing;
using System;
using System.Drawing;

namespace Plants;
     


internal static class Program
{
	static NativeTrayIcon trayIcon;

    
    public static void Main()
    {
        // Carica icona nella barra delle applicazioni
        Icon icon = Utility.LoadIconFromEmbedded("icon.ico", "assets");
        trayIcon = new NativeTrayIcon(icon, "Tooltip icona");
        
        trayIcon.OnClickLeft += () =>
        {
           Window.ClearState(ConfigFlags.HiddenWindow);
           var m = MouseHelper.GetMousePosition();

           Window.SetPosition(m.X-100, m.Y-400);
        };

        trayIcon.OnExit  += () =>
        {
            trayIcon.Dispose();
            Window.Close();
        };
         

        // Avvia il render ed il loop
        Rendering.Init();
        trayIcon.LoopEvent();

       
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            trayIcon.Dispose();
        };

    }

   
}


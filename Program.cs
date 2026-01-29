using NotificationIconSharp;
using Raylib_CSharp.Windowing;
using System;
using System.Drawing;
using Plants;
using System.Collections.Generic;

namespace Plants;
     


internal static class Program
{
	static NativeTrayIcon trayIcon;

    
    public static void Main()
    {
        //SetupIcon();
        
        var opening = new OpeningSystem();
        Dictionary<SeedRarity, int> packageCounts = new()
        {
            { SeedRarity.Comune, 0 },
            { SeedRarity.NonComune, 0 },
            { SeedRarity.Raro, 0 },
            { SeedRarity.Epico, 0 },
            { SeedRarity.Leggendario, 0 }
        };

		for(int i=0; i<1000; i++)
        {
            var seed = opening.RollSeedFromPackage(SeedPackageRarity.Common);
            packageCounts[seed.rarity]++;
		}

        Console.WriteLine("Common packages opened: " + packageCounts[SeedRarity.Comune]);
        Console.WriteLine("Uncommon packages opened: " + packageCounts[SeedRarity.NonComune]);
        Console.WriteLine("Rare packages opened: " + packageCounts[SeedRarity.Raro]);
        Console.WriteLine("Epic packages opened: " + packageCounts[SeedRarity.Epico]);
        Console.WriteLine("Legendary packages opened: " + packageCounts[SeedRarity.Leggendario]);
        Console.WriteLine("\n\n\n");
        Window.Init(GameProperties.windowWidth, GameProperties.windowHeight, "Plants");

        // Avvia il render ed il loop
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


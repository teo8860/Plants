﻿using CopperDevs.Core.Utility;
using CopperDevs.DearImGui;
using CopperDevs.DearImGui.Renderer.Raylib;
using CopperDevs.DearImGui.Renderer.Raylib.Raylib_CSharp;
using CopperDevs.DearImGui.Rendering;
using Hexa.NET.ImGui;
using NotificationIconSharp;
using Plants;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Windowing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing;
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
    
    public static void Main()
    {

        Window.Init(GameProperties.windowWidth, GameProperties.windowHeight, "Plants");
        Window.ClearState(ConfigFlags.ResizableWindow);

        IntPtr hwnd = Window.GetHandle();
		uint style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~WS_MINIMIZEBOX; // rimuove la possibilità di minimizzare
        SetWindowLong(hwnd, GWL_STYLE, style);


        //SetupIcon();
        //Window.SetState(ConfigFlags.HiddenWindow);

        Input.HideCursor();
        
     
		Game.Init();
        Window.SetState(ConfigFlags.HiddenWindow);
        Rendering.Init();
	}

    private static void SetupIcon()
    {
        // Carica icona nella barra delle applicazioni
        Icon icon = Utility.LoadIconFromEmbedded("icon.ico", "Assets");
        trayIcon = new NativeTrayIcon(icon, "Tooltip icona");
        
        trayIcon.OnClickLeft += ()=>
        {
           Window.ClearState(ConfigFlags.HiddenWindow);
           var m = MouseHelper.GetMousePosition();

           Window.SetPosition((int)m.X-(GameProperties.windowWidth/2), (int)m.Y-GameProperties.windowHeight-50);
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
}


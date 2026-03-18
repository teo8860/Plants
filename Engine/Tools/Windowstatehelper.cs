using Raylib_CSharp.Windowing;
using System;
using System.Runtime.InteropServices;

namespace Plants;

/// <summary>
/// Helper per rilevare lo stato della finestra del gioco
/// </summary>
public static class WindowStateHelper
{
    /// <summary>
    /// Verifica se la finestra del gioco è in primo piano
    /// </summary>
    public static bool IsGameWindowFocused()
    {
        try
        {
            return !Window.IsHidden();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel controllo focus finestra: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica se la finestra del gioco è minimizzata
    /// </summary>
    public static bool IsGameWindowMinimized()
    {
#if WINDOWS
        try
        {
            IntPtr hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero) return true;

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);

            if (GetWindowPlacement(hwnd, ref placement))
            {
                return placement.showCmd == SW_SHOWMINIMIZED;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel controllo minimizzazione: {ex.Message}");
            return false;
        }
#else
        return false;
#endif
    }

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    private const int SW_SHOWMINIMIZED = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }
#endif
}

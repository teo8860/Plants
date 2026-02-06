using System;
using System.Runtime.InteropServices;

namespace Plants;

/// <summary>
/// Helper per rilevare lo stato della finestra del gioco
/// </summary>
public static class WindowStateHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>
    /// Verifica se la finestra del gioco è in primo piano
    /// </summary>
    public static bool IsGameWindowFocused()
    {
        try
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            IntPtr activeWindow = GetActiveWindow();

            if (foregroundWindow == IntPtr.Zero || activeWindow == IntPtr.Zero)
                return false;

            // Ottieni il process ID della finestra in primo piano
            GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);

            // Ottieni il process ID corrente
            uint currentProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

            // Se il processo in primo piano è il nostro, siamo focati
            return foregroundProcessId == currentProcessId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel controllo focus finestra: {ex.Message}");
            return false; // In caso di errore, assumiamo non focati
        }
    }

    /// <summary>
    /// Verifica se la finestra del gioco è minimizzata
    /// </summary>
    public static bool IsGameWindowMinimized()
    {
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
    }

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
}
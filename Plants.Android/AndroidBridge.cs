using System;
using System.Runtime.InteropServices;

namespace Plants;

/// <summary>
/// Bridge P/Invoke verso la libreria nativa che gestisce EGL + rlgl su Android.
/// Il bridge nativo (libplants_bridge.so) si occupa di:
/// - Creare il contesto EGL dalla Surface Android
/// - Inizializzare rlgl (il layer GL di raylib)
/// - Swap dei buffers
/// - Gestione dell'input touch → mouse
/// </summary>
public static class AndroidBridge
{
    private const string BridgeLib = "plants_bridge";

    // ========== Display/EGL ==========

    /// <summary>
    /// Inizializza EGL display + contesto OpenGL ES 2.0 + rlgl
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern bool InitDisplay(IntPtr nativeWindow, int width, int height);

    /// <summary>
    /// Scambia i buffer (presenta il frame)
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void SwapBuffers();

    /// <summary>
    /// Pulisce il contesto EGL e rilascia risorse
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void CleanupDisplay();

    /// <summary>
    /// Aggiorna le dimensioni dello schermo (dopo rotazione/resize)
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void SetScreenSize(int width, int height);

    /// <summary>
    /// Richiedi la chiusura del game loop
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void RequestClose();

    /// <summary>
    /// Verifica se è stata richiesta la chiusura
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern bool ShouldClose();

    // ========== Input ==========

    /// <summary>
    /// Aggiorna la posizione touch (mappata come mouse)
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void SetTouchPosition(int x, int y, bool pressed);

    /// <summary>
    /// Ottieni la posizione X corrente del touch
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern int GetTouchX();

    /// <summary>
    /// Ottieni la posizione Y corrente del touch
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern int GetTouchY();

    /// <summary>
    /// Verifica se il touch è premuto (equivalente mouse button)
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern bool IsTouchPressed();

    // ========== rlgl wrappers ==========

    /// <summary>
    /// Inizializza il viewport rlgl
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void RlglInit(int width, int height);

    /// <summary>
    /// Pulisce i buffer dello schermo (glClear)
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void ClearBuffers();

    /// <summary>
    /// Flush del batch di rendering rlgl
    /// </summary>
    [DllImport(BridgeLib)]
    public static extern void FlushBatch();

    // ========== ANativeWindow ==========

    /// <summary>
    /// Ottieni ANativeWindow da una Surface Java
    /// </summary>
    [DllImport("android")]
    public static extern IntPtr ANativeWindow_fromSurface(IntPtr env, IntPtr surface);

    /// <summary>
    /// Rilascia il riferimento al ANativeWindow
    /// </summary>
    [DllImport("android")]
    public static extern void ANativeWindow_release(IntPtr window);

    public static IntPtr GetNativeWindow(IntPtr jniEnv, IntPtr surfaceHandle)
    {
        return ANativeWindow_fromSurface(jniEnv, surfaceHandle);
    }

    public static void ReleaseNativeWindow(IntPtr window)
    {
        if (window != IntPtr.Zero)
            ANativeWindow_release(window);
    }
}

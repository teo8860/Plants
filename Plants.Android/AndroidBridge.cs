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

    // ========== Asset Manager ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_InitAssetManagerJni")]
    public static extern void InitAssetManagerJni(IntPtr jniEnv, IntPtr javaAssetManager, string internalDataPath);

    // ========== Display/EGL ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_InitDisplay")]
    public static extern bool InitDisplay(IntPtr nativeWindow, int width, int height);

    [DllImport(BridgeLib, EntryPoint = "Bridge_SwapBuffers")]
    public static extern void SwapBuffers();

    [DllImport(BridgeLib, EntryPoint = "Bridge_CleanupDisplay")]
    public static extern void CleanupDisplay();

    [DllImport(BridgeLib, EntryPoint = "Bridge_SetScreenSize")]
    public static extern void SetScreenSize(int width, int height);

    [DllImport(BridgeLib, EntryPoint = "Bridge_RequestClose")]
    public static extern void RequestClose();

    [DllImport(BridgeLib, EntryPoint = "Bridge_ShouldClose")]
    public static extern bool ShouldClose();

    // ========== Input ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_SetTouchPosition")]
    public static extern void SetTouchPosition(int x, int y, bool pressed);

    [DllImport(BridgeLib, EntryPoint = "Bridge_GetTouchX")]
    public static extern int GetTouchX();

    [DllImport(BridgeLib, EntryPoint = "Bridge_GetTouchY")]
    public static extern int GetTouchY();

    [DllImport(BridgeLib, EntryPoint = "Bridge_IsTouchPressed")]
    public static extern bool IsTouchPressed();

    // ========== rlgl wrappers ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_RlglInit")]
    public static extern void RlglInit(int width, int height);

    [DllImport(BridgeLib, EntryPoint = "Bridge_ClearBuffers")]
    public static extern void ClearBuffers();

    [DllImport(BridgeLib, EntryPoint = "Bridge_FlushBatch")]
    public static extern void FlushBatch();

    [DllImport(BridgeLib, EntryPoint = "Bridge_TestFrame_ClearOnly")]
    public static extern void TestFrame_ClearOnly();

    [DllImport(BridgeLib, EntryPoint = "Bridge_TestFrame")]
    public static extern void TestFrame();

    // ========== FBO Diagnostics ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_GetDefaultFramebuffer")]
    public static extern int GetDefaultFramebuffer();

    [DllImport(BridgeLib, EntryPoint = "Bridge_CheckFramebufferStatus")]
    public static extern int CheckFramebufferStatus(uint fboId);

    [DllImport(BridgeLib, EntryPoint = "Bridge_TestFBO")]
    public static extern void TestFBO(int screenW, int screenH);

    // ========== Touch → Raylib Input ==========

    [DllImport(BridgeLib, EntryPoint = "Bridge_SetTouchInput")]
    public static extern void SetTouchInput(int x, int y, bool pressed);

    [DllImport(BridgeLib, EntryPoint = "Bridge_PollInputEvents")]
    public static extern void PollInputEvents();

    // ========== ANativeWindow ==========

    [DllImport("android")]
    public static extern IntPtr ANativeWindow_fromSurface(IntPtr env, IntPtr surface);

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

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Runtime;
using System;
using System.Threading;

namespace Plants;

/// <summary>
/// Entry point Android. Crea una SurfaceView per il rendering OpenGL
/// e avvia il game loop su un thread separato.
/// </summary>
[Activity(
    Label = "Plants",
    MainLauncher = true,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden,
    ScreenOrientation = ScreenOrientation.Portrait
)]
public class MainActivity : Activity, ISurfaceHolderCallback
{
    private SurfaceView surfaceView;
    private Thread gameThread;
    private bool isRunning = false;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Fullscreen immersivo
        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
            SystemUiFlags.Fullscreen |
            SystemUiFlags.HideNavigation |
            SystemUiFlags.ImmersiveSticky
        );

        // Crea la SurfaceView per il rendering
        surfaceView = new SurfaceView(this);
        surfaceView.Holder.AddCallback(this);
        SetContentView(surfaceView);
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        // La superficie è pronta - avvia il game loop su un thread separato
        isRunning = true;
        gameThread = new Thread(() => GameLoop(holder.Surface));
        gameThread.Start();
    }

    public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
    {
        // Notifica il bridge del cambio dimensioni
        AndroidBridge.SetScreenSize(width, height);
    }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        // Ferma il game loop
        isRunning = false;
        AndroidBridge.RequestClose();

        if (gameThread != null)
        {
            gameThread.Join(3000); // Aspetta max 3 secondi
            gameThread = null;
        }
    }

    /// <summary>
    /// Game loop principale - gira sul thread GL (NON UI thread)
    /// </summary>
    private void GameLoop(Surface surface)
    {
        try
        {
            // Ottieni il native window dalla Surface Android
            IntPtr nativeWindow = AndroidBridge.GetNativeWindow(
                Java.Interop.JniEnvironment.EnvironmentPointer,
                surface.Handle
            );

            if (nativeWindow == IntPtr.Zero)
            {
                Console.WriteLine("[Android] Errore: impossibile ottenere ANativeWindow");
                return;
            }

            int width = surfaceView.Width;
            int height = surfaceView.Height;

            // Inizializza EGL + rlgl tramite il bridge nativo
            if (!AndroidBridge.InitDisplay(nativeWindow, width, height))
            {
                Console.WriteLine("[Android] Errore: impossibile inizializzare il display");
                AndroidBridge.ReleaseNativeWindow(nativeWindow);
                return;
            }

            Console.WriteLine($"[Android] Display inizializzato: {width}x{height}");

            // Inizializza il gioco
            AndroidGameInit.Initialize(width, height);

            // Game loop
            DateTime lastFrame = DateTime.UtcNow;

            while (isRunning)
            {
                DateTime now = DateTime.UtcNow;
                float deltaTime = (float)(now - lastFrame).TotalSeconds;
                lastFrame = now;

                // Update + Draw
                AndroidGameInit.Frame(deltaTime);

                // Swap buffers
                AndroidBridge.SwapBuffers();

                // Cap a ~60 FPS
                int sleepMs = Math.Max(0, 16 - (int)((DateTime.UtcNow - now).TotalMilliseconds));
                if (sleepMs > 0) Thread.Sleep(sleepMs);
            }

            // Cleanup
            GameSave.get().Save();
            AndroidBridge.CleanupDisplay();
            AndroidBridge.ReleaseNativeWindow(nativeWindow);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Android] Errore nel game loop: {ex}");
        }
    }

    /// <summary>
    /// Gestione input touch → mappato a mouse per il gioco
    /// </summary>
    public override bool OnTouchEvent(MotionEvent e)
    {
        switch (e.Action)
        {
            case MotionEventActions.Down:
            case MotionEventActions.Move:
                AndroidBridge.SetTouchPosition((int)e.GetX(), (int)e.GetY(), true);
                break;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                AndroidBridge.SetTouchPosition((int)e.GetX(), (int)e.GetY(), false);
                break;
        }
        return true;
    }

    protected override void OnPause()
    {
        base.OnPause();
        // Salva quando l'app va in background
        try { GameSave.get().Save(); } catch { }
    }

    protected override void OnDestroy()
    {
        isRunning = false;
        base.OnDestroy();
    }
}

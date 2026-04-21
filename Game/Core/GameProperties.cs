using System;

namespace Plants;

internal static class GameProperties
{
    public static int windowWidth = 400;
    public static int windowHeight = 500;

    // Livello di scala UI 1..4. Livello 4 equivale al vecchio x2 (raddoppio). Livelli 2-3 intermedi.
    // Logica interna e coordinate GUI restano sempre a windowWidth x windowHeight.
    public const int MaxUiScaleLevel = 4;
    public static int uiScale = 1;

    public static float GetUiScaleMultiplier(int level)
    {
        switch (level)
        {
            case 2: return 4f / 3f;   // ~1.33
            case 3: return 5f / 3f;   // ~1.67
            case 4: return 2f;        // x2 assoluto (ex "x2")
            default: return 1f;
        }
    }

    public static float uiScaleMultiplier => GetUiScaleMultiplier(uiScale);
    public static int physicalWindowWidth => (int)(windowWidth * uiScaleMultiplier);
    public static int physicalWindowHeight => (int)(windowHeight * uiScaleMultiplier);

    public static int viewWidth => windowWidth / 4;
    public static int viewHeight => windowHeight / 4;

    public static int cameraWidth => (int)Rendering.camera.view.X;
    public static int cameraHeight => (int)Rendering.camera.view.Y;

    public static int groundPosition = 60;
    public static int groundHeight = 140;

    public const int MAX_GAME_ELEMENTS = 500;
    public const int MAX_PARTICLES = 3000;
    public const int MAX_SPLINE_POINTS = 1000;

    public const int TARGET_FPS = 60;
    public const int LOGIC_UPDATE_INTERVAL = 500;
    public const int PHASE_UPDATE_INTERVAL = 3600000;

#if DEBUG
    public static bool showDebugInfo = true;
    public static bool showBounds = false;
    public static bool showFPS = true;
#else
    public static bool showDebugInfo = false;
    public static bool showBounds = false;
    public static bool showFPS = false;
#endif
}
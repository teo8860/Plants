using System;

namespace Plants;

internal static class GameProperties
{
    public static int windowWidth = 400;
    public static int windowHeight = 500;

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
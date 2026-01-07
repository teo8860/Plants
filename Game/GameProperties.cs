using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

internal static class GameProperties
{
    // Dimensioni della finestra di windows
    public static int windowWidth = 400;
    public static int windowHeight = 500;

    // Dimensioni della view di rendering (quanto grandi i pixel)
    public static int viewWidth = GameProperties.windowWidth/4;
    public static int viewHeight = GameProperties.windowHeight/4;

    // Dimensioni della view della camera (considera anche lo zoom)
    public static int cameraWidth => (int)Rendering.camera.view.X;
    public static int cameraHeight => (int)Rendering.camera.view.Y;

    public static int groundPosition = 40;
}

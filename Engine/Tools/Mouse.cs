using System.Numerics;
using System.Runtime.InteropServices;

namespace Plants;

public static class MouseHelper
{
#if WINDOWS
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    public static Vector2 GetMousePosition()
    {
        GetCursorPos(out POINT point);
        return new Vector2(point.X, point.Y);
    }
#else
    public static Vector2 GetMousePosition()
    {
        return new Vector2(Raylib_CSharp.Interact.Input.GetMouseX(), Raylib_CSharp.Interact.Input.GetMouseY());
    }
#endif
}

using System;
using System.Numerics;

namespace Plants;

public static class ViewCulling
{

    public static bool IsYVisible(float y, float offsetY)
    {
        float adjustedY = y + offsetY;
        return adjustedY >= 0 && adjustedY <= GameProperties.viewHeight;
    }

    public static bool IsPointVisible(Vector2 point, float offsetY)
    {
        float adjustedY = point.Y + offsetY;
        float adjustedX = point.X;

        return adjustedX >= 0 &&
               adjustedX <= GameProperties.viewWidth &&
               adjustedY >= 0 &&
               adjustedY <= GameProperties.viewHeight;
    }

    public static bool IsRangeVisible(float minY, float maxY, float offsetY)
    {
        float adjustedMinY = minY + offsetY;
        float adjustedMaxY = maxY + offsetY;

        float viewTop = 0;
        float viewBottom = GameProperties.viewHeight;

        return adjustedMaxY >= viewTop && adjustedMinY <= viewBottom;
    }

    public static bool IsRectVisible(float x, float y, float width, float height, float offsetY)
    {
        float adjustedY = y + offsetY;

        float viewTop = 0;
        float viewBottom = GameProperties.viewHeight;
        float viewLeft = 0;
        float viewRight = GameProperties.viewWidth;

        return (x + width) >= viewLeft &&
               x <= viewRight &&
               (adjustedY + height) >= viewTop &&
               adjustedY <= viewBottom;
    }

    public static bool IsCircleVisible(Vector2 center, float radius, float offsetY)
    {
        float adjustedY = center.Y + offsetY;

        return (center.X + radius) >= 0 &&
               (center.X - radius) <= GameProperties.viewWidth &&
               (adjustedY + radius) >= 0 &&
               (adjustedY - radius) <= GameProperties.viewHeight;
    }

    public static bool IsLineVisible(Vector2 start, Vector2 end, float offsetY)
    {
        float minY = Math.Min(start.Y, end.Y);
        float maxY = Math.Max(start.Y, end.Y);
        float minX = Math.Min(start.X, end.X);
        float maxX = Math.Max(start.X, end.X);

        float adjustedMinY = minY + offsetY;
        float adjustedMaxY = maxY + offsetY;

        float viewTop = 0;
        float viewBottom = GameProperties.viewHeight;
        float viewLeft = 0;
        float viewRight = GameProperties.viewWidth;

        return maxX >= viewLeft &&
               minX <= viewRight &&
               adjustedMaxY >= viewTop &&
               adjustedMinY <= viewBottom;
    }
}
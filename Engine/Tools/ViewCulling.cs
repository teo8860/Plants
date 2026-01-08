using System;
using System.Numerics;

namespace Plants;

public static class ViewCulling
{
    // World coordinates: Y=0 at ground, Y positive upward
    // Screen coordinates: Y=0 at top, Y positive downward
    // cameraY = how high the camera is looking (0 = ground level)

    public static bool IsYVisible(float worldY, float cameraY)
    {
        float screenY = CoordinateHelper.ToScreenY(worldY, cameraY);
        return screenY >= 0 && screenY <= GameProperties.viewHeight;
    }

    public static bool IsPointVisible(Vector2 worldPoint, float cameraY)
    {
        float screenY = CoordinateHelper.ToScreenY(worldPoint.Y, cameraY);

        return worldPoint.X >= 0 &&
               worldPoint.X <= GameProperties.viewWidth &&
               screenY >= 0 &&
               screenY <= GameProperties.viewHeight;
    }

    public static bool IsRangeVisible(float minWorldY, float maxWorldY, float cameraY)
    {
        // In world coords, minY is lower (closer to ground), maxY is higher (plant top)
        // Convert both to screen coords
        float screenMinY = CoordinateHelper.ToScreenY(maxWorldY, cameraY); // max world Y = min screen Y
        float screenMaxY = CoordinateHelper.ToScreenY(minWorldY, cameraY); // min world Y = max screen Y

        // Check if range overlaps with visible area [0, viewHeight]
        return screenMaxY >= 0 && screenMinY <= GameProperties.viewHeight;
    }

    public static bool IsRectVisible(float x, float worldY, float width, float height, float cameraY)
    {
        // worldY is the bottom of the rect, height goes up
        float screenBottom = CoordinateHelper.ToScreenY(worldY, cameraY);
        float screenTop = CoordinateHelper.ToScreenY(worldY + height, cameraY);

        return (x + width) >= 0 &&
               x <= GameProperties.viewWidth &&
               screenBottom >= 0 &&
               screenTop <= GameProperties.viewHeight;
    }

    public static bool IsCircleVisible(Vector2 worldCenter, float radius, float cameraY)
    {
        float screenY = CoordinateHelper.ToScreenY(worldCenter.Y, cameraY);

        return (worldCenter.X + radius) >= 0 &&
               (worldCenter.X - radius) <= GameProperties.viewWidth &&
               (screenY + radius) >= 0 &&
               (screenY - radius) <= GameProperties.viewHeight;
    }

    public static bool IsLineVisible(Vector2 worldStart, Vector2 worldEnd, float cameraY)
    {
        float screenStartY = CoordinateHelper.ToScreenY(worldStart.Y, cameraY);
        float screenEndY = CoordinateHelper.ToScreenY(worldEnd.Y, cameraY);

        float minScreenY = Math.Min(screenStartY, screenEndY);
        float maxScreenY = Math.Max(screenStartY, screenEndY);
        float minX = Math.Min(worldStart.X, worldEnd.X);
        float maxX = Math.Max(worldStart.X, worldEnd.X);

        return maxX >= 0 &&
               minX <= GameProperties.viewWidth &&
               maxScreenY >= 0 &&
               minScreenY <= GameProperties.viewHeight;
    }
}
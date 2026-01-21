using System;
using System.Numerics;
using Engine.Tools;

namespace Plants;

public static class ViewCulling
{

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

    public static bool IsValueVisible(float value, float cameraY)
    {
        return value > cameraY && value < cameraY + Rendering.camera.view.Y;
    }

    public static bool IsRangeVisible(float minWorldY, float maxWorldY, float cameraY)
    {
        return minWorldY > cameraY && minWorldY < cameraY + GameProperties.cameraHeight && 
                maxWorldY > cameraY && maxWorldY < cameraY + GameProperties.cameraHeight;
    }

    public static bool IsRectVisible(float x, float worldY, float width, float height, float cameraY)
    {
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
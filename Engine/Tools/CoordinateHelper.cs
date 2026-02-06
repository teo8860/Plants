/// <summary>
using System.Numerics;

namespace Plants;

/// <summary>
/// Helper per coordinate e conversioni
/// </summary>
public static class CoordinateHelper
{
    /// <summary>
    /// Converte coordinate mondo a schermo (Y)
    /// </summary>
    public static float ToScreenY(float worldY, float cameraY) => worldY - cameraY;

    /// <summary>
    /// Converte coordinate mondo a schermo
    /// </summary>
    public static Vector2 ToScreen(Vector2 worldPos, Vector2 cameraPos) =>
        new(worldPos.X - cameraPos.X, worldPos.Y - cameraPos.Y);

    /// <summary>
    /// Converte coordinate schermo a mondo (Y)
    /// </summary>
    public static float ToWorldY(float screenY, float cameraY) => screenY + cameraY;

    /// <summary>
    /// Converte coordinate schermo a mondo
    /// </summary>
    public static Vector2 ToWorld(Vector2 screenPos, Vector2 cameraPos = default)
    {
        float wx = screenPos.X / GameProperties.windowWidth * GameProperties.cameraWidth;
        float wy = screenPos.Y / GameProperties.windowHeight * GameProperties.cameraHeight;

		return new(wx + cameraPos.X, GameProperties.cameraHeight - wy + cameraPos.Y);// + cameraPos.Y);
    }
}

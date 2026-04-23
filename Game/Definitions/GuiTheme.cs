using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Rendering;

namespace Plants;

// Palette GUI. Cambia CurrentTheme per temi diversi.
public static class GuiTheme
{
    public enum ThemeId { Default }

    public static ThemeId CurrentTheme = ThemeId.Default;

    // ---- Nav bar ----
    public static Color NavBarBg         => Pick(new Color(46, 30, 62, 255));
    public static Color NavBarOutline    => Pick(new Color(20, 12, 28, 255));
    public static Color TabInactiveBg    => Pick(new Color(79, 53, 96, 255));
    public static Color TabActiveBg      => Pick(new Color(232, 155, 110, 255));
    public static Color TabHoverBg       => Pick(new Color(102, 72, 122, 255));
    public static Color TabOutline       => Pick(new Color(20, 12, 28, 255));
    public static Color TabTextInactive  => Pick(new Color(255, 255, 255, 255));
    public static Color TabTextActive    => Pick(new Color(20, 12, 28, 255));

    // ---- Pannelli generici ----
    public static Color PanelBg          => Pick(new Color(46, 30, 62, 255));
    public static Color PanelOutline     => Pick(new Color(120, 90, 150, 255));
    public static Color PanelDivider     => Pick(new Color(120, 90, 150, 255));
    public static Color PanelText        => Pick(new Color(255, 255, 255, 255));
    public static Color PanelTextDim     => Pick(new Color(180, 170, 200, 255));

    // Tracce barre stat (bg interno)
    public static Color BarTrack         => Pick(new Color(20, 12, 28, 255));

    // ---- Stat colori (bar + pallino) ----
    public static Color StatSalute       => Pick(new Color(232, 91, 126, 255));
    public static Color StatIdratazione  => Pick(new Color(141, 185, 232, 255));
    public static Color StatEnergia      => Pick(new Color(232, 200, 91, 255));
    public static Color StatOssigeno     => Pick(new Color(141, 219, 232, 255));
    public static Color StatTemperatura  => Pick(new Color(232, 160, 91, 255));
    public static Color StatIdeale       => Pick(new Color(141, 232, 91, 255));

    // Hook temi futuri. Ora identity.
    private static Color Pick(Color def) => def;

    // ---- Text rendering comune ----
    public const int FontSize = 10;
    public const int FontSpacing = 2;

    // Disegna testo pixel (default font) con faux bold via double-draw offset 1px.
    public static void DrawText(string text, int x, int y, Color color)
    {
        DrawText(text, x, y, color, FontSize);
	}
     public static void DrawText(string text, int x, int y, Color color, int fontsize)
    {
        var f = Font.GetDefault();
        Graphics.DrawTextEx(f, text, new Vector2(x, y), fontsize, FontSpacing, color);
        Graphics.DrawTextEx(f, text, new Vector2(x + 1, y), fontsize, FontSpacing, color);
    }

    public static int MeasureText(string text)
    {
        return (int)TextManager.MeasureTextEx(Font.GetDefault(), text, FontSize, FontSpacing).X + 1;
    }
}

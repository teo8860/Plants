using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;

namespace Plants;

public class GuiStatsPanel : GameElement
{
    private int x, y;
    private bool expanded = true;

    public GuiStatsPanel(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.guiLayer = true;
    }

    public override void Draw()
    {
        var stats = Game.pianta.Stats;

        Color bg = Color.Black;
        bg.A = 180;
        int panelHeight = expanded ? 145 : 25;
        Graphics.DrawRectangleRounded(
            new Rectangle(x - 5, y - 5, 140, panelHeight),
            0.2f, 16, bg
        );

        Graphics.DrawText($"[{WorldManager.GetCurrentWorld()}]", x, y, 12, Color.Gold);

        if (!expanded) return;

        int yOff = y + 20;
        int lineH = 15;

        DrawStatBar("Salute", stats.Salute, Color.Red, ref yOff, lineH);
        DrawStatBar("Idrat.", stats.Idratazione, Color.Blue, ref yOff, lineH);
        DrawStatBar("Energia", stats.Metabolismo, Color.Yellow, ref yOff, lineH);
        DrawStatBar("O2", stats.Ossigeno, Color.SkyBlue, ref yOff, lineH);

        yOff += 5;

    }

    private void DrawStatBar(string label, float value, Color color, ref int y, int h)
    {
        Graphics.DrawText(label, x, y, 9, Color.LightGray);

        int barW = 60;
        Graphics.DrawRectangle(x + 45, y + 2, barW, 8, Color.DarkGray);
        Graphics.DrawRectangle(x + 45, y + 2, (int)(barW * value), 8, color);

        Graphics.DrawText($"{value:P0}", x + 110, y, 9, Color.White);
        y += h;
    }

    private void DrawStatLine(string label, string value, ref int y, int h)
    {
        Graphics.DrawText(label, x, y, 9, Color.LightGray);
        Graphics.DrawText(value, x + 70, y, 9, Color.White);
        y += h;
    }
}
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
        int panelHeight = expanded ? 160 : 25;
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

        Color tempColor = GetTemperatureColor(stats.Temperatura);
        DrawTemperatureLine(stats.Temperatura, tempColor, ref yOff, lineH);

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

    private void DrawTemperatureLine(float temp, Color color, ref int y, int h)
    {
        Graphics.DrawText("Temp.", x, y, 9, Color.LightGray);
        Graphics.DrawText($"{temp:F1}°C", x + 45, y, 9, color);

        string desc = GetTemperatureDescription(temp);
        Graphics.DrawText(desc, x + 90, y, 9, color);
        y += h;
    }

    private Color GetTemperatureColor(float temp)
    {
        if (temp <= 0)
            return new Color(100, 150, 255, 255); 
        if (temp < 10)
            return new Color(150, 200, 255, 255); 
        if (temp < 18)
            return new Color(200, 230, 255, 255);
        if (temp <= 25)
            return new Color(100, 255, 100, 255);  
        if (temp <= 30)
            return new Color(255, 255, 100, 255);  
        if (temp < 38)
            return new Color(255, 180, 100, 255); 
        return new Color(255, 100, 100, 255);      
    }

    private string GetTemperatureDescription(float temp)
    {
        if (temp <= 0) return "Gelido";
        if (temp < 10) return "Freddo";
        if (temp < 15) return "Fresco";
        if (temp < 18) return "Mite";
        if (temp <= 25) return "Ideale";
        if (temp <= 30) return "Caldo";
        if (temp < 38) return "Torrido";
        return "Estremo";
    }

    private void DrawStatLine(string label, string value, ref int y, int h)
    {
        Graphics.DrawText(label, x, y, 9, Color.LightGray);
        Graphics.DrawText(value, x + 70, y, 9, Color.White);
        y += h;
    }
}
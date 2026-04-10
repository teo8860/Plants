using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;

namespace Plants;

public class Obj_GuiStatsPanel : GameElement
{
    private int x, y;
    private bool expanded = true;

    // Tooltip oggetto
    private string tooltipItemId = null;

    public Obj_GuiStatsPanel(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.guiLayer = true;
    }

    public override void Update()
    {
        tooltipItemId = null;

        if (!expanded) return;
        if (Game.pianta == null || Game.pianta.equippedItemIds == null) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        int iconSize = 18;
        int iconSpacing = 4;
        int iconsY = y + 100;
        int iconsX = x + 2;

        for (int i = 0; i < Game.pianta.equippedItemIds.Count; i++)
        {
            string id = Game.pianta.equippedItemIds[i];
            if (string.IsNullOrEmpty(id)) continue;

            int ix = iconsX + i * (iconSize + iconSpacing);
            if (mx >= ix && mx <= ix + iconSize && my >= iconsY && my <= iconsY + iconSize)
            {
                tooltipItemId = id;
                break;
            }
        }
    }

    public override void Draw()
    {
        var stats = Game.pianta.Stats;

        Color bg = Color.Black;
        bg.A = 180;
        int panelHeight = expanded ? 130 : 25;
        Graphics.DrawRectangleRounded(
            new Rectangle(x - 5, y - 5, 140, panelHeight),
            0.2f, 16, bg
        );

        int stage = WorldManager.GetCurrentStage();
        float diff = WorldManager.GetDifficultyMultiplier(stage);
        Graphics.DrawText($"Stage {stage} x{diff:F2}", x + 5, y + 2, 9, new Color(192, 192, 192, 255));

        if (!expanded) return;

        int yOff = y + 16;
        int lineH = 15;

        DrawStatBar("Salute", stats.Salute, Color.Red, ref yOff, lineH);
        DrawStatBar("Idrat.", stats.Idratazione, Color.Blue, ref yOff, lineH);
        DrawStatBar("Energia", stats.Metabolismo, Color.Yellow, ref yOff, lineH);
        DrawStatBar("O2", stats.Ossigeno, Color.SkyBlue, ref yOff, lineH);

        Color tempColor = GetTemperatureColor(stats.Temperatura);
        DrawTemperatureLine(stats.Temperatura, tempColor, ref yOff, lineH);

        // Icone oggetti equipaggiati
        DrawEquippedItems();

        // Tooltip (disegnato per ultimo, sopra tutto)
        DrawItemTooltip();

        yOff += 5;
    }

    private void DrawEquippedItems()
    {
        if (Game.pianta == null || Game.pianta.equippedItemIds == null) return;

        int iconSize = 18;
        int iconSpacing = 4;
        int iconsY = y + 100;
        int iconsX = x + 2;

        bool hasAny = false;
        for (int i = 0; i < Game.pianta.equippedItemIds.Count; i++)
        {
            string id = Game.pianta.equippedItemIds[i];
            if (string.IsNullOrEmpty(id)) continue;
            hasAny = true;

            var def = ItemRegistry.Get(id);
            if (def == null) continue;

            int ix = iconsX + i * (iconSize + iconSpacing);

            Color iconBg = tooltipItemId == id
                ? new Color(100, 180, 255, 200)
                : new Color(60, 60, 80, 200);
            Color iconBorder = tooltipItemId == id
                ? new Color(140, 200, 255, 255)
                : new Color(90, 90, 110, 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(ix, iconsY, iconSize, iconSize),
                0.25f, 4, iconBg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(ix, iconsY, iconSize, iconSize),
                0.25f, 4, 1, iconBorder);

            string initial = def.Name.Length > 0 ? def.Name.Substring(0, 1) : "?";
            Graphics.DrawText(initial, ix + iconSize / 2 - 3, iconsY + iconSize / 2 - 5, 10, new Color(240, 240, 255, 255));
        }

        if (!hasAny)
        {
            Graphics.DrawText("No oggetti", iconsX, iconsY + 3, 7, new Color(120, 120, 140, 180));
        }
    }

    private void DrawItemTooltip()
    {
        if (string.IsNullOrEmpty(tooltipItemId)) return;

        var def = ItemRegistry.Get(tooltipItemId);
        if (def == null) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        int tw = 140;
        int th = 80;
        int tx = mx - tw - 8;
        int ty = my - th / 2;

        // Clamp sullo schermo
        if (tx < 2) tx = mx + 12;
        if (ty < 2) ty = 2;
        if (ty + th > Rendering.camera.screenHeight - 2)
            ty = Rendering.camera.screenHeight - th - 2;

        Color tooltipBg = new Color(25, 25, 38, 245);
        Color tooltipBorder = new Color(80, 80, 110, 255);
        Color textAccent = new Color(100, 180, 255, 255);
        Color textDim = new Color(170, 170, 195, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(tx - 2, ty - 2, tw + 4, th + 4),
            0.1f, 6, tooltipBorder);
        Graphics.DrawRectangleRounded(
            new Rectangle(tx, ty, tw, th),
            0.1f, 6, tooltipBg);

        Graphics.DrawText(def.Name, tx + 6, ty + 5, 10, textAccent);

        string catLabel = def.Category switch
        {
            ItemCategory.Cosmetico => "Cosmetico",
            ItemCategory.Consumabile => "Consumabile",
            _ => "Equipaggiabile"
        };
        Graphics.DrawText(catLabel, tx + 6, ty + 18, 7, textDim);

        Graphics.DrawLine(tx + 6, ty + 27, tx + tw - 6, ty + 27, tooltipBorder);

        DrawWrappedText(def.Description, tx + 6, ty + 31, tw - 12, 7, textDim);
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

    private void DrawWrappedText(string text, int x, int y, int maxWidth, int fontSize, Color color)
    {
        int charWidth = Math.Max(1, fontSize - 2);
        int maxChars = Math.Max(1, maxWidth / charWidth);
        string[] words = text.Split(' ');
        string line = "";
        int lineY = y;

        foreach (var word in words)
        {
            if ((line + " " + word).Trim().Length > maxChars)
            {
                Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
                lineY += fontSize + 3;
                line = word;
            }
            else
            {
                line = line.Length > 0 ? line + " " + word : word;
            }
        }

        if (line.Trim().Length > 0)
            Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
    }
}

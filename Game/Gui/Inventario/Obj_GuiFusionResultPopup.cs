using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

public class Obj_GuiFusionResultPopup : GameElement
{
    private bool isVisible = false;
    private Seed parent1, parent2, fused;
    private Obj_Seed visualNew, visualP1, visualP2;

    private float pulse = 0f;

    // Colori
    private readonly Color overlay = new Color(0, 0, 0, 200);
    private readonly Color panelBg = new Color(82, 54, 35, 250);
    private readonly Color panelBorder = new Color(62, 39, 25, 255);
    private readonly Color headerBg = new Color(62, 39, 25, 255);
    private readonly Color headerText = new Color(255, 230, 150, 255);
    private readonly Color textColor = new Color(240, 240, 240, 255);
    private readonly Color dimText = new Color(170, 170, 185, 255);
    private readonly Color sectionDivider = new Color(62, 39, 25, 180);
    private readonly Color barBg = new Color(41, 26, 17, 220);
    private readonly Color buttonColor = new Color(101, 67, 43, 255);
    private readonly Color buttonHover = new Color(139, 90, 55, 255);
    private readonly Color betterColor = new Color(100, 220, 100, 255);
    private readonly Color midColor = new Color(230, 200, 90, 255);
    private readonly Color worseColor = new Color(220, 100, 100, 255);
    private readonly Color tickColor = new Color(230, 230, 230, 200);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    private const int PANEL_W = 360;
    private const int PANEL_H = 400;

    public bool IsVisible => isVisible;

    public Obj_GuiFusionResultPopup()
    {
        this.guiLayer = true;
        this.depth = -3500;
        this.persistent = true;
    }

    public void Show(Seed parent1, Seed parent2, Seed fused)
    {
        if (parent1 == null || parent2 == null || fused == null) return;

        this.parent1 = parent1;
        this.parent2 = parent2;
        this.fused = fused;
        isVisible = true;
        pulse = 0f;

        CreateVisualSeeds();
    }

    public void Hide()
    {
        isVisible = false;
        DestroyVisualSeeds();
    }

    private void CreateVisualSeeds()
    {
        DestroyVisualSeeds();

        visualNew = new Obj_Seed
        {
            guiLayer = true, persistent = true, roomId = uint.MaxValue,
            active = false, dati = fused, color = fused.color, scale = 3.2f
        };

        visualP1 = new Obj_Seed
        {
            guiLayer = true, persistent = true, roomId = uint.MaxValue,
            active = false, dati = parent1, color = parent1.color, scale = 1.3f
        };

        visualP2 = new Obj_Seed
        {
            guiLayer = true, persistent = true, roomId = uint.MaxValue,
            active = false, dati = parent2, color = parent2.color, scale = 1.3f
        };
    }

    private void DestroyVisualSeeds()
    {
        if (visualNew != null) { visualNew.Destroy(); visualNew = null; }
        if (visualP1 != null)  { visualP1.Destroy();  visualP1 = null; }
        if (visualP2 != null)  { visualP2.Destroy();  visualP2 = null; }
    }

    public override void Update()
    {
        if (!isVisible) return;
        InputGate.ConsumeMouse();

        pulse += Time.GetFrameTime() * 3f;

        if (Input.IsKeyPressed(KeyboardKey.Escape)
         || Input.IsKeyPressed(KeyboardKey.Enter)
         || Input.IsKeyPressed(KeyboardKey.Space))
        {
            Hide();
        }
    }

    public override void Draw()
    {
        if (!isVisible) return;

        Hud.Overlay(overlay);

        var (px, py) = ((sw - PANEL_W) / 2, (sh - PANEL_H) / 2);

        Hud.Panel(px, py, PANEL_W, PANEL_H, new Hud.PanelOptions
        {
            Bg = panelBg, Border = panelBorder, Roundness = 0.08f
        });

        Hud.Panel(px, py, PANEL_W, 24, new Hud.PanelOptions
        {
            Bg = headerBg, Roundness = 0.25f, BorderThickness = 0
        });

        Hud.Label(px, py + 6, PANEL_W, "FUSIONE COMPLETATA!", new Hud.LabelOptions
        {
            FontSize = 14, Color = headerText, Align = TextAlign.Center
        });

        // === Nuovo seme ===
        int iconCx = px + PANEL_W / 2;
        int iconCy = py + 66;
        Color rarityColor = SeedDefinitions.GetRarityColor(fused.rarity);

        float glow = 0.5f + MathF.Sin(pulse) * 0.5f;
        for (int i = 3; i >= 0; i--)
        {
            byte a = (byte)(25 + glow * 25);
            int r = 26 + i * 3 + (int)(glow * 3);
            Graphics.DrawCircle(iconCx, iconCy, r,
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, a));
        }

        if (visualNew != null)
        {
            visualNew.position = new Vector2(iconCx, iconCy);
            visualNew.Draw();
        }

        Hud.Label(px, py + 100, PANEL_W, SeedDefinitions.GetSeedName(fused.type), new Hud.LabelOptions
        {
            FontSize = 12, Color = textColor, Align = TextAlign.Center
        });

        Hud.Label(px, py + 116, PANEL_W, SeedDefinitions.GetRarityName(fused.rarity), new Hud.LabelOptions
        {
            FontSize = 10, Color = rarityColor, Align = TextAlign.Center
        });

        Hud.Divider(px + 20, py + 136, PANEL_W - 40, sectionDivider);

        // === Genitori ===
        Hud.Label(px + 20, py + 142, "Fusione di:", new Hud.LabelOptions
        {
            FontSize = 10, Color = dimText
        });

        int parentColW = (PANEL_W - 40) / 2;
        DrawParentCell(px + 20, py + 156, parentColW, parent1, visualP1);
        DrawParentCell(px + 20 + parentColW, py + 156, parentColW, parent2, visualP2);

        Hud.Divider(px + 20, py + 190, PANEL_W - 40, sectionDivider);

        // === Statistiche confronto ===
        Hud.Label(px + 20, py + 196, "Statistiche:", new Hud.LabelOptions
        {
            FontSize = 10, Color = dimText
        });
        DrawStatsComparison(px + 20, py + 210, PANEL_W - 40);

        // === Bottone ===
        int btnW = 130;
        int btnH = 26;
        int btnX = px + (PANEL_W - btnW) / 2;
        int btnY = py + PANEL_H - btnH - 10;

        Hud.Button(btnX, btnY, btnW, btnH, "Fantastico!", () => Hide(), new Hud.ButtonOptions
        {
            Bg = buttonColor, HoverBg = buttonHover,
            Border = panelBorder, TextColor = textColor,
            FontSize = 12, Roundness = 0.3f, BorderThickness = 2
        });
    }

    private void DrawParentCell(int x, int y, int width, Seed seed, Obj_Seed visual)
    {
        int iconCx = x + 14;
        int iconCy = y + 14;

        if (visual != null)
        {
            visual.position = new Vector2(iconCx, iconCy);
            visual.Draw();
        }

        int textX = x + 28;
        string name = SeedDefinitions.GetSeedName(seed.type);
        int maxChars = Math.Max(5, (width - 30) / 6);
        if (name.Length > maxChars) name = name.Substring(0, maxChars - 1) + ".";
        GuiTheme.DrawText(name, textX, y + 2, 10, textColor);

        string rar = SeedDefinitions.GetRarityName(seed.rarity);
        GuiTheme.DrawText(rar, textX, y + 14, 10, SeedDefinitions.GetRarityColor(seed.rarity));
    }

    private struct StatInfo
    {
        public string Label;
        public float P1, P2, New;
        public float Min, Max;
    }

    private void DrawStatsComparison(int x, int y, int width)
    {
        var stats = new StatInfo[]
        {
            new() { Label = "VIT", P1 = parent1.stats.vitalita,             P2 = parent2.stats.vitalita,             New = fused.stats.vitalita,             Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "IDR", P1 = parent1.stats.idratazione,          P2 = parent2.stats.idratazione,          New = fused.stats.idratazione,          Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "MET", P1 = parent1.stats.metabolismo,          P2 = parent2.stats.metabolismo,          New = fused.stats.metabolismo,          Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "VEG", P1 = parent1.stats.vegetazione,          P2 = parent2.stats.vegetazione,          New = fused.stats.vegetazione,          Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "FRD", P1 = parent1.stats.resistenzaFreddo,     P2 = parent2.stats.resistenzaFreddo,     New = fused.stats.resistenzaFreddo,     Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "CLD", P1 = parent1.stats.resistenzaCaldo,      P2 = parent2.stats.resistenzaCaldo,      New = fused.stats.resistenzaCaldo,      Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "PAR", P1 = parent1.stats.resistenzaParassiti,  P2 = parent2.stats.resistenzaParassiti,  New = fused.stats.resistenzaParassiti,  Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
            new() { Label = "VUO", P1 = parent1.stats.resistenzaVuoto,      P2 = parent2.stats.resistenzaVuoto,      New = fused.stats.resistenzaVuoto,      Min = SeedStatScaling.StatMin, Max = SeedStatScaling.StatMax },
        };

        int rowH = 14;
        int labelW = 26;
        int parentBlockW = 82;
        int arrowW = 14;
        int newValW = 36;
        int barX = x + labelW + parentBlockW + arrowW + newValW;
        int barW = width - (labelW + parentBlockW + arrowW + newValW);
        int barH = 6;

        for (int i = 0; i < stats.Length; i++)
        {
            var s = stats[i];
            int ry = y + i * rowH;

            GuiTheme.DrawText(s.Label, x, ry + 2, 10, dimText);

            string p1 = ((int)Math.Round(s.P1)).ToString();
            string p2 = ((int)Math.Round(s.P2)).ToString();
            int p1W = GuiTheme.MeasureText(p1, 10);
            int sepX = x + labelW + 30;
            GuiTheme.DrawText(p1, sepX - 4 - p1W, ry + 2, 10, dimText);
            GuiTheme.DrawText("|", sepX, ry + 2, 10, dimText);
            GuiTheme.DrawText(p2, sepX + 8, ry + 2, 10, dimText);

            GuiTheme.DrawText("->", x + labelW + parentBlockW, ry + 2, 10, dimText);

            float maxP = Math.Max(s.P1, s.P2);
            float minP = Math.Min(s.P1, s.P2);
            Color newColor;
            if (s.New > maxP + 0.5f)       newColor = betterColor;
            else if (s.New >= maxP - 0.5f) newColor = textColor;
            else if (s.New > minP + 0.5f)  newColor = midColor;
            else                           newColor = worseColor;

            string newStr = ((int)Math.Round(s.New)).ToString();
            GuiTheme.DrawText(newStr, x + labelW + parentBlockW + arrowW, ry + 2, 10, newColor);

            int bY = ry + 3;
            Graphics.DrawRectangleRounded(new Rectangle(barX, bY, barW, barH), 0.5f, 4, barBg);

            float newRatio = Math.Clamp((s.New - s.Min) / (s.Max - s.Min), 0f, 1f);
            int fillW = Math.Max(1, (int)(barW * newRatio));
            Graphics.DrawRectangleRounded(new Rectangle(barX, bY, fillW, barH), 0.5f, 4, newColor);

            DrawParentTick(barX, bY, barW, barH, s.P1, s.Min, s.Max);
            DrawParentTick(barX, bY, barW, barH, s.P2, s.Min, s.Max);
        }
    }

    private void DrawParentTick(int barX, int barY, int barW, int barH, float value, float min, float max)
    {
        float ratio = Math.Clamp((value - min) / (max - min), 0f, 1f);
        int tickX = barX + (int)(barW * ratio);
        Graphics.DrawRectangle(tickX - 1, barY - 1, 2, barH + 2, tickColor);
    }
}

using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

/// <summary>
/// Popup mostrato dopo una fusione riuscita. Mostra il nuovo seme ottenuto
/// con icona, nome, rarita' e una tabella di statistiche che confronta i due
/// semi genitori con il risultato (tick sui valori genitori sovrapposti alla
/// barra del nuovo valore).
/// </summary>
public class Obj_GuiFusionResultPopup : GameElement
{
    private bool isVisible = false;
    private Seed parent1, parent2, fused;
    private Obj_Seed visualNew, visualP1, visualP2;

    private bool buttonHovered = false;
    private float pulse = 0f;

    // Colori
    private readonly Color overlay = new Color(0, 0, 0, 200);
    private readonly Color panelBg = new Color(30, 35, 45, 250);
    private readonly Color panelBorder = new Color(150, 150, 255, 255);
    private readonly Color headerBg = new Color(60, 70, 100, 255);
    private readonly Color headerText = new Color(255, 230, 150, 255);
    private readonly Color textColor = new Color(240, 240, 240, 255);
    private readonly Color dimText = new Color(170, 170, 185, 255);
    private readonly Color sectionDivider = new Color(80, 80, 120, 180);
    private readonly Color barBg = new Color(20, 25, 20, 220);
    private readonly Color buttonColor = new Color(90, 130, 200, 255);
    private readonly Color buttonHover = new Color(120, 170, 240, 255);
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
        this.depth = -3500; // sopra a tutti gli altri popup
        this.persistent = true;
    }

    public void Show(Seed parent1, Seed parent2, Seed fused)
    {
        if (parent1 == null || parent2 == null || fused == null) return;

        this.parent1 = parent1;
        this.parent2 = parent2;
        this.fused = fused;
        isVisible = true;
        buttonHovered = false;
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
            guiLayer = true,
            persistent = true,
            roomId = uint.MaxValue,
            active = false,
            dati = fused,
            color = fused.color,
            scale = 3.2f
        };

        visualP1 = new Obj_Seed
        {
            guiLayer = true,
            persistent = true,
            roomId = uint.MaxValue,
            active = false,
            dati = parent1,
            color = parent1.color,
            scale = 1.3f
        };

        visualP2 = new Obj_Seed
        {
            guiLayer = true,
            persistent = true,
            roomId = uint.MaxValue,
            active = false,
            dati = parent2,
            color = parent2.color,
            scale = 1.3f
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

        pulse += Time.GetFrameTime() * 3f;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        var btn = GetButtonRect();
        buttonHovered = mx >= btn.X && mx <= btn.X + btn.Width
                     && my >= btn.Y && my <= btn.Y + btn.Height;

        if (Input.IsKeyPressed(KeyboardKey.Escape)
         || Input.IsKeyPressed(KeyboardKey.Enter)
         || Input.IsKeyPressed(KeyboardKey.Space))
        {
            Hide();
            return;
        }

        if (Input.IsMouseButtonPressed(MouseButton.Left) && buttonHovered)
        {
            Hide();
        }
    }

    private (int x, int y) GetPanelOrigin()
    {
        return ((sw - PANEL_W) / 2, (sh - PANEL_H) / 2);
    }

    private Rectangle GetButtonRect()
    {
        var (px, py) = GetPanelOrigin();
        int btnW = 130;
        int btnH = 26;
        int btnX = px + (PANEL_W - btnW) / 2;
        int btnY = py + PANEL_H - btnH - 10;
        return new Rectangle(btnX, btnY, btnW, btnH);
    }

    public override void Draw()
    {
        if (!isVisible) return;

        // Overlay
        Graphics.DrawRectangle(0, 0, sw, sh, overlay);

        var (px, py) = GetPanelOrigin();

        // Pannello
        Graphics.DrawRectangleRounded(new Rectangle(px, py, PANEL_W, PANEL_H), 0.08f, 10, panelBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, PANEL_W, PANEL_H), 0.08f, 10, 2, panelBorder);

        // Header
        Graphics.DrawRectangleRounded(new Rectangle(px, py, PANEL_W, 24), 0.25f, 8, headerBg);
        string title = "FUSIONE COMPLETATA!";
        int titleW = title.Length * 7;
        Graphics.DrawText(title, px + (PANEL_W - titleW) / 2, py + 6, 14, headerText);

        // === Nuovo seme ===
        int iconCx = px + PANEL_W / 2;
        int iconCy = py + 66;
        Color rarityColor = SeedDefinitions.GetRarityColor(fused.rarity);

        // Alone pulsante intorno al seme
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

        // Nome
        string name = SeedDefinitions.GetSeedName(fused.type);
        int nameW = name.Length * 6;
        Graphics.DrawText(name, px + (PANEL_W - nameW) / 2, py + 100, 12, textColor);

        // Rarita
        string rar = SeedDefinitions.GetRarityName(fused.rarity);
        int rarW = rar.Length * 5;
        Graphics.DrawText(rar, px + (PANEL_W - rarW) / 2, py + 116, 10, rarityColor);

        // Separatore
        Graphics.DrawRectangle(px + 20, py + 136, PANEL_W - 40, 1, sectionDivider);

        // === Genitori ===
        Graphics.DrawText("Fusione di:", px + 20, py + 142, 9, dimText);

        int parentColW = (PANEL_W - 40) / 2;
        DrawParentCell(px + 20,                    py + 156, parentColW, parent1, visualP1);
        DrawParentCell(px + 20 + parentColW,       py + 156, parentColW, parent2, visualP2);

        // Separatore
        Graphics.DrawRectangle(px + 20, py + 190, PANEL_W - 40, 1, sectionDivider);

        // === Statistiche confronto ===
        Graphics.DrawText("Statistiche:", px + 20, py + 196, 9, dimText);
        DrawStatsComparison(px + 20, py + 210, PANEL_W - 40);

        // === Bottone ===
        var btn = GetButtonRect();
        Color bc = buttonHovered ? buttonHover : buttonColor;
        Graphics.DrawRectangleRounded(btn, 0.3f, 8, bc);
        Graphics.DrawRectangleRoundedLines(btn, 0.3f, 8, 2, panelBorder);

        string btnText = "Fantastico!";
        int btnTextW = btnText.Length * 6;
        Graphics.DrawText(btnText, (int)btn.X + ((int)btn.Width - btnTextW) / 2,
            (int)btn.Y + 7, 12, textColor);
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
        // Tronca il nome se troppo lungo per la colonna
        int maxChars = Math.Max(5, (width - 30) / 5);
        if (name.Length > maxChars) name = name.Substring(0, maxChars - 1) + ".";
        Graphics.DrawText(name, textX, y + 2, 9, textColor);

        string rar = SeedDefinitions.GetRarityName(seed.rarity);
        Graphics.DrawText(rar, textX, y + 14, 8, SeedDefinitions.GetRarityColor(seed.rarity));
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
        int labelW = 22;
        int parentBlockW = 78;   // "1.23 | 1.45"
        int arrowW = 12;
        int newValW = 34;
        int barX = x + labelW + parentBlockW + arrowW + newValW;
        int barW = width - (labelW + parentBlockW + arrowW + newValW);
        int barH = 6;

        for (int i = 0; i < stats.Length; i++)
        {
            var s = stats[i];
            int ry = y + i * rowH;

            // Label
            Graphics.DrawText(s.Label, x, ry + 2, 9, dimText);

            // Genitori (piccoli, grigi) — scala 0-99 intera.
            // p1 allineato a destra verso il separatore, p2 a sinistra del separatore.
            string p1 = ((int)Math.Round(s.P1)).ToString();
            string p2 = ((int)Math.Round(s.P2)).ToString();
            int p1W = p1.Length * 5;
            int sepX = x + labelW + 28;
            Graphics.DrawText(p1, sepX - 4 - p1W, ry + 3, 8, dimText);
            Graphics.DrawText("|", sepX, ry + 3, 8, dimText);
            Graphics.DrawText(p2, sepX + 8, ry + 3, 8, dimText);

            // Freccia
            Graphics.DrawText("->", x + labelW + parentBlockW, ry + 3, 8, dimText);

            // Valore nuovo (colorato):
            //   verde  = sopra max genitori
            //   bianco = ≈ max genitori
            //   giallo = tra min e max
            //   rosso  = sotto min genitori
            float maxP = Math.Max(s.P1, s.P2);
            float minP = Math.Min(s.P1, s.P2);
            Color newColor;
            if (s.New > maxP + 0.5f)       newColor = betterColor;
            else if (s.New >= maxP - 0.5f) newColor = textColor;
            else if (s.New > minP + 0.5f)  newColor = midColor;
            else                           newColor = worseColor;

            string newStr = ((int)Math.Round(s.New)).ToString();
            Graphics.DrawText(newStr, x + labelW + parentBlockW + arrowW, ry + 2, 9, newColor);

            // Barra con marker dei genitori sovrapposti
            int bY = ry + 3;
            Graphics.DrawRectangleRounded(new Rectangle(barX, bY, barW, barH), 0.5f, 4, barBg);

            float newRatio = Math.Clamp((s.New - s.Min) / (s.Max - s.Min), 0f, 1f);
            int fillW = Math.Max(1, (int)(barW * newRatio));
            Graphics.DrawRectangleRounded(new Rectangle(barX, bY, fillW, barH), 0.5f, 4, newColor);

            // Tick sui valori genitori (sovrapposti)
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

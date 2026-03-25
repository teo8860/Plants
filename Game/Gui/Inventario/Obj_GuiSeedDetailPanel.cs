using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiSeedDetailPanel : GameElement
{
    private bool isOpen = true;
    private float slideProgress = 0f;
    private const float AnimSpeed = 8f;

    public int panelWidth = 170;
    private int selectedSeedIndex = -1;

    private Color panelColor    = new Color(82,  54,  35,  245);
    private Color panelBorder   = new Color(62,  39,  25,  255);
    private Color buttonColor   = new Color(101, 67,  43,  255);
    private Color buttonHover   = new Color(139, 90,  55,  255);
    private Color buttonActive  = new Color(100, 180, 255, 255);
    private Color buttonBorder  = new Color(62,  39,  25,  255);
    private Color textColor     = new Color(245, 235, 220, 255);
    private Color dimTextColor  = new Color(170, 148, 118, 255);

    private readonly string[] buttonLabels = { "Unisci", "Scarta", "Migliora" };
    private int hoveredButton = -1;

    public Action<int, string> OnButtonClicked;

    // Definizioni statistiche: (etichetta, getter, min, max, lowerBetter)
    // lowerBetter = true solo per Idratazione (meno acqua consumata = meglio)
    private static readonly (string label, Func<SeedStats, float> get, float min, float max, bool lowerBetter)[] StatDefs =
    {
        ("VIT", s => s.vitalita,             0.5f, 2.5f, false),
        ("IDR", s => s.idratazione,          0.3f, 2.0f, true ),
        ("MET", s => s.metabolismo,          0.5f, 2.5f, false),
        ("VEG", s => s.vegetazione,          0.5f, 2.5f, false),
        ("R.F", s => s.resistenzaFreddo,    -0.5f, 1.0f, false),
        ("R.C", s => s.resistenzaCaldo,     -0.5f, 1.0f, false),
        ("R.P", s => s.resistenzaParassiti, -0.5f, 1.0f, false),
        ("R.V", s => s.resistenzaVuoto,     -0.3f, 1.0f, false),
    };

    public Obj_GuiSeedDetailPanel() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -100;
    }

    public void Open(int seedIndex)
    {
        selectedSeedIndex = seedIndex;
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
    }

    public void Toggle(int seedIndex)
    {
        if (isOpen && selectedSeedIndex == seedIndex)
            Close();
        else
            Open(seedIndex);
    }

    public bool IsOpen => isOpen;
    public float SlideProgress => slideProgress;
    public int PanelWidth => panelWidth;

    public override void Update()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
        {
            Close();
            return;
        }

        float target = isOpen ? 1f : 0f;
        slideProgress += (target - slideProgress) * Time.GetFrameTime() * AnimSpeed;
        slideProgress = Math.Clamp(slideProgress, 0f, 1f);

        if (slideProgress < 0.01f)
        {
            hoveredButton = -1;
            return;
        }

        int panelX      = Rendering.camera.screenWidth - (int)(panelWidth * slideProgress);
        int screenH     = Rendering.camera.screenHeight;
        int mx          = Input.GetMouseX();
        int my          = Input.GetMouseY();
        bool clicked    = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredButton = -1;

        int btnH        = 30;
        int btnSpacing  = 10;
        int btnMargin   = 10;
        int totalH      = buttonLabels.Length * btnH + (buttonLabels.Length - 1) * btnSpacing;
        int buttonsY    = screenH - totalH - btnMargin - 10;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + btnMargin;
            int btnY = buttonsY + i * (btnH + btnSpacing);
            int btnW = panelWidth - btnMargin * 2;

            if (mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH)
            {
                hoveredButton = i;
                if (clicked) HandleButtonClick(i);
                break;
            }
        }
    }

    private void HandleButtonClick(int idx)
    {
        switch (buttonLabels[idx])
        {
            case "Unisci":   HandleFusion();  break;
            case "Scarta":   HandleDiscard(); break;
            case "Migliora": HandleUpgrade(); break;
        }
        OnButtonClicked?.Invoke(selectedSeedIndex, buttonLabels[idx]);
    }

    private void HandleFusion()
    {
        var fm = SeedFusionManager.Get();
        if (fm.IsFusionMode)
        {
            if (fm.CanFuse)
            {
                if (fm.PerformFusion() != null) Game.inventoryGrid?.Populate();
            }
            else
            {
                fm.StopFusionMode();
            }
        }
        else
        {
            fm.StartFusionMode();
        }
    }

    private void HandleDiscard()
    {
        if (selectedSeedIndex < 0) return;
        var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
        if (seed == null) return;
        Inventario.get().RemoveSeed(seed);
        Game.inventoryGrid?.Populate();
        selectedSeedIndex = -1;
    }

    private void HandleUpgrade()
    {
        if (selectedSeedIndex < 0) return;
        var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
        if (seed != null) Game.seedUpgradePanel?.OpenForSeed(seed, selectedSeedIndex);
    }

    public override void Draw()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        if (slideProgress < 0.01f) return;

        int panelX  = Rendering.camera.screenWidth - (int)(panelWidth * slideProgress);
        int screenH = Rendering.camera.screenHeight;

        // Sfondo pannello
        Graphics.DrawRectangle(panelX, 0, panelWidth, screenH, panelColor);
        Graphics.DrawLine(panelX, 0, panelX, screenH, panelBorder);

        Seed seed = selectedSeedIndex >= 0 ? Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex) : null;

        DrawSeedInfo(panelX, 10, seed);
        DrawStats(panelX, 92, seed);

        if (SeedFusionManager.Get().IsFusionMode)
            DrawFusionInfo(panelX, 264);

        DrawButtons(panelX, screenH);
    }

    // ── Sezione info seme (nome, rarità, descrizione) ────────────────────────

    private void DrawSeedInfo(int panelX, int y, Seed seed)
    {
        Color boxBg     = new Color(55, 35, 22, 230);
        Color boxBorder = new Color(41, 26, 17, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 8, y, panelWidth - 16, 74),
            0.12f, 6, boxBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX + 8, y, panelWidth - 16, 74),
            0.12f, 6, 1, boxBorder);

        if (seed == null)
        {
            Graphics.DrawText("Nessun seme", panelX + 14, y + 28, 10, dimTextColor);
            return;
        }

        // Nome
        Graphics.DrawText(seed.name, panelX + 14, y + 8, 11, textColor);

        // Rarità
        Graphics.DrawText(
            SeedRarityHelper.GetName(seed.rarity),
            panelX + 14, y + 25,
            9, SeedRarityHelper.GetColor(seed.rarity));

        // Descrizione divisa al primo punto
        string desc = SeedDataType.GetDescription(seed.type);
        int dotIdx = desc.IndexOf('.');
        if (dotIdx > 0 && dotIdx < desc.Length - 1)
        {
            Graphics.DrawText(desc[..(dotIdx + 1)],     panelX + 14, y + 43, 7, dimTextColor);
            Graphics.DrawText(desc[(dotIdx + 1)..].Trim(), panelX + 14, y + 55, 7, dimTextColor);
        }
        else
        {
            Graphics.DrawText(desc, panelX + 14, y + 43, 7, dimTextColor);
        }
    }

    // ── Sezione statistiche ───────────────────────────────────────────────────

    private void DrawStats(int panelX, int y, Seed seed)
    {
        Graphics.DrawText("STATISTICHE", panelX + 14, y, 9, dimTextColor);

        if (seed == null) return;

        // Layout riga: label(24) + gap(3) + barra(87) + gap(3) + valore(33) = 150px
        const int labelW = 24;
        const int barW   = 87;
        const int gap    = 3;
        const int barH   = 7;
        const int rowH   = 18;

        for (int i = 0; i < StatDefs.Length; i++)
        {
            var (label, getValue, min, max, lowerBetter) = StatDefs[i];
            float value    = getValue(seed.stats);
            float fill     = Math.Clamp((value - min) / (max - min), 0f, 1f);
            float goodness = lowerBetter ? 1f - fill : fill;

            int rowY  = y + 14 + i * rowH;
            int baseX = panelX + 10;

            // Etichetta
            Graphics.DrawText(label, baseX, rowY + 1, 7, dimTextColor);

            // Barra
            int barX = baseX + labelW + gap;
            int barY = rowY + (rowH - barH) / 2 - 1;
            Graphics.DrawRectangle(barX, barY, barW, barH, new Color(30, 18, 10, 200));
            int fillPx = (int)(barW * fill);
            if (fillPx > 0)
                Graphics.DrawRectangle(barX, barY, fillPx, barH, GetStatColor(goodness));

            // Valore numerico
            bool isResistance = min < 0;
            string valStr = isResistance
                ? (value >= 0 ? $"+{value:F2}" : $"{value:F2}")
                : $"{value:F2}x";
            Graphics.DrawText(valStr, barX + barW + gap, rowY + 1, 7, GetStatColor(goodness));
        }
    }

    private static Color GetStatColor(float goodness)
    {
        if (goodness >= 0.62f) return new Color(100, 210, 100, 255); // verde
        if (goodness >= 0.35f) return new Color(220, 200, 80,  255); // giallo
        return                        new Color(210, 90,  70,  255); // rosso
    }

    // ── Sezione info fusione ──────────────────────────────────────────────────

    private void DrawFusionInfo(int panelX, int y)
    {
        var fm = SeedFusionManager.Get();

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 8, y, panelWidth - 16, 72),
            0.12f, 6, new Color(80, 150, 220, 85));

        Graphics.DrawText("FUSIONE", panelX + 14, y + 8, 10, Color.White);

        if (fm.CanFuse)
        {
            Graphics.DrawText("Pronti a fondere!", panelX + 14, y + 27, 8, textColor);
            Graphics.DrawText("Premi Unisci.",     panelX + 14, y + 40, 8, dimTextColor);
        }
        else
        {
            int count = fm.SelectedSeed1 != null ? 1 : 0;
            Graphics.DrawText($"Selezionati: {count}/2", panelX + 14, y + 27, 8, textColor);
            Graphics.DrawText("Scegli 2 semi.",     panelX + 14, y + 40, 8, dimTextColor);
        }

        if (selectedSeedIndex >= 0)
        {
            var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
            if (seed != null)
                Graphics.DrawText(
                    $"Fusioni: {seed.stats.fusionCount}/{Seed.MAX_FUSIONS}",
                    panelX + 14, y + 56, 7, dimTextColor);
        }
    }

    // ── Bottoni ───────────────────────────────────────────────────────────────

    private void DrawButtons(int panelX, int screenH)
    {
        var fm = SeedFusionManager.Get();

        const int btnH       = 30;
        const int btnSpacing = 10;
        const int btnMargin  = 10;
        int totalH   = buttonLabels.Length * btnH + (buttonLabels.Length - 1) * btnSpacing;
        int buttonsY = screenH - totalH - btnMargin - 10;

        // Divisore sottile
        Graphics.DrawLine(
            panelX + 8, buttonsY - 8,
            panelX + panelWidth - 8, buttonsY - 8,
            panelBorder);

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + btnMargin;
            int btnY = buttonsY + i * (btnH + btnSpacing);
            int btnW = panelWidth - btnMargin * 2;

            Color bg;
            if (i == 0 && fm.IsFusionMode)
                bg = fm.CanFuse ? new Color(80, 160, 240, 255) : buttonActive;
            else
                bg = i == hoveredButton ? buttonHover : buttonColor;

            Graphics.DrawRectangleRounded(
                new Rectangle(btnX, btnY, btnW, btnH),
                0.25f, 6, bg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(btnX, btnY, btnW, btnH),
                0.25f, 6, 2, buttonBorder);

            string label = buttonLabels[i];
            if (i == 0 && fm.IsFusionMode && fm.CanFuse) label = "FONDI!";

            int tw = label.Length * 6;
            Graphics.DrawText(label, btnX + (btnW - tw) / 2, btnY + (btnH - 11) / 2, 11, textColor);
        }
    }
}

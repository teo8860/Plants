using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiSeedUpgradePanel : GameElement
{
    private bool isOpen = false;
    private Seed currentSeed = null;
    private int seedInventoryIndex = -1;

    // Animazione
    private float slideProgress = 0f;
    private float animationSpeed = 8f;

    // Colori
    private Color panelColor = new Color(82, 54, 35, 245);
    private Color panelBorder = new Color(62, 39, 25, 255);
    private Color buttonColor = new Color(101, 67, 43, 255);
    private Color buttonHoverColor = new Color(139, 90, 55, 255);
    private Color buttonDisabledColor = new Color(60, 45, 30, 200);
    private Color essenceColor = new Color(210, 150, 255, 255);
    private Color statBarBg = new Color(41, 26, 17, 255);
    private Color statBarFill = new Color(200, 150, 80, 255);
    private Color maxLevelColor = new Color(255, 200, 50, 255);

    // Layout
    private int panelWidth = 350;
    private int panelHeight = 400;
    private const int NAV_BAR_HEIGHT = 45;

    // Interazione
    private int hoveredStatIndex = -1;
    private int hoveredActionButton = -1; // 0 = Sacrifica, 1 = Chiudi

    // Lista di statistiche da mostrare
    private static readonly SeedStatType[] StatTypes = new[]
    {
        SeedStatType.Vitalita,
        SeedStatType.Idratazione,
        SeedStatType.ResistenzaParassiti,
        SeedStatType.Vegetazione,
        SeedStatType.Metabolismo
    };

    private static readonly string[] StatNames = new[]
    {
        "Vitalità",
        "Idratazione",
        "Res. Parassiti",
        "Vegetazione",
        "Metabolismo"
    };

    public Obj_GuiSeedUpgradePanel()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -150; // Sopra al detail panel
    }

    public void OpenForSeed(Seed seed, int inventoryIndex)
    {
        currentSeed = seed;
        seedInventoryIndex = inventoryIndex;
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
        currentSeed = null;
        seedInventoryIndex = -1;
    }

    public override void Update()
    {
        if (!isOpen && slideProgress <= 0.01f)
            return;

        float target = isOpen ? 1f : 0f;
        slideProgress += (target - slideProgress) * Time.GetFrameTime() * animationSpeed;
        slideProgress = Math.Clamp(slideProgress, 0f, 1f);

        if (!isOpen || currentSeed == null)
            return;

        InputGate.ConsumeMouse();

        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            Close();
            return;
        }

        UpdateInteraction();
    }

    private void UpdateInteraction()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int panelX = (screenW - panelWidth) / 2;
        int panelY = (screenH - NAV_BAR_HEIGHT - panelHeight) / 2;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left) && slideProgress > 0.99f;

        // Reset hover
        hoveredStatIndex = -1;
        hoveredActionButton = -1;

        // Check hover sulle statistiche
        int statsY = panelY + 100;
        int statHeight = 36;
        int statSpacing = 3;

        for (int i = 0; i < StatTypes.Length; i++)
        {
            int statY = statsY + i * (statHeight + statSpacing);
            int statX = panelX + 15;
            int statW = panelWidth - 30;

            if (mx >= statX && mx <= statX + statW &&
                my >= statY && my <= statY + statHeight)
            {
                hoveredStatIndex = i;

                if (clicked)
                {
                    TryUpgradeStat(StatTypes[i]);
                }
                break;
            }
        }

        // Check hover sui pulsanti azione
        int buttonW = 120;
        int buttonH = 32;
        int buttonY = panelY + panelHeight - 45;
        int sacrificeX = panelX + 15;
        if (mx >= sacrificeX && mx <= sacrificeX + buttonW &&
            my >= buttonY && my <= buttonY + buttonH)
        {
            hoveredActionButton = 0;
            if (clicked)
            {
                SacrificeSeed();
            }
        }

        // Pulsante Chiudi
        int closeX = panelX + panelWidth - 15 - buttonW;
        if (mx >= closeX && mx <= closeX + buttonW &&
            my >= buttonY && my <= buttonY + buttonH)
        {
            hoveredActionButton = 1;
            if (clicked)
            {
                Close();
            }
        }
    }

    private void TryUpgradeStat(SeedStatType statType)
    {
        
        if (SeedUpgradeSystem.UpgradeStat(currentSeed, statType))
        {
            // Aggiorna la griglia
            if (Game.inventoryGrid != null)
            {
                Game.inventoryGrid.Populate();
            }

            Console.WriteLine($"Miglioramento completato: {statType} +1");
        }
        else
        {
            Console.WriteLine("Impossibile migliorare: essenza insufficiente o livello massimo raggiunto");
        }
    }

    private void SacrificeSeed()
    {
        if (currentSeed == null) return;

        int essenceGained = SeedUpgradeSystem.SacrificeSeed(currentSeed);

        Console.WriteLine($"Seme sacrificato! Ottenuta {essenceGained} essenza");

        // Aggiorna inventario
        if (Game.inventoryGrid != null)
        {
            Game.inventoryGrid.Populate();
        }

        Close();
    }

    public override void Draw()
    {
        if (slideProgress < 0.01f)
            return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        // Overlay scuro
        byte overlayAlpha = (byte)(150 * slideProgress);
        Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, overlayAlpha));

        // Posizione pannello (riserva spazio per la navbar in basso).
        float eased = EaseOutBack(slideProgress);
        int currentPanelH = (int)(panelHeight * eased);
        int panelX = (screenW - panelWidth) / 2;
        int panelY = (screenH - NAV_BAR_HEIGHT - currentPanelH) / 2;

        if (currentPanelH < 50)
            return;

        // Pannello principale
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelWidth, currentPanelH),
            0.08f, 8, panelColor
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelWidth, currentPanelH),
            0.08f, 8, 3, panelBorder
        );

        if (slideProgress < 0.5f || currentSeed == null)
            return;

        // Header
        DrawHeader(panelX, panelY);

        // Essenza
        DrawEssenceDisplay(panelX, panelY + 65);

        // Statistiche
        DrawStats(panelX, panelY + 100);

        // Pulsanti azione
        DrawActionButtons(panelX, panelY + currentPanelH - 45);
    }

    private void DrawHeader(int panelX, int panelY)
    {
        Color headerBg = new Color(50, 60, 80, 255);
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 10, panelY + 10, panelWidth - 20, 50),
            0.15f, 6, headerBg
        );

        string title = "MIGLIORA SEME";
        int titleW = GuiTheme.MeasureText(title, 12);
        GuiTheme.DrawText(title, panelX + (panelWidth - titleW) / 2, panelY + 18, 12, Color.White);

        // Rarità
        Color rarityColor = SeedDefinitions.GetRarityColor(currentSeed.rarity);
        string rarityText = SeedDefinitions.GetRarityName(currentSeed.rarity);
        int rarityW = GuiTheme.MeasureText(rarityText, 10);
        GuiTheme.DrawText(rarityText, panelX + (panelWidth - rarityW) / 2, panelY + 38, 10, rarityColor);
    }

    private void DrawEssenceDisplay(int panelX, int y)
    {
        int essence = SeedUpgradeSystem.Essence;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 15, y, panelWidth - 30, 35),
            0.2f, 6, new Color(30, 25, 40, 255)
        );

        string label = "ESSENZA:";
        GuiTheme.DrawText(label, panelX + 25, y + 10, 10, new Color(220, 210, 195, 255));
        int labelW = GuiTheme.MeasureText(label, 10);
        GuiTheme.DrawText(essence.ToString(), panelX + 25 + labelW + 8, y + 10, 12, essenceColor);

        // Icona essenza (cristallo)
        int iconX = panelX + panelWidth - 45;
        int iconY = y + 17;
        DrawEssenceIcon(iconX, iconY, 15);
    }

    private void DrawStats(int panelX, int statsY)
    {
        int maxLevel = SeedUpgradeSystem.GetMaxUpgradeLevel(currentSeed);

        int statHeight = 36;
        int statSpacing = 3;

        for (int i = 0; i < StatTypes.Length; i++)
        {
            var statType = StatTypes[i];
            int currentLevel = SeedUpgradeSystem.GetStatLevel(currentSeed, statType);
            int cost = SeedUpgradeSystem.GetUpgradeCost(currentSeed);
            bool canUpgrade = SeedUpgradeSystem.CanUpgrade(currentSeed);
            bool isMaxLevel = currentLevel >= maxLevel;
            bool isHovered = hoveredStatIndex == i;

            int statY = statsY + i * (statHeight + statSpacing);
            int statX = panelX + 15;
            int statW = panelWidth - 30;

            // Background
            Color bgColor = isHovered && canUpgrade ? new Color(60, 80, 120, 255) :
                           isMaxLevel ? new Color(80, 70, 40, 255) :
                           statBarBg;

            Graphics.DrawRectangleRounded(
                new Rectangle(statX, statY, statW, statHeight),
                0.15f, 6, bgColor
            );

            // Bordo
            Color borderColor = isMaxLevel ? maxLevelColor :
                               canUpgrade ? (isHovered ? Color.White : new Color(180, 120, 70, 255)) :
                                new Color(60, 45, 30, 255);

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(statX, statY, statW, statHeight),
                0.15f, 6, 2, borderColor
            );

            // Nome stat (top-left)
            GuiTheme.DrawText(StatNames[i], statX + 8, statY + 5, 10, Color.White);

            // Livello (bottom-left)
            string levelText = isMaxLevel ? "MAX" : $"Lv {currentLevel}/{maxLevel}";
            Color levelColor = isMaxLevel ? maxLevelColor : new Color(200, 150, 80, 255);
            GuiTheme.DrawText(levelText, statX + 8, statY + 19, 10, levelColor);

            // Costo (bottom-right, right-aligned)
            if (!isMaxLevel)
            {
                Color costColor = canUpgrade ? essenceColor : new Color(100, 100, 120, 255);
                string costText = $"{cost}";
                int costTW = GuiTheme.MeasureText(costText, 10);
                int iconSpace = 18;
                GuiTheme.DrawText(costText, statX + statW - 10 - iconSpace - costTW, statY + 19, 10, costColor);
                DrawEssenceIcon(statX + statW - 16, statY + 25, 7);
            }

            // Barra di progresso
            if (maxLevel > 0)
            {
                int barW = statW - 16;
                int barH = 3;
                int barX = statX + 8;
                int barY = statY + statHeight - 6;

                Graphics.DrawRectangle(barX, barY, barW, barH, new Color(20, 20, 30, 255));

                float progress = (float)currentLevel / maxLevel;
                int fillW = (int)(barW * progress);

                Color fillColor = isMaxLevel ? maxLevelColor : statBarFill;
                Graphics.DrawRectangle(barX, barY, fillW, barH, fillColor);
            }
        }
    }

    private void DrawActionButtons(int panelX, int buttonY)
    {
        int buttonW = 120;
        int buttonH = 32;

        int sacrificeValue = SeedUpgradeSystem.PreviewSacrificeValue(currentSeed);

        // Pulsante Sacrifica
        int sacrificeX = panelX + 15;
        bool sacrificeHovered = hoveredActionButton == 0;

        Color sacrificeBg = sacrificeHovered ? new Color(200, 80, 80, 255) : new Color(160, 60, 60, 255);
        Graphics.DrawRectangleRounded(
            new Rectangle(sacrificeX, buttonY, buttonW, buttonH),
            0.25f, 6, sacrificeBg
        );

        GuiTheme.DrawText("Sacrifica", sacrificeX + 12, buttonY + 4, 10, Color.White);

        // Mostra valore essenza
        string valueText = $"+{sacrificeValue}";
        int valW = GuiTheme.MeasureText(valueText, 10);
        GuiTheme.DrawText(valueText, sacrificeX + 15, buttonY + 18, 10, essenceColor);
        DrawEssenceIcon(sacrificeX + 15 + valW + 8, buttonY + 23, 5);

        // Pulsante Chiudi
        int closeX = panelX + panelWidth - 15 - buttonW;
        bool closeHovered = hoveredActionButton == 1;

        Color closeBg = closeHovered ? new Color(180, 120, 70, 255) : new Color(120, 80, 45, 255);
        Graphics.DrawRectangleRounded(
            new Rectangle(closeX, buttonY, buttonW, buttonH),
            0.25f, 6, closeBg
        );

        string chiudiText = "Chiudi";
        int chiudiW = GuiTheme.MeasureText(chiudiText, 10);
        GuiTheme.DrawText(chiudiText, closeX + (buttonW - chiudiW) / 2, buttonY + 10, 10, Color.White);
    }

    private void DrawEssenceIcon(int x, int y, int size)
    {
        // Disegna un cristallo stilizzato
        float time = (float)Time.GetTime();
        float pulse = (MathF.Sin(time * 3f) + 1f) * 0.5f;

        byte alpha = (byte)(180 + pulse * 75);
        Color crystalColor = new Color(essenceColor.R, essenceColor.G, essenceColor.B, alpha);

        // Forma rombo
        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x, y - size),
            new System.Numerics.Vector2(x - size / 2, y),
            new System.Numerics.Vector2(x + size / 2, y),
            crystalColor
        );

        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x, y + size),
            new System.Numerics.Vector2(x - size / 2, y),
            new System.Numerics.Vector2(x + size / 2, y),
            crystalColor
        );

        // Highlight
        Graphics.DrawCircle(x, y - size / 2, size / 4, new Color(255, 255, 255, (byte)(100 + pulse * 50)));
    }


    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}
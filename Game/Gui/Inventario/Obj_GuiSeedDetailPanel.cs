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
    private float animationSpeed = 8f;

    public int panelWidth = 170;
    private int selectedSeedIndex = -1;

    // Colori stile legno
    private Color panelColor = new Color(82, 54, 35, 245);
    private Color panelBorder = new Color(62, 39, 25, 255);
    private Color buttonColor = new Color(101, 67, 43, 255);
    private Color buttonHoverColor = new Color(139, 90, 55, 255);
    private Color buttonActiveColor = new Color(100, 180, 255, 255);
    private Color buttonBorder = new Color(62, 39, 25, 255);
    private Color textColor = new Color(245, 235, 220, 255);

    private string[] buttonLabels = { "Unisci", "Scarta", "Migliora" };
    private int hoveredButton = -1;

    public Action<int, string> OnButtonClicked;

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
        isOpen = true;
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
        slideProgress += (target - slideProgress) * Time.GetFrameTime() * animationSpeed;
        slideProgress = Math.Clamp(slideProgress, 0f, 1f);

        if (slideProgress < 0.01f)
        {
            hoveredButton = -1;
            return;
        }

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;
        int panelX = screenWidth - (int)(panelWidth * slideProgress);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredButton = -1;

        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int navBarHeight = 45; // Spazio per la barra di navigazione (35px + padding)
        int buttonsStartY = screenHeight - totalButtonsHeight - buttonMargin - navBarHeight;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + buttonMargin;
            int btnY = buttonsStartY + i * (buttonHeight + buttonSpacing);
            int btnWidth = panelWidth - buttonMargin * 2;

            if (mx >= btnX && mx <= btnX + btnWidth && my >= btnY && my <= btnY + buttonHeight)
            {
                hoveredButton = i;

                if (clicked)
                {
                    HandleButtonClick(i);
                }
                break;
            }
        }
    }

    private void HandleButtonClick(int buttonIndex)
    {
        string action = buttonLabels[buttonIndex];

        switch (action)
        {
            case "Unisci":
                HandleFusion();
                break;
            case "Scarta":
                HandleDiscard();
                break;
            case "Migliora":
                HandleUpgrade();
                break;
        }

        OnButtonClicked?.Invoke(selectedSeedIndex, action);
    }

    private void HandleFusion()
    {
        var fusionManager = SeedFusionManager.Get();

        if (fusionManager.IsFusionMode)
        {
            // Se siamo già in modalità fusione e abbiamo 2 semi selezionati, esegui la fusione
            if (fusionManager.CanFuse)
            {
                Seed fusedSeed = fusionManager.PerformFusion();

                if (fusedSeed != null && Game.inventoryGrid != null)
                {
                    // Ripopola la griglia
                    Game.inventoryGrid.Populate();

                    // Mostra animazione o notifica (opzionale)
                    Console.WriteLine($"Fusione completata! Nuovo seme: {fusedSeed.name} [{fusedSeed.rarity}]");
                }
            }
            else
            {
                // Cancella la modalità fusione
                fusionManager.StopFusionMode();
            }
        }
        else
        {
            // Entra in modalità fusione
            fusionManager.StartFusionMode();
        }
    }

    private void HandleDiscard()
    {
        if (selectedSeedIndex < 0) return;

        var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
        if (seed != null)
        {
            Inventario.get().RemoveSeed(seed);
            Game.inventoryGrid?.Populate();
            selectedSeedIndex = -1;
            Console.WriteLine("Seme scartato");
        }
    }

    private void HandleUpgrade()
    {
        if (selectedSeedIndex < 0) return;

        var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
        if (seed == null) return;

        // Apri la schermata di upgrade
        if (Game.seedUpgradePanel != null)
        {
            Game.seedUpgradePanel.OpenForSeed(seed, selectedSeedIndex);
        }
    }

    public override void Draw()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
        {
            Close();
            return;
        }

        if (slideProgress < 0.01f) return;

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;
        int panelX = screenWidth - (int)(panelWidth * slideProgress);

        // Pannello principale
        Graphics.DrawRectangle(panelX, 0, panelWidth, screenHeight, panelColor);
        Graphics.DrawLine(panelX, 0, panelX, screenHeight, panelBorder);

        // Area stats
        int statsAreaHeight = 150;
        Color statsBoxColor = new Color(62, 39, 25, 220);
        Color statsBoxBorder = new Color(41, 26, 17, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 10, 15, panelWidth - 20, statsAreaHeight),
            0.1f,
            8,
            statsBoxColor
        );
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX + 10, 15, panelWidth - 20, statsAreaHeight),
            0.1f,
            8,
            2,
            statsBoxBorder
        );

        // Contenuto stats
        Graphics.DrawText("STATS", panelX + 18, 25, 14, textColor);

        if (selectedSeedIndex >= 0)
        {
            var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
            if (seed != null)
            {
                Color subtextColor = new Color(200, 180, 150, 255);
                Graphics.DrawText($"Seme #{selectedSeedIndex + 1}", panelX + 18, 50, 12, subtextColor);

                // Mostra rarità
                Color rarityColor = GetRarityColor(seed.rarity);
                Graphics.DrawText(GetRarityName(seed.rarity), panelX + 18, 70, 11, rarityColor);
            }
        }

        // Info modalità fusione
        var fusionManager = SeedFusionManager.Get();
        if (fusionManager.IsFusionMode)
        {
            DrawFusionInfo(panelX, statsAreaHeight + 30);
        }

        // Bottoni
        DrawButtons(panelX, screenHeight);
    }

    private void DrawFusionInfo(int panelX, int startY)
    {
        var fusionManager = SeedFusionManager.Get();

        Color infoBoxColor = new Color(100, 180, 255, 100);
        int boxHeight = 80;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 10, startY, panelWidth - 20, boxHeight),
            0.1f, 8, infoBoxColor
        );

        Graphics.DrawText("FUSIONE", panelX + 18, startY + 8, 12, new Color(255, 255, 255, 255));

        if (fusionManager.CanFuse)
        {
            Graphics.DrawText("Semi selezionati: 2", panelX + 18, startY + 28, 9, textColor);
            Graphics.DrawText("Premi Unisci per", panelX + 18, startY + 43, 9, textColor);
            Graphics.DrawText("completare", panelX + 18, startY + 58, 9, textColor);
        }
        else
        {
            int count = fusionManager.SelectedSeed1 != null ? 1 : 0;
            Graphics.DrawText($"Semi selezionati: {count}/2", panelX + 18, startY + 28, 9, textColor);
            Graphics.DrawText("Seleziona 2 semi", panelX + 18, startY + 43, 9, textColor);
            Graphics.DrawText("dalla griglia", panelX + 18, startY + 58, 9, textColor);
        }
    }

    private void DrawButtons(int panelX, int screenHeight)
    {
        var fusionManager = SeedFusionManager.Get();

        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int navBarHeight = 45; // Spazio per la barra di navigazione (35px + padding)
        int buttonsStartY = screenHeight - totalButtonsHeight - buttonMargin - navBarHeight;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + buttonMargin;
            int btnY = buttonsStartY + i * (buttonHeight + buttonSpacing);
            int btnWidth = panelWidth - buttonMargin * 2;

            // Colore bottone speciale per "Unisci" in modalità fusione
            Color bg = buttonColor;
            if (i == 0 && fusionManager.IsFusionMode) // Bottone "Unisci"
            {
                bg = buttonActiveColor;
            }
            else if (i == hoveredButton)
            {
                bg = buttonHoverColor;
            }

            Graphics.DrawRectangleRounded(
                new Rectangle(btnX, btnY, btnWidth, buttonHeight),
                0.22f,
                8,
                bg
            );

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(btnX, btnY, btnWidth, buttonHeight),
                0.22f,
                8,
                3,
                buttonBorder
            );

            // Testo del bottone
            string label = buttonLabels[i];
            if (i == 0 && fusionManager.IsFusionMode && fusionManager.CanFuse)
            {
                label = "FONDI!";
            }

            int textWidth = label.Length * 7;
            int textX = btnX + (btnWidth - textWidth) / 2;
            int textY = btnY + (buttonHeight - 14) / 2;
            Graphics.DrawText(label, textX, textY, 14, textColor);
        }
    }

    private Color GetRarityColor(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune => new Color(200, 200, 200, 255),
        SeedRarity.NonComune => new Color(80, 200, 80, 255),
        SeedRarity.Raro => new Color(80, 150, 255, 255),
        SeedRarity.Epico => new Color(180, 80, 255, 255),
        SeedRarity.Leggendario => new Color(255, 180, 50, 255),
        _ => Color.White
    };

    private string GetRarityName(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune => "Comune",
        SeedRarity.NonComune => "Non Comune",
        SeedRarity.Raro => "Raro",
        SeedRarity.Epico => "Epico",
        SeedRarity.Leggendario => "Leggendario",
        _ => "???"
    };
}
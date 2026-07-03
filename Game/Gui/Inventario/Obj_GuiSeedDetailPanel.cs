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
    private Color buttonColor = new Color(160, 105, 60, 255);
    private Color buttonHoverColor = new Color(200, 140, 80, 255);
    private Color buttonActiveColor = new Color(220, 170, 90, 255);
    private Color buttonBorder = new Color(110, 70, 40, 255);
    private Color textColor = new Color(245, 235, 220, 255);
    private Color essenceColor = new Color(180, 100, 255, 255);

    private string[] buttonLabels = { "Unisci", "Scarta", "Migliora" };
    private bool _canInteract;

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

    public void ClearSeed()
    {
        selectedSeedIndex = -1;
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
            _canInteract = false;
            return;
        }

        int screenWidth = Rendering.camera.screenWidth;
        int panelX = screenWidth - (int)(panelWidth * slideProgress);
        int mx = Input.GetMouseX();

        if (InputGate.MouseConsumed) { _canInteract = false; return; }

        if (mx >= panelX && mx <= panelX + panelWidth)
            InputGate.ConsumeMouse();

        _canInteract = !(Game.itemSlots != null && Game.itemSlots.IsPickerOpen)
            && !(Game.guiFusionResultPopup != null && Game.guiFusionResultPopup.IsVisible)
            && !(Game.fusionPanel != null && Game.fusionPanel.IsOpen)
            && (Game.inventoryCrates == null || !Game.inventoryCrates.IsClickBlocked);
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
        if (Game.fusionPanel != null && Game.fusionPanel.IsOpen)
            return;

        Seed preSelected = null;
        int preSelectedIndex = -1;

        if (Game.inventoryGrid != null)
        {
            preSelectedIndex = Game.inventoryGrid.GetSelectedIndex();
            preSelected = Game.inventoryGrid.GetSeedAtIndex(preSelectedIndex);
        }

        if (Game.fusionPanel != null)
        {
            Game.fusionPanel.Open(preSelected, preSelectedIndex);
        }
    }

    private void HandleDiscard()
    {
        if (selectedSeedIndex < 0) return;

        var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
        if (seed == null) return;

        // Restituisci gli oggetti equipaggiati all'inventario prima del sacrificio.
        if (seed.equippedItems != null)
        {
            foreach (var itemId in seed.equippedItems)
            {
                if (!string.IsNullOrEmpty(itemId))
                    ItemInventory.get().Add(itemId);
            }
        }

        // Sacrifica invece di scartare: converte il seme in essenza.
        int gained = SeedUpgradeSystem.SacrificeSeed(seed);

        // Spawn animazione "+N essenza" al centro del bottone Scarta.
        SpawnEssenceGainFx(gained);

        Game.inventoryGrid?.Populate();
        selectedSeedIndex = -1;
        Console.WriteLine($"Seme sacrificato (oggetti restituiti). Essenza: +{gained}");
    }

    private void SpawnEssenceGainFx(int amount)
    {
        if (amount <= 0) return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int panelX = screenW - (int)(panelWidth * Math.Max(slideProgress, 1f));

        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int navBarHeight = 45;
        int buttonsStartY = screenH - totalButtonsHeight - buttonMargin - navBarHeight;

        // Scarta = indice 1
        int btnX = panelX + buttonMargin;
        int btnY = buttonsStartY + 1 * (buttonHeight + buttonSpacing);
        int btnWidth = panelWidth - buttonMargin * 2;

        var pos = new System.Numerics.Vector2(btnX + btnWidth / 2f - 10, btnY);
        new Obj_EssenceGainFx(pos, amount);
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

        // Contenuto stats
        if (selectedSeedIndex >= 0)
        {
            var seed = Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex);
            if (seed != null)
            {
                // Header: nome e rarita
                int headerY = 12;
                Color statsBoxColor = new Color(62, 39, 25, 220);
                Color statsBoxBorder = new Color(41, 26, 17, 255);

                string seedName = SeedDefinitions.GetSeedName(seed.type);
                GuiTheme.DrawText(seedName, panelX + 18, headerY, 11, textColor);

                Color rarityColor = SeedDefinitions.GetRarityColor(seed.rarity);
                string rarityName = SeedDefinitions.GetRarityName(seed.rarity);
                GuiTheme.DrawText(rarityName, panelX + 18, headerY + 14, 9, rarityColor);

                // Descrizione tipo seme
                string desc = SeedDefinitions.GetSeedDescription(seed.type);
                Color descColor = new Color(180, 170, 150, 200);
                // Wrap manuale per descrizione corta
                int descY = headerY + 28;
                int maxCharsPerLine = (panelWidth - 36) / 5;
                string[] words = desc.Split(' ');
                string line = "";
                int lineCount = 0;
                foreach (var word in words)
                {
                    if ((line + " " + word).Trim().Length > maxCharsPerLine && line.Length > 0)
                    {
                        GuiTheme.DrawText(line, panelX + 18, descY + lineCount * 10, 8, descColor);
                        line = word;
                        lineCount++;
                    }
                    else
                    {
                        line = line.Length == 0 ? word : line + " " + word;
                    }
                }
                if (line.Length > 0)
                {
                    GuiTheme.DrawText(line, panelX + 18, descY + lineCount * 10, 8, descColor);
                    lineCount++;
                }

                // Separatore
                int sepY = descY + lineCount * 10 + 4;
                Hud.Divider(panelX + 15, sepY, panelWidth - 30, new Color(100, 80, 60, 120));

                // Box statistiche (compact per stare sopra gli item slots a Y=160)
                int statsStartY = sepY + 4;
                int statsBoxWidth = panelWidth - 36;

                // Disegna statistiche con barre (compact = 2 colonne)
                if (seed.stats != null)
                {
                    SeedStatsDrawer.Draw(seed.stats, panelX + 18, statsStartY, statsBoxWidth, compact: true);
                }

                // Fusion count e upgrade sotto gli item slots
                int infoY = 218;
                if (seed.stats != null && seed.stats.fusionCount > 0)
                {
                    string fusionText = $"Fusioni: {seed.stats.fusionCount}/{Seed.MAX_FUSIONS}";
                    Color fusionColor = seed.CanBeFused ? new Color(100, 200, 255, 255) : new Color(200, 80, 80, 255);
                    GuiTheme.DrawText(fusionText, panelX + 18, infoY, 9, fusionColor);
                    infoY += 14;
                }

                if (seed.upgradeLevel > 0)
                {
                    string upgradeText = $"Livello: +{seed.upgradeLevel}";
                    GuiTheme.DrawText(upgradeText, panelX + 18, infoY, 9, new Color(255, 200, 50, 255));
                }
            }
        }
        else
        {
            // Nessun seme selezionato
            Color hintColor = new Color(160, 150, 130, 180);
            GuiTheme.DrawText("Seleziona un", panelX + 18, 40, 10, hintColor);
            GuiTheme.DrawText("seme dalla", panelX + 18, 55, 10, hintColor);
            GuiTheme.DrawText("griglia", panelX + 18, 70, 10, hintColor);
        }

        // Info modalità fusione
        var fusionManager = SeedFusionManager.Get();
        if (fusionManager.IsFusionMode)
        {
            DrawFusionInfo(panelX, 220);
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

        GuiTheme.DrawText("FUSIONE", panelX + 18, startY + 8, 12, new Color(255, 255, 255, 255));

        if (fusionManager.CanFuse)
        {
            GuiTheme.DrawText("Semi selezionati: 2", panelX + 18, startY + 28, 9, textColor);
            GuiTheme.DrawText("Premi Unisci per", panelX + 18, startY + 43, 9, textColor);
            GuiTheme.DrawText("completare", panelX + 18, startY + 58, 9, textColor);
        }
        else
        {
            int count = fusionManager.SelectedSeed1 != null ? 1 : 0;
            GuiTheme.DrawText($"Semi selezionati: {count}/2", panelX + 18, startY + 28, 9, textColor);
            GuiTheme.DrawText("Seleziona 2 semi", panelX + 18, startY + 43, 9, textColor);
            GuiTheme.DrawText("dalla griglia", panelX + 18, startY + 58, 9, textColor);
        }
    }

    private void DrawButtons(int panelX, int screenHeight)
    {
        var fusionManager = SeedFusionManager.Get();
        bool hasSeed = selectedSeedIndex >= 0 && Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex) != null;

        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int navBarHeight = 45;
        int buttonsStartY = screenHeight - totalButtonsHeight - buttonMargin - navBarHeight;

        Color disabledBg = new Color(50, 40, 35, 200);
        Color disabledBorder = new Color(45, 33, 22, 200);
        Color disabledText = new Color(140, 130, 120, 150);

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + buttonMargin;
            int btnY = buttonsStartY + i * (buttonHeight + buttonSpacing);
            int btnWidth = panelWidth - buttonMargin * 2;

            bool disabled = !hasSeed && i >= 1;

            string label = buttonLabels[i];
            if (i == 0 && fusionManager.IsFusionMode && fusionManager.CanFuse)
                label = "FONDI!";

            Color bg = (i == 0 && fusionManager.IsFusionMode) ? buttonActiveColor : buttonColor;

            int capturedI = i;
            Action onClick = disabled || !_canInteract ? null : () => HandleButtonClick(capturedI);

            if (i == 1)
            {
                Hud.Button(btnX, btnY, btnWidth, buttonHeight, "", onClick, new Hud.ButtonOptions
                {
                    Disabled = disabled,
                    Bg = bg, HoverBg = buttonHoverColor,
                    DisabledBg = disabledBg,
                    Border = disabled ? disabledBorder : buttonBorder,
                    Roundness = 0.22f, BorderThickness = 3
                });

                Color txtColor = disabled ? disabledText : textColor;
                int labelW = GuiTheme.MeasureText(label, 11);
                GuiTheme.DrawText(label, btnX + (btnWidth - labelW) / 2, btnY + 4, 11, txtColor);

                if (!disabled)
                {
                    Seed sel = selectedSeedIndex >= 0 ? Game.inventoryGrid?.GetSeedAtIndex(selectedSeedIndex) : null;
                    if (sel != null)
                    {
                        int preview = SeedUpgradeSystem.PreviewSacrificeValue(sel);
                        string previewText = $"+{preview}";
                        int pvTextW = GuiTheme.MeasureText(previewText, 9);
                        int iconSize = 5;
                        int totalW = pvTextW + 4 + iconSize * 2;
                        int startX = btnX + (btnWidth - totalW) / 2;
                        GuiTheme.DrawText(previewText, startX, btnY + 20, 9, essenceColor);
                        DrawEssenceIcon(startX + pvTextW + 4 + iconSize, btnY + 25, iconSize);
                    }
                }
            }
            else
            {
                Hud.Button(btnX, btnY, btnWidth, buttonHeight, label, onClick, new Hud.ButtonOptions
                {
                    Disabled = disabled,
                    Bg = bg, HoverBg = buttonHoverColor,
                    DisabledBg = disabledBg, DisabledTextColor = disabledText,
                    Border = disabled ? disabledBorder : buttonBorder,
                    TextColor = textColor,
                    FontSize = 14, Roundness = 0.22f, BorderThickness = 3
                });
            }
        }
    }

    private void DrawEssenceIcon(int x, int y, int size)
    {
        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x, y - size),
            new System.Numerics.Vector2(x - size / 2f, y),
            new System.Numerics.Vector2(x + size / 2f, y),
            essenceColor
        );
        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x, y + size),
            new System.Numerics.Vector2(x - size / 2f, y),
            new System.Numerics.Vector2(x + size / 2f, y),
            essenceColor
        );
    }

}
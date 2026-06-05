using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Plants;

public class Obj_GuiFusionPanel : GameElement
{
    private bool isOpen = false;
    private Seed seed1 = null;
    private Seed seed2 = null;
    private int index1 = -1;
    private int index2 = -1;

    // Cached seed visuals (avoid creating new Obj_Seed every frame)
    private Obj_Seed seedVisual1 = null;
    private Obj_Seed seedVisual2 = null;
    private Dictionary<Seed, Obj_Seed> browserVisuals = new Dictionary<Seed, Obj_Seed>();

    // Animazione
    private float slideProgress = 0f;
    private float animationSpeed = 8f;

    // Colori (warm brown/earth tones matching other panels)
    private Color panelColor = new Color(82, 54, 35, 245);
    private Color panelBorder = new Color(62, 39, 25, 255);
    private Color buttonColor = new Color(101, 67, 43, 255);
    private Color buttonHoverColor = new Color(139, 90, 55, 255);
    private Color buttonDisabledColor = new Color(60, 45, 30, 200);
    private Color closeButtonColor = new Color(90, 60, 40, 255);
    private Color closeButtonHoverColor = new Color(120, 80, 50, 255);
    private Color statBarBg = new Color(35, 22, 14, 255);
    private Color statBarFill = new Color(200, 150, 80, 255);
    private Color statBarFillSecondary = new Color(160, 120, 60, 255);
    private Color headerBg = new Color(62, 39, 25, 255);
    private Color slotEmptyColor = new Color(70, 48, 32, 255);
    private Color slotBorderEmpty = new Color(62, 39, 25, 200);
    private Color slotBorderHover = new Color(200, 150, 80, 255);
    private Color lineColor = new Color(82, 54, 35, 200);
    private Color previewBg = new Color(52, 35, 22, 255);
    private Color browserBg = new Color(45, 30, 18, 255);
    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color filterActiveColor = new Color(200, 150, 80, 255);
    private Color filterInactiveColor = new Color(70, 48, 32, 255);
    private Color fuseButtonColor = new Color(120, 160, 80, 255);
    private Color fuseButtonHoverColor = new Color(150, 200, 100, 255);
    private Color fuseButtonDisabledColor = new Color(60, 70, 40, 200);
    private Color maxFusionColor = new Color(150, 50, 50, 255);

    // Text colors (warm palette for readability)
    private Color textColor = new Color(245, 235, 220, 255);
    private Color textDimColor = new Color(200, 190, 170, 255);
    private Color textMutedColor = new Color(170, 155, 135, 230);
    private Color separatorColor = new Color(82, 54, 35, 120);

    // Layout
    private int panelWidth = 380;
    private int panelHeight = 435;
    private const int NAV_BAR_HEIGHT = 45;

    // Browser state
    private List<Seed> browserSeeds = new();
    private int browserScrollY = 0;
    private int browserCellSize = 38;
    private int browserSpacing = 4;
    private int hoveredBrowserIndex = -1;
    private int browserColumns = 8;
    private bool isDraggingBrowserScroll = false;
    private float dragBrowserOffset = 0;

    private SeedRarity? filterRarity = null;
    private enum SortMode { Nome, Rarita, Stat }
    private SortMode sortMode = SortMode.Nome;
    private int hoveredFilterIndex = -1;
    private bool filterDropdownOpen = false;
    private bool sortDropdownOpen = false;

    private int hoveredSlot = -1;
    private int hoveredButton = -1;
    private int hoveredStat = -1;

    private struct StatPreview
    {
        public string name;
        public float min;
        public float max;
        public float current1;
        public float current2;
        public SeedStatType statType;
        public bool isPrimary;
    }
    private StatPreview[] statPreviews = new StatPreview[0];
    private float compatibilityBonus = 0f;

    private static readonly (SeedStatType type, string name, bool primary)[] StatDisplay = new[]
    {
        (SeedStatType.Vitalita, "Vitalità", true),
        (SeedStatType.Idratazione, "Idratazione", true),
        (SeedStatType.Metabolismo, "Metabolismo", true),
        (SeedStatType.Vegetazione, "Vegetazione", true),
        (SeedStatType.ResistenzaFreddo, "Res. Freddo", false),
        (SeedStatType.ResistenzaCaldo, "Res. Caldo", false),
        (SeedStatType.ResistenzaParassiti, "Res. Parassiti", false),
        (SeedStatType.ResistenzaVuoto, "Res. Vuoto", false),
    };

    public bool IsOpen => isOpen;

    public Obj_GuiFusionPanel()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -150;
    }

    public void Open(Seed preSelected, int preSelectedIndex)
    {
        seed1 = preSelected;
        index1 = preSelectedIndex;
        seed2 = null;
        index2 = -1;
        isOpen = true;
        filterRarity = null;
        sortMode = SortMode.Nome;
        filterDropdownOpen = false;
        sortDropdownOpen = false;
        browserScrollY = 0;
        SyncSeedVisuals();
        RefreshBrowser();
        RecalculatePreview();
    }

    public void Close()
    {
        isOpen = false;
        seed1 = null;
        seed2 = null;
        index1 = -1;
        index2 = -1;
        filterDropdownOpen = false;
        sortDropdownOpen = false;

        seedVisual1?.Destroy();
        seedVisual1 = null;
        seedVisual2?.Destroy();
        seedVisual2 = null;

        foreach (var vis in browserVisuals.Values)
            vis.Destroy();
        browserVisuals.Clear();

        SeedFusionManager.Get().ClosePanel();
    }

    private void SyncSeedVisuals()
    {
        if (seed1 != null && (seedVisual1 == null || seedVisual1.dati != seed1))
        {
            seedVisual1?.Destroy();
            seedVisual1 = new Obj_Seed(seed1)
            {
                roomId = Game.room_inventory.id,
                scale = 1.5f,
                depth = -151,
                guiLayer = true,
                drawManually = true
            };
        }
        else if (seed1 == null && seedVisual1 != null)
        {
            seedVisual1.Destroy();
            seedVisual1 = null;
        }

        if (seed2 != null && (seedVisual2 == null || seedVisual2.dati != seed2))
        {
            seedVisual2?.Destroy();
            seedVisual2 = new Obj_Seed(seed2)
            {
                roomId = Game.room_inventory.id,
                scale = 1.5f,
                depth = -151,
                guiLayer = true,
                drawManually = true
            };
        }
        else if (seed2 == null && seedVisual2 != null)
        {
            seedVisual2.Destroy();
            seedVisual2 = null;
        }
    }

    private void RefreshBrowser()
    {
        var allSeeds = Inventario.get().GetAllSeeds();
        IEnumerable<Seed> source = allSeeds.Where(s => s != seed1 && s != seed2);

        if (filterRarity.HasValue)
            source = source.Where(s => s.rarity == filterRarity.Value);

        source = sortMode switch
        {
            SortMode.Nome => source.OrderBy(s => s.name),
            SortMode.Rarita => source.OrderByDescending(s => (int)s.rarity),
            SortMode.Stat => source.OrderByDescending(s => s.stats.vitalita + s.stats.idratazione + s.stats.metabolismo + s.stats.vegetazione),
            _ => source.OrderBy(s => s.name)
        };

        browserSeeds = source.ToList();
        browserScrollY = 0;

        var toRemove = new List<Seed>();
        foreach (var key in browserVisuals.Keys)
        {
            if (!browserSeeds.Contains(key))
                toRemove.Add(key);
        }
        foreach (var key in toRemove)
        {
            browserVisuals[key].Destroy();
            browserVisuals.Remove(key);
        }
    }

    private void RecalculatePreview()
    {
        if (seed1 == null || seed2 == null)
        {
            statPreviews = new StatPreview[0];
            compatibilityBonus = 0f;
            return;
        }

        compatibilityBonus = CalculateCompatibilityBonus(seed1, seed2);

        var previews = new List<StatPreview>();
        foreach (var (type, name, primary) in StatDisplay)
        {
            float val1 = GetStatValue(seed1.stats, type);
            float val2 = GetStatValue(seed2.stats, type);
            float better = Math.Max(val1, val2);
            float worse = Math.Min(val1, val2);
            float baseVal = better * 0.7f + worse * 0.3f;

            // Mutation range ±15%
            float minVal = baseVal * 0.85f;
            float maxVal = baseVal * 1.15f;

            // Compatibility bonus for vitalita and metabolismo
            if (primary && compatibilityBonus > 0 && (type == SeedStatType.Vitalita || type == SeedStatType.Metabolismo))
            {
                float bonusMult = 1f + compatibilityBonus * 0.1f;
                minVal *= bonusMult;
                maxVal *= bonusMult;
            }

            // Clamp
            if (primary)
            {
                float minClamp = GetStatMin(type);
                minVal = Math.Clamp(minVal, minClamp, SeedStatScaling.StatMax);
                maxVal = Math.Clamp(maxVal, minClamp, SeedStatScaling.StatMax);
            }
            else
            {
                minVal = Math.Clamp(minVal, SeedStatScaling.StatMin, SeedStatScaling.StatMax);
                maxVal = Math.Clamp(maxVal, SeedStatScaling.StatMin, SeedStatScaling.StatMax);
            }

            previews.Add(new StatPreview
            {
                name = name,
                min = minVal,
                max = maxVal,
                current1 = val1,
                current2 = val2,
                statType = type,
                isPrimary = primary
            });
        }
        statPreviews = previews.ToArray();
    }

    private float GetStatValue(SeedStats stats, SeedStatType type) => type switch
    {
        SeedStatType.Vitalita => stats.vitalita,
        SeedStatType.Idratazione => stats.idratazione,
        SeedStatType.Metabolismo => stats.metabolismo,
        SeedStatType.Vegetazione => stats.vegetazione,
        SeedStatType.ResistenzaFreddo => stats.resistenzaFreddo,
        SeedStatType.ResistenzaCaldo => stats.resistenzaCaldo,
        SeedStatType.ResistenzaParassiti => stats.resistenzaParassiti,
        SeedStatType.ResistenzaVuoto => stats.resistenzaVuoto,
        _ => 0f
    };

    private float GetStatMin(SeedStatType type) => type switch
    {
        SeedStatType.Vitalita => SeedStatScaling.VitalitaMin,
        SeedStatType.Idratazione => SeedStatScaling.IdratazioneMin,
        SeedStatType.Metabolismo => SeedStatScaling.MetabolismoMin,
        SeedStatType.Vegetazione => SeedStatScaling.VegetazioneMin,
        _ => SeedStatScaling.StatMin
    };

    private float CalculateCompatibilityBonus(Seed s1, Seed s2)
    {
        float bonus = 0f;
        int rarityDiff = Math.Abs(SeedDefinitions.GetRarityRank(s1.rarity) - SeedDefinitions.GetRarityRank(s2.rarity));
        if (rarityDiff == 0) bonus += 0.3f;
        else if (rarityDiff == 1) bonus += 0.15f;
        if (SeedDefinitions.AreTypesComplementary(s1.type, s2.type)) bonus += 0.2f;
        return bonus;
    }

    public override void Update()
    {
        if (!isOpen && slideProgress <= 0.01f)
            return;

        float target = isOpen ? 1f : 0f;
        slideProgress += (target - slideProgress) * Time.GetFrameTime() * animationSpeed;
        slideProgress = Math.Clamp(slideProgress, 0f, 1f);

        if (!isOpen)
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
        bool rightClicked = Input.IsMouseButtonPressed(MouseButton.Right);

        hoveredSlot = -1;
        hoveredButton = -1;
        hoveredStat = -1;
        hoveredBrowserIndex = -1;
        hoveredFilterIndex = -1;

        int contentX = panelX + 12;
        int contentW = panelWidth - 24;

        int slotSize = 48;
        int slot1X = contentX + 45;
        int slot2X = contentX + contentW - 45 - slotSize;
        int slotY = panelY + 44;

        if (mx >= slot1X && mx <= slot1X + slotSize && my >= slotY && my <= slotY + slotSize)
        {
            hoveredSlot = 0;
            if (rightClicked && seed1 != null)
            {
                seed1 = null;
                index1 = -1;
                SyncSeedVisuals();
                RefreshBrowser();
                RecalculatePreview();
            }
        }

        if (mx >= slot2X && mx <= slot2X + slotSize && my >= slotY && my <= slotY + slotSize)
        {
            hoveredSlot = 1;
            if (rightClicked && seed2 != null)
            {
                seed2 = null;
                index2 = -1;
                SyncSeedVisuals();
                RefreshBrowser();
                RecalculatePreview();
            }
        }

        if (seed1 != null && seed2 != null)
        {
            int statsY = panelY + 116;
            int statH = 14;
            for (int i = 0; i < statPreviews.Length; i++)
            {
                int sy = statsY + i * statH;
                if (mx >= contentX && mx <= contentX + contentW && my >= sy && my <= sy + statH)
                {
                    hoveredStat = i;
                    break;
                }
            }
        }

        int filterBarY = panelY + 243;
        UpdateFilterSortInput(mx, my, clicked, contentX, contentW, filterBarY);

        int browserY = panelY + 266;
        int browserH = 130;
        UpdateBrowserInput(mx, my, clicked, rightClicked, contentX, contentW, browserY, browserH);

        int btnY = panelY + panelHeight - 35;
        int btnW = 130;
        int btnH = 28;
        int fuseBtnX = contentX + (contentW / 2) - btnW - 8;
        int closeBtnX = contentX + (contentW / 2) + 8;

        bool canFuse = seed1 != null && seed2 != null && seed1.CanBeFused && seed2.CanBeFused;

        if (mx >= fuseBtnX && mx <= fuseBtnX + btnW && my >= btnY && my <= btnY + btnH)
        {
            hoveredButton = 0;
            if (clicked && canFuse)
            {
                PerformFusion();
            }
        }

        if (mx >= closeBtnX && mx <= closeBtnX + btnW && my >= btnY && my <= btnY + btnH)
        {
            hoveredButton = 1;
            if (clicked)
            {
                Close();
            }
        }
    }

    private void UpdateFilterSortInput(int mx, int my, bool clicked, int contentX, int contentW, int filterBarY)
    {
        int filterBtnW = 85;
        int filterBtnH = 20;
        int filterBtnX = contentX;

        if (mx >= filterBtnX && mx <= filterBtnX + filterBtnW && my >= filterBarY && my <= filterBarY + filterBtnH)
        {
            hoveredFilterIndex = 0;
            if (clicked)
            {
                filterDropdownOpen = !filterDropdownOpen;
                sortDropdownOpen = false;
            }
        }

        if (filterDropdownOpen)
        {
            var rarities = new SeedRarity?[] { null, SeedRarity.Comune, SeedRarity.NonComune, SeedRarity.Raro, SeedRarity.Esotico, SeedRarity.Epico, SeedRarity.Leggendario, SeedRarity.Mitico };
            int ddY = filterBarY + filterBtnH + 2;
            for (int i = 0; i < rarities.Length; i++)
            {
                int itemY = ddY + i * 18;
                if (mx >= filterBtnX && mx <= filterBtnX + filterBtnW && my >= itemY && my <= itemY + 18)
                {
                    if (clicked)
                    {
                        filterRarity = rarities[i];
                        filterDropdownOpen = false;
                        RefreshBrowser();
                    }
                }
            }
        }

        int sortBtnX = contentX + filterBtnW + 10;
        int sortBtnW = 85;
        if (mx >= sortBtnX && mx <= sortBtnX + sortBtnW && my >= filterBarY && my <= filterBarY + filterBtnH)
        {
            hoveredFilterIndex = 1;
            if (clicked)
            {
                sortDropdownOpen = !sortDropdownOpen;
                filterDropdownOpen = false;
            }
        }

        if (sortDropdownOpen)
        {
            var modes = new[] { SortMode.Nome, SortMode.Rarita, SortMode.Stat };
            int ddY = filterBarY + filterBtnH + 2;
            for (int i = 0; i < modes.Length; i++)
            {
                int itemY = ddY + i * 18;
                if (mx >= sortBtnX && mx <= sortBtnX + sortBtnW && my >= itemY && my <= itemY + 18)
                {
                    if (clicked)
                    {
                        sortMode = modes[i];
                        sortDropdownOpen = false;
                        RefreshBrowser();
                    }
                }
            }
        }
    }

    private void UpdateBrowserInput(int mx, int my, bool clicked, bool rightClicked, int contentX, int contentW, int browserY, int browserH)
    {
        int totalCols = Math.Max(1, contentW / (browserCellSize + browserSpacing));
        browserColumns = totalCols;

        int contentHeight = (int)Math.Ceiling((float)browserSeeds.Count / browserColumns) * (browserCellSize + browserSpacing);
        int maxScroll = Math.Min(0, browserH - contentHeight);

        float wheel = Input.GetMouseWheelMove();
        if (wheel != 0 && mx >= contentX && mx <= contentX + contentW && my >= browserY && my <= browserY + browserH)
        {
            browserScrollY += (int)(wheel * 20);
            browserScrollY = Math.Clamp(browserScrollY, maxScroll, 0);
        }

        if (mx >= contentX && mx <= contentX + contentW && my >= browserY && my <= browserY + browserH)
        {
            for (int i = 0; i < browserSeeds.Count; i++)
            {
                int col = i % browserColumns;
                int row = i / browserColumns;
                int cx = contentX + col * (browserCellSize + browserSpacing);
                int cy = browserY + row * (browserCellSize + browserSpacing) + browserScrollY;

                if (cy + browserCellSize < browserY || cy > browserY + browserH)
                    continue;

                if (mx >= cx && mx <= cx + browserCellSize && my >= cy && my <= cy + browserCellSize)
                {
                    hoveredBrowserIndex = i;
                    if (clicked)
                    {
                        AssignSeedToSlot(browserSeeds[i]);
                    }
                    break;
                }
            }
        }
    }

    private void AssignSeedToSlot(Seed seed)
    {
        if (!seed.CanBeFused) return;

        if (seed1 == null)
        {
            seed1 = seed;
            index1 = Inventario.get().GetAllSeeds().IndexOf(seed);
        }
        else if (seed2 == null)
        {
            seed2 = seed;
            index2 = Inventario.get().GetAllSeeds().IndexOf(seed);
        }
        else
        {
            seed1 = seed;
            index1 = Inventario.get().GetAllSeeds().IndexOf(seed);
        }

        SyncSeedVisuals();
        RefreshBrowser();
        RecalculatePreview();
    }

    private void PerformFusion()
    {
        if (seed1 == null || seed2 == null) return;
        if (!seed1.CanBeFused || !seed2.CanBeFused) return;

        var fusionManager = SeedFusionManager.Get();
        fusionManager.StartFusionMode(seed1, index1);
        fusionManager.ToggleSeedSelection(seed2, index2);

        Seed parent1 = seed1;
        Seed parent2 = seed2;

        Seed fusedSeed = fusionManager.PerformFusion();

        if (fusedSeed != null)
        {
            Game.inventoryGrid?.Populate();
            Game.guiFusionResultPopup?.Show(parent1, parent2, fusedSeed);
            Console.WriteLine($"Fusione completata! Nuovo seme: {fusedSeed.name} [{fusedSeed.rarity}]");
        }

        Close();
    }

    public override void Draw()
    {
        if (slideProgress < 0.01f)
            return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        byte overlayAlpha = (byte)(150 * slideProgress);
        Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, overlayAlpha));

        float eased = EaseOutBack(slideProgress);
        int currentPanelH = (int)(panelHeight * eased);
        int panelX = (screenW - panelWidth) / 2;
        int panelY = (screenH - NAV_BAR_HEIGHT - currentPanelH) / 2;

        if (currentPanelH < 50)
            return;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelWidth, currentPanelH),
            0.08f, 8, panelColor
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelWidth, currentPanelH),
            0.08f, 8, 3, panelBorder
        );

        if (slideProgress < 0.5f)
            return;

        int contentX = panelX + 12;
        int contentW = panelWidth - 24;

        DrawHeader(panelX, panelY);
        DrawParentSlots(panelX, panelY, contentX, contentW);

        if (seed1 != null && seed2 != null)
            DrawStatPreview(panelX, panelY, contentX, contentW);

        int separatorY = panelY + 240;
        Graphics.DrawLine(contentX, separatorY, contentX + contentW, separatorY, separatorColor);

        DrawFilterSortBar(panelX, panelY, contentX, contentW);
        DrawBrowser(panelX, panelY, contentX, contentW);
        DrawActionButtons(panelX, panelY, contentX, contentW);
    }

    private void DrawHeader(int panelX, int panelY)
    {
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 8, panelY + 8, panelWidth - 16, 30),
            0.15f, 6, headerBg
        );

        string title = "FUSIONE SEMI";
        int titleW = GuiTheme.MeasureText(title, 13);
        GuiTheme.DrawText(title, panelX + (panelWidth - titleW) / 2, panelY + 16, 13, textColor);
    }

    private void DrawParentSlots(int panelX, int panelY, int contentX, int contentW)
    {
        int slotSize = 48;
        int slot1X = contentX + 45;
        int slot2X = contentX + contentW - 45 - slotSize;
        int slotY = panelY + 44;

        DrawSeedSlot(slot1X, slotY, slotSize, seed1, hoveredSlot == 0, "S1");
        DrawSeedSlot(slot2X, slotY, slotSize, seed2, hoveredSlot == 1, "S2");

        int centerY = slotY + slotSize + 20;
        int centerX = contentX + contentW / 2;

        int s1CenterX = slot1X + slotSize / 2;
        int s2CenterX = slot2X + slotSize / 2;
        int slotBottom = slotY + slotSize;

        Graphics.DrawLine(s1CenterX, slotBottom, centerX, centerY, lineColor);
        Graphics.DrawLine(s2CenterX, slotBottom, centerX, centerY, lineColor);

        if (seed1 != null && seed2 != null)
        {
            Graphics.DrawCircle(centerX, centerY, 5, new Color(200, 150, 80, 230));
            Graphics.DrawTriangle(
                new Vector2(centerX - 4, centerY + 5),
                new Vector2(centerX + 4, centerY + 5),
                new Vector2(centerX, centerY + 12),
                new Color(200, 150, 80, 230)
            );
        }
        else
        {
            Graphics.DrawCircle(centerX, centerY, 3, new Color(82, 54, 35, 180));
        }
    }

    private void DrawSeedSlot(int x, int y, int size, Seed seed, bool isHovered, string label)
    {
        Color bg = seed != null ? new Color(62, 39, 25, 255) : slotEmptyColor;
        Color border = isHovered ? slotBorderHover : (seed != null ? SeedDefinitions.GetRarityColor(seed.rarity) : slotBorderEmpty);

        Graphics.DrawRectangleRounded(new Rectangle(x + 2, y + 2, size, size), 0.15f, 6, new Color(30, 18, 10, 150));

        Graphics.DrawRectangleRounded(new Rectangle(x, y, size, size), 0.15f, 6, bg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, size, size), 0.15f, 6, 2, border);

        if (seed != null)
        {
            Obj_Seed cachedVisual = (seed == seed1) ? seedVisual1 : (seed == seed2) ? seedVisual2 : null;
            if (cachedVisual != null)
            {
                cachedVisual.position = new Vector2(x + size / 2, y + size / 2);
                cachedVisual.DrawNow();
            }

            string name = SeedDefinitions.GetSeedName(seed.type);
            int nameW = GuiTheme.MeasureText(name, 10);
            GuiTheme.DrawText(name, x + (size - nameW) / 2, y + size + 5, 10, textColor);

            Color rarityCol = SeedDefinitions.GetRarityColor(seed.rarity);
            Graphics.DrawCircle(x + size - 6, y + 6, 4, rarityCol);

            if (isHovered)
            {
                string hint = "Rimuovi";
                int hintW = GuiTheme.MeasureText(hint, 10);
                GuiTheme.DrawText(hint, x + (size - hintW) / 2, y - 12, 10, new Color(200, 150, 120, 230));
            }
        }
        else
        {
            int labelW = GuiTheme.MeasureText(label, 11);
            GuiTheme.DrawText(label, x + (size - labelW) / 2, y + (size - 11) / 2, 11, new Color(170, 150, 130, 230));

            if (isHovered)
            {
                string hint = "Seleziona";
                int hintW = GuiTheme.MeasureText(hint, 10);
                GuiTheme.DrawText(hint, x + (size - hintW) / 2, y + size + 2, 10, new Color(180, 160, 130, 230));
            }
        }
    }

    private void DrawStatPreview(int panelX, int panelY, int contentX, int contentW)
    {
        int statsY = panelY + 116;

        int previewH = statPreviews.Length * 14 + 8;
        Graphics.DrawRectangleRounded(
            new Rectangle(contentX - 2, statsY - 4, contentW + 4, previewH),
            0.1f, 6, previewBg
        );

        if (compatibilityBonus > 0)
        {
            string bonusText = $"Compatibilità: +{compatibilityBonus:F1}";
            int bonusW = GuiTheme.MeasureText(bonusText, 10);
            GuiTheme.DrawText(bonusText, contentX + (contentW - bonusW) / 2, statsY - 14, 10, new Color(150, 200, 120, 230));
        }

        for (int i = 0; i < statPreviews.Length; i++)
        {
            var sp = statPreviews[i];
            int sy = statsY + i * 14;
            bool isHovered = hoveredStat == i;

            Color nameColor = isHovered ? Color.White : textDimColor;
            GuiTheme.DrawText(sp.name, contentX, sy + 2, 10, nameColor);

            string rangeText = $"{sp.min:F0}-{sp.max:F0}";
            int rangeTextW = GuiTheme.MeasureText(rangeText, 10);
            int rangeTextX = contentX + contentW - rangeTextW;

            int barW = 150;
            int barH = 7;
            int barX = rangeTextX - 5 - barW;
            int barY = sy + 3;
            Graphics.DrawRectangle(barX, barY, barW, barH, statBarBg);

            float minRatio = sp.min / SeedStatScaling.StatMax;
            float maxRatio = sp.max / SeedStatScaling.StatMax;
            int rangeX = barX + (int)(barW * minRatio);
            int rangeW = Math.Max(3, (int)(barW * (maxRatio - minRatio)));
            Graphics.DrawRectangle(rangeX, barY, rangeW, barH, statBarFill);

            GuiTheme.DrawText(rangeText, rangeTextX, sy + 2, 10, textDimColor);
        }
    }

    private void DrawFilterSortBar(int panelX, int panelY, int contentX, int contentW)
    {
        int filterBarY = panelY + 243;

        int filterBtnW = 85;
        int filterBtnH = 20;
        int filterBtnX = contentX;
        string filterLabel = filterRarity.HasValue ? SeedDefinitions.GetRarityName(filterRarity.Value) : "Tutti";
        Color filterBg = hoveredFilterIndex == 0 ? filterActiveColor : filterInactiveColor;
        Graphics.DrawRectangleRounded(new Rectangle(filterBtnX, filterBarY, filterBtnW, filterBtnH), 0.2f, 4, filterBg);
        string filterText = $"Filtro: {filterLabel}";
        GuiTheme.DrawText(filterText, filterBtnX + 5, filterBarY + 5, 10, textColor);

        if (filterDropdownOpen)
        {
            var rarities = new (string label, SeedRarity? val)[] {
                ("Tutti", null), ("Comune", SeedRarity.Comune), ("Non Comune", SeedRarity.NonComune),
                ("Raro", SeedRarity.Raro), ("Esotico", SeedRarity.Esotico), ("Epico", SeedRarity.Epico),
                ("Leggendario", SeedRarity.Leggendario), ("Mitico", SeedRarity.Mitico)
            };
            int ddY = filterBarY + filterBtnH + 2;
            Graphics.DrawRectangleRounded(new Rectangle(filterBtnX - 2, ddY - 2, filterBtnW + 4, rarities.Length * 18 + 4), 0.1f, 4, new Color(52, 35, 22, 245));
            for (int i = 0; i < rarities.Length; i++)
            {
                int itemY = ddY + i * 18;
                Color itemBg = filterRarity == rarities[i].val ? filterActiveColor : new Color(62, 42, 28, 255);
                Graphics.DrawRectangle(filterBtnX, itemY, filterBtnW, 18, itemBg);
                GuiTheme.DrawText(rarities[i].label, filterBtnX + 5, itemY + 4, 10, textColor);
            }
        }

        int sortBtnX = contentX + filterBtnW + 10;
        int sortBtnW = 85;
        string sortLabel = sortMode switch { SortMode.Nome => "Nome", SortMode.Rarita => "Rarità", SortMode.Stat => "Stat", _ => "Nome" };
        Color sortBg = hoveredFilterIndex == 1 ? filterActiveColor : filterInactiveColor;
        Graphics.DrawRectangleRounded(new Rectangle(sortBtnX, filterBarY, sortBtnW, filterBtnH), 0.2f, 4, sortBg);
        string sortText = $"Ordina: {sortLabel}";
        GuiTheme.DrawText(sortText, sortBtnX + 5, filterBarY + 5, 10, textColor);

        if (sortDropdownOpen)
        {
            var modes = new[] { SortMode.Nome, SortMode.Rarita, SortMode.Stat };
            int ddY = filterBarY + filterBtnH + 2;
            Graphics.DrawRectangleRounded(new Rectangle(sortBtnX - 2, ddY - 2, sortBtnW + 4, modes.Length * 18 + 4), 0.1f, 4, new Color(52, 35, 22, 245));
            for (int i = 0; i < modes.Length; i++)
            {
                int itemY = ddY + i * 18;
                Color itemBg = sortMode == modes[i] ? filterActiveColor : new Color(62, 42, 28, 255);
                Graphics.DrawRectangle(sortBtnX, itemY, sortBtnW, 18, itemBg);
                string modeName = modes[i] switch { SortMode.Nome => "Nome", SortMode.Rarita => "Rarità", SortMode.Stat => "Stat", _ => "?" };
                GuiTheme.DrawText(modeName, sortBtnX + 5, itemY + 4, 10, textColor);
            }
        }

        string countText = $"{browserSeeds.Count} semi";
        int countW = GuiTheme.MeasureText(countText, 10);
        GuiTheme.DrawText(countText, contentX + contentW - countW, filterBarY + 5, 10, textMutedColor);
    }

    private void DrawBrowser(int panelX, int panelY, int contentX, int contentW)
    {
        int browserY = panelY + 266;
        int browserH = 130;

        Graphics.DrawRectangleRounded(
            new Rectangle(contentX - 2, browserY - 2, contentW + 4, browserH + 4),
            0.08f, 6, browserBg
        );

        Graphics.BeginScissorMode(contentX, browserY + GameProperties.TopBarHeight, contentW, browserH);

        for (int i = 0; i < browserSeeds.Count; i++)
        {
            int col = i % browserColumns;
            int row = i / browserColumns;
            int cx = contentX + col * (browserCellSize + browserSpacing);
            int cy = browserY + row * (browserCellSize + browserSpacing) + browserScrollY;

            if (cy + browserCellSize < browserY || cy > browserY + browserH)
                continue;

            Seed seed = browserSeeds[i];
            bool isMaxFusion = !seed.CanBeFused;
            bool isHovered = hoveredBrowserIndex == i;

            Color bg = isMaxFusion ? maxFusionColor : (isHovered ? cellHoverColor : cellColor);
            Color border = isMaxFusion ? new Color(200, 50, 50, 255) : SeedDefinitions.GetRarityColor(seed.rarity);

            Graphics.DrawRectangleRounded(new Rectangle(cx + 1, cy + 1, browserCellSize, browserCellSize), 0.15f, 4, new Color(20, 12, 8, 120));
            Graphics.DrawRectangleRounded(new Rectangle(cx, cy, browserCellSize, browserCellSize), 0.15f, 4, bg);
            Graphics.DrawRectangleRoundedLines(new Rectangle(cx, cy, browserCellSize, browserCellSize), 0.15f, 4, 1, border);

            if (!browserVisuals.TryGetValue(seed, out var seedVis))
            {
                seedVis = new Obj_Seed(seed)
                {
                    roomId = Game.room_inventory.id,
                    scale = 1.2f,
                    depth = -151,
                    guiLayer = true,
                    drawManually = true
                };
                browserVisuals[seed] = seedVis;
            }
            seedVis.position = new Vector2(cx + browserCellSize / 2, cy + browserCellSize / 2);
            seedVis.DrawNow();

            if (seed.stats.fusionCount > 0)
            {
                int barH = 3;
                int barY = cy + browserCellSize - barH - 1;
                int barW = browserCellSize - 2;
                Graphics.DrawRectangle(cx + 1, barY, barW, barH, new Color(0, 0, 0, 150));
                float fillRatio = (float)seed.stats.fusionCount / Seed.MAX_FUSIONS;
                int fillW = (int)(barW * fillRatio);
                Color fillColor = seed.stats.fusionCount >= Seed.MAX_FUSIONS ? new Color(200, 50, 50, 255) : new Color(120, 180, 80, 255);
                Graphics.DrawRectangle(cx + 1, barY, fillW, barH, fillColor);
            }
        }

        Graphics.EndScissorMode();

        int contentHeight = (int)Math.Ceiling((float)browserSeeds.Count / browserColumns) * (browserCellSize + browserSpacing);
        if (contentHeight > browserH)
        {
            float ratio = (float)browserH / contentHeight;
            int thumbH = Math.Max(12, (int)(browserH * ratio));
            int scrollRange = contentHeight - browserH;
            float progress = scrollRange > 0 ? (-browserScrollY) / (float)scrollRange : 0f;
            int thumbY = browserY + (int)((browserH - thumbH) * progress);
            int trackX = contentX + contentW - 5;

            Graphics.DrawRectangleRounded(new Rectangle(trackX, browserY, 4, browserH), 0.4f, 4, new Color(41, 26, 17, 200));
            Graphics.DrawRectangleRounded(new Rectangle(trackX, thumbY, 4, thumbH), 0.4f, 4, new Color(139, 90, 55, 240));
        }
    }

    private void DrawActionButtons(int panelX, int panelY, int contentX, int contentW)
    {
        int btnY = panelY + panelHeight - 35;
        int btnW = 130;
        int btnH = 28;
        int fuseBtnX = contentX + (contentW / 2) - btnW - 8;
        int closeBtnX = contentX + (contentW / 2) + 8;

        bool canFuse = seed1 != null && seed2 != null && seed1.CanBeFused && seed2.CanBeFused;

        Color fuseBg = !canFuse ? fuseButtonDisabledColor : (hoveredButton == 0 ? fuseButtonHoverColor : fuseButtonColor);
        Graphics.DrawRectangleRounded(new Rectangle(fuseBtnX, btnY, btnW, btnH), 0.25f, 6, fuseBg);
        string fuseText = "Fondi";
        int fuseW = GuiTheme.MeasureText(fuseText, 12);
        GuiTheme.DrawText(fuseText, fuseBtnX + (btnW - fuseW) / 2, btnY + 6, 12, canFuse ? textColor : new Color(130, 120, 100, 200));

        Color closeBg = hoveredButton == 1 ? closeButtonHoverColor : closeButtonColor;
        Graphics.DrawRectangleRounded(new Rectangle(closeBtnX, btnY, btnW, btnH), 0.25f, 6, closeBg);
        string closeText = "Chiudi";
        int closeW = GuiTheme.MeasureText(closeText, 12);
        GuiTheme.DrawText(closeText, closeBtnX + (btnW - closeW) / 2, btnY + 6, 12, textColor);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

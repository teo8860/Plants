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

public class Obj_GuiInventoryGrid : GameElement
{
    private int cellSize = 50;
    private int spacing = 9;
    private int startX = 25;
    private int startY = 60;
    private int scrollY = 0;
    private int bottomMargin = 45; // Spazio per la barra di navigazione (35px + padding)

    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    public Action<int> OnSeedSelected;
    public Obj_GuiSeedDetailPanel detailPanel;

    private SeedRarity? rarityFilter = null;
    private List<Seed> filteredSeeds = new();
    private List<Obj_Seed> visualSeedList = new();

    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color cellSelectedColor = new Color(166, 118, 76, 250);
    private Color cellFusionSelectedColor = new Color(100, 180, 255, 250);
    private Color cellMaxFusionColor = new Color(150, 50, 50, 250);
    private Color borderColor = new Color(62, 39, 25, 255);
    private Color borderSelectedColor = new Color(200, 150, 80, 255);
    private Color borderFusionColor = new Color(100, 200, 255, 255);
    private Color borderMaxFusionColor = new Color(200, 50, 50, 255);
    private Color innerShadow = new Color(41, 26, 17, 180);

    public Obj_GuiInventoryGrid() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -50;
    }

    public void SetRarityFilter(SeedRarity rarity)
    {
        rarityFilter = rarity;
        UpdateFilteredSeeds();
        Populate();
    }

    public void ClearRarityFilter()
    {
        rarityFilter = null;
        UpdateFilteredSeeds();
        Populate();
    }

    private void UpdateFilteredSeeds()
    {
        if (!rarityFilter.HasValue)
            filteredSeeds = Inventario.get().GetAllSeeds();
        else
            filteredSeeds = Inventario.get().GetAllSeeds()
                .Where(seed => seed.rarity == rarityFilter.Value)
                .ToList();
    }

    private int GetCurrentColumns()
    {
        if (detailPanel == null)
            return 10;

        int usableWidth = GameProperties.windowWidth - detailPanel.panelWidth - startX;
        return Math.Max(1, usableWidth / (cellSize + spacing));
    }

    private int GetMaxVisibleRows()
    {
        int screenHeight = Rendering.camera.screenHeight;
        int usableHeight = screenHeight - startY - bottomMargin;
        return Math.Max(1, usableHeight / (cellSize + spacing));
    }

    public void Populate()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        UpdateFilteredSeeds();

        foreach (var seed in visualSeedList)
            seed.Destroy();

        visualSeedList.Clear();
        scrollY = 0;

        int columns = GetCurrentColumns();

        for (int i = 0; i < filteredSeeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;

            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Obj_Seed seedVisual = new Obj_Seed
            {
                roomId = Game.room_inventory.id,
                scale = 1.8f,
                depth = -50,
                position = new Vector2(x + (cellSize / 2), y + (cellSize / 2))
            };

            Seed seedInfo = filteredSeeds[i];
            seedVisual.dati = seedInfo;
            seedVisual.color = seedInfo.color;

            visualSeedList.Add(seedVisual);
        }
    }

    public override void Update()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        float wheelDelta = Input.GetMouseWheelMove();

        if (wheelDelta != 0)
        {
            scrollY += (int)(wheelDelta * 20);

            int columns = GetCurrentColumns();
            int rows = (int)Math.Ceiling((float)visualSeedList.Count / columns);

            int contentHeight = rows * (cellSize + spacing);
            int visibleHeight = Rendering.camera.screenHeight - startY;

            int minScroll = Math.Min(0, visibleHeight - contentHeight);
            int maxScroll = 0;

            scrollY = Math.Clamp(scrollY, minScroll, maxScroll);

            for (int i = 0; i < visualSeedList.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;

                visualSeedList[i].position.Y =
                    startY + row * (cellSize + spacing) + scrollY + (cellSize / 2);
            }
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredIndex = -1;

        int columnsCurrent = GetCurrentColumns();
        var fusionManager = SeedFusionManager.Get();

        for (int i = 0; i < visualSeedList.Count; i++)
        {
            int col = i % columnsCurrent;
            int row = i / columnsCurrent;

            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize)
            {
                hoveredIndex = i;

                if (clicked)
                {
                    if (fusionManager.IsFusionMode)
                        fusionManager.ToggleSeedSelection(filteredSeeds[i], i);
                    else
                    {
                        selectedIndex = i;
                        detailPanel?.Open(i);
                        OnSeedSelected?.Invoke(i);
                    }
                }

                break;
            }
        }
    }

    public override void Draw()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        int columns = GetCurrentColumns();
        int maxX = detailPanel != null
            ? GameProperties.windowWidth - detailPanel.panelWidth
            : GameProperties.windowWidth;

        int rows = (int)Math.Ceiling((float)filteredSeeds.Count / columns);
        var fusionManager = SeedFusionManager.Get();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int i = row * columns + col;
                if (i >= filteredSeeds.Count)
                    break;

                int x = startX + col * (cellSize + spacing);
                int y = startY + row * (cellSize + spacing) + scrollY;

                if (x + cellSize > maxX)
                    continue;

                Seed seed = filteredSeeds[i];
                bool isMaxFusion = seed != null && !seed.CanBeFused;

                Color bg = cellColor;
                Color border = borderColor;

                if (isMaxFusion)
                {
                    bg = cellMaxFusionColor;
                    border = borderMaxFusionColor;
                }
                else if (fusionManager.IsFusionMode && fusionManager.IsSeedSelected(i))
                {
                    bg = cellFusionSelectedColor;
                    border = borderFusionColor;
                }
                else if (i == selectedIndex)
                {
                    bg = cellSelectedColor;
                    border = borderSelectedColor;
                }
                else if (i == hoveredIndex)
                    bg = cellHoverColor;

                Graphics.DrawRectangleRounded(
                    new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                    0.18f, 8, innerShadow);

                Graphics.DrawRectangleRounded(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, bg);

                Graphics.DrawRectangleRoundedLines(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, 3, border);

                if (fusionManager.IsFusionMode && fusionManager.IsSeedSelected(i))
                {
                    int selectionNum = i == fusionManager.SelectedIndex1 ? 1 : 2;
                    Graphics.DrawCircle(x + cellSize - 10, y + 10, 8, new Color(255, 255, 255, 200));
                    Graphics.DrawText(selectionNum.ToString(), x + cellSize - 13, y + 4, 12, new Color(0, 0, 0, 255));
                }

                if (seed.stats.fusionCount > 0)
                    DrawFusionBar(x, y, seed.stats.fusionCount);
            }
        }

        for (int i = 0; i < visualSeedList.Count; i++)
        {
            int col = i % columns;
            int x = startX + col * (cellSize + spacing);

            if (x + cellSize > maxX)
                continue;

            visualSeedList[i].Draw();
        }
    }

    private void DrawFusionBar(int x, int y, int fusionCount)
    {
        int barHeight = 4;
        int barY = y + cellSize - barHeight - 2;
        int barWidth = cellSize - 4;
        int barX = x + 2;

        Graphics.DrawRectangle(barX, barY, barWidth, barHeight, new Color(0, 0, 0, 150));

        float fillRatio = (float)fusionCount / Seed.MAX_FUSIONS;
        int fillWidth = (int)(barWidth * fillRatio);

        Color fillColor;

        if (fusionCount >= Seed.MAX_FUSIONS)
            fillColor = new Color(200, 50, 50, 255);
        else if (fusionCount >= 3)
            fillColor = new Color(255, 150, 50, 255);
        else if (fusionCount >= 2)
            fillColor = new Color(255, 200, 50, 255);
        else
            fillColor = new Color(100, 200, 100, 255);

        Graphics.DrawRectangle(barX, barY, fillWidth, barHeight, fillColor);

        string fusionText = $"{fusionCount}/{Seed.MAX_FUSIONS}";
        Graphics.DrawText(fusionText, x + cellSize - 18, y + cellSize - 12, 7, Color.White);
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public Seed GetSeedAtIndex(int index)
    {
        if (index >= 0 && index < filteredSeeds.Count)
            return filteredSeeds[index];

        return null;
    }
}
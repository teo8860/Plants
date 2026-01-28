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
    private int startY = 60; // Più in basso per header

    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    public Action<int> OnSeedSelected;
    public Obj_GuiSeedDetailPanel detailPanel;
    public List<Obj_Seed> visualSeedList = new();

    private SeedRarity? rarityFilter = null;
    private List<Seed> filteredSeeds = new();

    // Colori
    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color cellSelectedColor = new Color(166, 118, 76, 250);
    private Color borderColor = new Color(62, 39, 25, 255);
    private Color borderSelectedColor = new Color(200, 150, 80, 255);
    private Color innerShadow = new Color(41, 26, 17, 180);

    public Obj_GuiInventoryGrid() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -50;

        for (int i = 0; i < 100; i++)
        {
            int col = i % 100;
            int row = i / 100;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Obj_Seed seedVisual = new Obj_Seed();
            seedVisual.roomId = Game.room_inventory.id;
            seedVisual.scale = 1.8f;
            seedVisual.depth = -1000;
            seedVisual.position.X = x + (cellSize / 2);
            seedVisual.position.Y = y + (cellSize / 2);
            visualSeedList.Add(seedVisual);
        }
    }

    public void SetRarityFilter(SeedRarity rarity)
    {
        rarityFilter = rarity;
        UpdateFilteredSeeds();
    }

    public void ClearRarityFilter()
    {
        rarityFilter = null;
        filteredSeeds.Clear();
    }

    private void UpdateFilteredSeeds()
    {
        if (!rarityFilter.HasValue)
        {
            filteredSeeds = Inventario.get().GetAllSeeds();
        }
        else
        {
            filteredSeeds = Inventario.get().GetAllSeeds()
                .Where(seed => seed.rarity == rarityFilter.Value)
                .ToList();
        }
    }

    private int GetSeedCount()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return 0;

        UpdateFilteredSeeds();
        return filteredSeeds.Count;
    }

    private int GetPanelOffset()
    {
        if (detailPanel == null) return 0;
        return (int)((detailPanel.PanelWidth + 10) * detailPanel.SlideProgress);
    }

    private int GetCurrentColumns()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int availableWidth = screenWidth - GetPanelOffset();

        int margin = 25;
        int usableWidth = availableWidth - margin * 2;
        return Math.Max(1, usableWidth / (cellSize + spacing));
    }

    public override void Update()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
        {
            // Nascondi tutto se non siamo in modalità inventario
            return;
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredIndex = -1;
        int seedCount = GetSeedCount();
        int currentColumns = GetCurrentColumns();

        bool clickedOnCell = false;

        for (int i = 0; i < seedCount; i++)
        {
            Seed seedInfo = filteredSeeds[i];

            // Aggiorna colori visuali
            if (seedInfo.type == SeedType.Glaciale)
                visualSeedList[i].color = new Vector3(1.0f, 1.0f, 1.0f);

            if (seedInfo.type == SeedType.Magmatico)
                visualSeedList[i].color = new Vector3(1.0f, 0.0f, 0.0f);

            if (seedInfo.type == SeedType.Cosmico)
                visualSeedList[i].color = new Vector3(0.1f, 0.1f, 0.1f);

            int col = i % currentColumns;
            int row = i / currentColumns;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize)
            {
                hoveredIndex = i;

                if (clicked)
                {
                    clickedOnCell = true;
                    selectedIndex = i;
                    OnSeedSelected?.Invoke(i);
                }
                break;
            }
        }

        if (clicked && !clickedOnCell && detailPanel != null && detailPanel.IsOpen)
        {
            int screenWidth = Rendering.camera.screenWidth;
            int panelX = screenWidth - (int)(detailPanel.PanelWidth * detailPanel.SlideProgress);

            if (mx < panelX)
            {
                detailPanel.Close();
                selectedIndex = -1;
            }
        }
    }

    public override void Draw()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        int seedCount = GetSeedCount();
        int currentColumns = GetCurrentColumns();

        // Messaggio se vuoto
        if (seedCount == 0)
        {
            Graphics.DrawText("Nessun seme di questa rarità",
                Rendering.camera.screenWidth / 2 - 100,
                Rendering.camera.screenHeight / 2,
                14, new Color(150, 150, 150, 255));
            return;
        }

        for (int i = 0; i < seedCount; i++)
        {
            int col = i % currentColumns;
            int row = i / currentColumns;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Color bg = cellColor;
            if (i == selectedIndex)
                bg = cellSelectedColor;
            else if (i == hoveredIndex)
                bg = cellHoverColor;

            Graphics.DrawRectangleRounded(
                new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                0.18f, 8, innerShadow
            );

            Graphics.DrawRectangleRounded(
                new Rectangle(x, y, cellSize, cellSize),
                0.18f, 8, bg
            );

            Color border = (i == selectedIndex) ? borderSelectedColor : borderColor;
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(x, y, cellSize, cellSize),
                0.18f, 8, 3, border
            );
        }
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
}
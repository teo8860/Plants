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

    // Colori
    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color cellSelectedColor = new Color(166, 118, 76, 250);
    private Color cellFusionSelectedColor = new Color(100, 180, 255, 250);
    private Color cellMaxFusionColor = new Color(150, 50, 50, 250); // Rosso per semi al max fusioni
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
        filteredSeeds.Clear();
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

        int screenWidth = Rendering.camera.screenWidth;
        int usableWidth = GameProperties.windowWidth - detailPanel.panelWidth - startX - startX;
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

        visualSeedList.ForEach(seed => seed.Destroy());
		visualSeedList.Clear();

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
        float wheelDelta = Input.GetMouseWheelMove();

        if (wheelDelta != 0)
        {
            scrollY += (int)(wheelDelta * 20);
            scrollY = Math.Clamp(scrollY, -(cellSize * visualSeedList.Count / 3), -cellSize + (cellSize / 2));

            int columns = GetCurrentColumns();
            for (int i = 0; i < visualSeedList.Count; i++)
            {
                var seed = visualSeedList[i];

                int col = i % columns;
                int row = i / columns;

                seed.position.Y = startY + row * (cellSize + spacing) + scrollY + (cellSize / 2);
            }
        }

        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredIndex = -1;
        int seedCount = visualSeedList.Count;
        bool clickedOnCell = false;

        for (int i = 0; i < seedCount; i++)
        {
            int columns = GetCurrentColumns();
            int col = i % columns;
            int row = i / columns;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize)
            {
                hoveredIndex = i;

                if (clicked)
                {
                    clickedOnCell = true;

                    var fusionManager = SeedFusionManager.Get();

                    if (fusionManager.IsFusionMode)
                    {
                        // Modalit� fusione: seleziona/deseleziona semi
                        fusionManager.ToggleSeedSelection(filteredSeeds[i], i);
                    }
                    else
                    {
                        // Modalit� normale: mostra dettagli
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
        int maxRows = GetMaxVisibleRows();
        int maxX = detailPanel != null ? GameProperties.windowWidth - detailPanel.panelWidth - startX : GameProperties.windowWidth - startX;

        var fusionManager = SeedFusionManager.Get();

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int i = row * columns + col;
                if (i >= 100) break;

                int x = startX + col * (cellSize + spacing);
                int y = startY + row * (cellSize + spacing) + scrollY;

                if (x + cellSize > maxX) continue;

                // Ottieni il seme per controllare fusionCount
                Seed seed = i < filteredSeeds.Count ? filteredSeeds[i] : null;
                bool isMaxFusion = seed != null && !seed.CanBeFused;

                Color bg = cellColor;
                Color border = borderColor;

                // Determina il colore in base allo stato
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
                {
                    bg = cellHoverColor;
                }

                // Ombra
                Graphics.DrawRectangleRounded(new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                    0.18f, 8, innerShadow);

                // Background
                Graphics.DrawRectangleRounded(new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, bg);

                // Bordo
                Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, 3, border);

                // Indicatore fusione (numerino nell'angolo in alto a destra)
                if (fusionManager.IsFusionMode && fusionManager.IsSeedSelected(i))
                {
                    int selectionNum = i == fusionManager.SelectedIndex1 ? 1 : 2;
                    Graphics.DrawCircle(x + cellSize - 10, y + 10, 8, new Color(255, 255, 255, 200));
                    Graphics.DrawText(selectionNum.ToString(), x + cellSize - 13, y + 4, 12, new Color(0, 0, 0, 255));
                }

                // Indicatore contatore fusioni (barra in basso)
                if (seed != null && seed.stats.fusionCount > 0)
                {
                    DrawFusionBar(x, y, seed.stats.fusionCount);
                }
            }
        }

        // Disegna i semi
        for (int i = 0; i < visualSeedList.Count; i++)
        {
            int col = i % columns;
            int x = startX + col * (cellSize + spacing);

            if (x + cellSize > maxX) continue;

            visualSeedList[i].Draw();
        }
    }

    private void DrawFusionBar(int x, int y, int fusionCount)
    {
        int barHeight = 4;
        int barY = y + cellSize - barHeight - 2;
        int barWidth = cellSize - 4;
        int barX = x + 2;

        // Background barra
        Graphics.DrawRectangle(barX, barY, barWidth, barHeight, new Color(0, 0, 0, 150));

        // Calcola larghezza riempimento
        float fillRatio = (float)fusionCount / Seed.MAX_FUSIONS;
        int fillWidth = (int)(barWidth * fillRatio);

        // Colore in base al livello di fusione
        Color fillColor;
        if (fusionCount >= Seed.MAX_FUSIONS)
            fillColor = new Color(200, 50, 50, 255); // Rosso - max
        else if (fusionCount >= 3)
            fillColor = new Color(255, 150, 50, 255); // Arancione - quasi max
        else if (fusionCount >= 2)
            fillColor = new Color(255, 200, 50, 255); // Giallo
        else
            fillColor = new Color(100, 200, 100, 255); // Verde

        Graphics.DrawRectangle(barX, barY, fillWidth, barHeight, fillColor);

        // Numero fusioni (piccolo testo)
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
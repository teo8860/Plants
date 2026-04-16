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
    private int startY = 70;
    private int scrollY = 0;
    private int bottomMargin = 45; // Spazio per la barra di navigazione (35px + padding)

    // Ordinamento (rarita esclusa perche' le casse gia' filtrano per rarita)
    private SeedSorter sorter = new SeedSorter { IncludeRarity = false };
    private Seed selectedSeedRef = null;

    private int scrollbarWidth = 5;
    private int scrollbarPadding = 3;
    private bool isDraggingScrollbar = false;
    private float dragMouseOffsetFromThumbTop = 0;

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

        sorter.OnChanged = () =>
        {
            scrollY = 0;
            Populate();
        };
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

    public void DestroyAllSeeds()
    {
        foreach (var seed in visualSeedList)
            seed.Destroy();
        visualSeedList.Clear();
    }

    private void UpdateFilteredSeeds()
    {
        IEnumerable<Seed> source = Inventario.get().GetAllSeeds();
        if (rarityFilter.HasValue)
            source = source.Where(seed => seed.rarity == rarityFilter.Value);

        filteredSeeds = sorter.Apply(source);

        // Mantieni la selezione sul seme stesso quando cambia l'ordine
        if (selectedSeedRef != null)
        {
            int newIdx = filteredSeeds.IndexOf(selectedSeedRef);
            selectedIndex = newIdx;
            if (newIdx < 0) selectedSeedRef = null;
        }
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

    private int GetVisibleHeight()
    {
        return Rendering.camera.screenHeight - startY - bottomMargin;
    }

    private int GetContentHeight()
    {
        int columns = GetCurrentColumns();
        int rows = (int)Math.Ceiling((float)filteredSeeds.Count / columns);
        return rows * (cellSize + spacing);
    }

    private int GetMaxScrollY()
    {
        return Math.Min(0, GetVisibleHeight() - GetContentHeight());
    }

    private int GetGridMaxX()
    {
        return detailPanel != null
            ? GameProperties.windowWidth - detailPanel.panelWidth
            : GameProperties.windowWidth;
    }

    private void ApplyScrollToSeeds()
    {
        int columns = GetCurrentColumns();
        for (int i = 0; i < visualSeedList.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            visualSeedList[i].position.Y =
                startY + row * (cellSize + spacing) + scrollY + (cellSize / 2);
        }
    }

    private Rectangle GetScrollbarTrackRect()
    {
        int maxX = GetGridMaxX();
        int trackX = maxX - scrollbarWidth - scrollbarPadding;
        int trackY = startY;
        int trackH = GetVisibleHeight();
        return new Rectangle(trackX, trackY, scrollbarWidth, trackH);
    }

    private Rectangle GetScrollbarThumbRect()
    {
        var track = GetScrollbarTrackRect();
        int contentHeight = GetContentHeight();
        int visibleHeight = GetVisibleHeight();

        if (contentHeight <= visibleHeight || contentHeight <= 0)
            return new Rectangle(track.X, track.Y, track.Width, track.Height);

        float ratio = (float)visibleHeight / contentHeight;
        int thumbH = Math.Max(12, (int)(track.Height * ratio));
        int scrollRange = contentHeight - visibleHeight;
        float scrollProgress = scrollRange > 0 ? (-scrollY) / (float)scrollRange : 0f;
        int thumbY = (int)(track.Y + (track.Height - thumbH) * scrollProgress);
        return new Rectangle(track.X, thumbY, track.Width, thumbH);
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
                drawManually = true,
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

        // Blocca input mentre il popup di fusione e' aperto
        if (Game.guiFusionResultPopup != null && Game.guiFusionResultPopup.IsVisible)
            return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // === Sort controls ===
        bool sortConsumed = false;
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsClickBlocked)
            sortConsumed = sorter.HandleInput();

        // === Scrollbar drag ===
        var track = GetScrollbarTrackRect();
        var thumb = GetScrollbarThumbRect();
        int contentHeight = GetContentHeight();
        int visibleHeight = GetVisibleHeight();
        bool hasOverflow = contentHeight > visibleHeight;

        if (isDraggingScrollbar)
        {
            if (!Input.IsMouseButtonDown(MouseButton.Left))
            {
                isDraggingScrollbar = false;
            }
            else if (hasOverflow)
            {
                int thumbH = (int)thumb.Height;
                int trackRange = (int)track.Height - thumbH;
                if (trackRange > 0)
                {
                    int scrollRange = contentHeight - visibleHeight;
                    int thumbYTarget = (int)Math.Clamp(my - dragMouseOffsetFromThumbTop, track.Y, track.Y + trackRange);
                    float p = (thumbYTarget - track.Y) / (float)trackRange;
                    scrollY = Math.Clamp((int)(-p * scrollRange), -scrollRange, 0);
                    ApplyScrollToSeeds();
                }
            }
        }
        else if (Input.IsMouseButtonPressed(MouseButton.Left) && hasOverflow
            && mx >= track.X && mx <= track.X + track.Width
            && my >= track.Y && my <= track.Y + track.Height
            && (Game.inventoryCrates == null || !Game.inventoryCrates.IsClickBlocked))
        {
            if (my >= thumb.Y && my <= thumb.Y + thumb.Height)
            {
                isDraggingScrollbar = true;
                dragMouseOffsetFromThumbTop = my - thumb.Y;
            }
            else
            {
                // Jump: centra il thumb sul click
                int thumbH = (int)thumb.Height;
                int trackRange = (int)track.Height - thumbH;
                int thumbYTarget = (int)Math.Clamp(my - thumbH / 2, track.Y, track.Y + trackRange);
                int scrollRange = contentHeight - visibleHeight;
                float p = trackRange > 0 ? (thumbYTarget - track.Y) / (float)trackRange : 0f;
                scrollY = Math.Clamp((int)(-p * scrollRange), -scrollRange, 0);
                ApplyScrollToSeeds();
                isDraggingScrollbar = true;
                dragMouseOffsetFromThumbTop = thumbH / 2;
            }
        }

        // === Mouse wheel scroll ===
        float wheelDelta = Input.GetMouseWheelMove();

        if (wheelDelta != 0)
        {
            scrollY += (int)(wheelDelta * 20);
            scrollY = Math.Clamp(scrollY, GetMaxScrollY(), 0);
            ApplyScrollToSeeds();
        }

        // === Cell hover / click ===
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left)
            && !isDraggingScrollbar
            && !sortConsumed
            && (Game.inventoryCrates == null || !Game.inventoryCrates.IsClickBlocked);

        bool overSort = sorter.IsMouseOverControls(mx, my);

        hoveredIndex = -1;

        int columnsCurrent = GetCurrentColumns();
        int gridTop = startY;
        int gridBottom = startY + visibleHeight;
        var fusionManager = SeedFusionManager.Get();

        for (int i = 0; i < visualSeedList.Count; i++)
        {
            int col = i % columnsCurrent;
            int row = i / columnsCurrent;

            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            // Skip cells outside visible area (clipped by scissor)
            if (y + cellSize <= gridTop || y >= gridBottom)
                continue;

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize
                && my >= gridTop && my <= gridBottom && !overSort)
            {
                hoveredIndex = i;

                if (clicked)
                {
                    if (fusionManager.IsFusionMode)
                        fusionManager.ToggleSeedSelection(filteredSeeds[i], i);
                    else
                    {
                        selectedIndex = i;
                        selectedSeedRef = filteredSeeds[i];
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
        int maxX = GetGridMaxX();
        int visibleHeight = GetVisibleHeight();

        // Sort bar sopra la griglia
        int sortX = startX;
        int sortW = Math.Max(120, maxX - startX - 10);
        if (sortW > 260) sortW = 260;
        sorter.Draw(sortX, 44, sortW);

        // Riserva spazio per la scrollbar se c'è overflow
        int contentHeight = GetContentHeight();
        bool hasOverflow = contentHeight > visibleHeight;
        int gridRight = hasOverflow ? maxX - scrollbarWidth - scrollbarPadding * 2 : maxX;

        // Scissor clip: celle e semi tagliati ai bordi della griglia visibile
        int scissorX = startX - 2;
        int scissorW = Math.Max(1, gridRight - scissorX);
        Graphics.BeginScissorMode(scissorX, startY, scissorW, visibleHeight);

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

                if (x + cellSize > gridRight)
                    continue;

                // Skip righe completamente fuori (ottimizzazione)
                if (y + cellSize < startY || y > startY + visibleHeight)
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
            int row = i / columns;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            if (x + cellSize > gridRight)
                continue;

            // Skip semi completamente fuori dall'area visibile
            if (y + cellSize < startY || y > startY + visibleHeight)
                continue;

            visualSeedList[i].DrawNow();
        }

        Graphics.EndScissorMode();

        if (hasOverflow)
            DrawScrollbar();
    }

    private void DrawScrollbar()
    {
        var track = GetScrollbarTrackRect();
        var thumb = GetScrollbarThumbRect();

        Color trackBg = new Color(41, 26, 17, 200);
        Color thumbBg = isDraggingScrollbar
            ? new Color(200, 150, 80, 255)
            : new Color(139, 90, 55, 240);
        Color thumbBorder = new Color(62, 39, 25, 255);

        Graphics.DrawRectangleRounded(track, 0.4f, 4, trackBg);
        Graphics.DrawRectangleRounded(thumb, 0.4f, 4, thumbBg);
        Graphics.DrawRectangleRoundedLines(thumb, 0.4f, 4, 1, thumbBorder);
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
        selectedSeedRef = null;
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
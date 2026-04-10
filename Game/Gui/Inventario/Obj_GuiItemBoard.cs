using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

/// <summary>
/// Schermo digitale degli oggetti nella stanza inventario.
/// Stile monitor/tablet con tab per categorie e dettaglio a destra.
/// Visibile solo quando l'inventario casse NON e' aperto.
/// </summary>
public class Obj_GuiItemBoard : GameElement
{
    private int cellSize = 38;
    private int spacing = 5;
    private int scrollY = 0;

    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    private List<string> filteredItemIds = new();
    private ItemCategory activeTab = ItemCategory.Equipaggiabile;

    // Colori stile digitale (come la barra di navigazione)
    private Color screenBg = new Color(25, 25, 35, 245);
    private Color screenBorder = new Color(70, 70, 95, 255);
    private Color screenInner = new Color(35, 35, 50, 240);

    private Color tabActive = new Color(100, 180, 255, 255);
    private Color tabInactive = new Color(55, 55, 75, 255);
    private Color tabHover = new Color(75, 95, 130, 255);
    private Color tabBorder = new Color(80, 80, 100, 255);

    private Color cellBg = new Color(50, 50, 70, 250);
    private Color cellHover = new Color(70, 80, 110, 250);
    private Color cellSelected = new Color(90, 150, 220, 250);
    private Color cellBorder = new Color(80, 80, 100, 255);

    private Color textWhite = new Color(240, 240, 255, 255);
    private Color textDim = new Color(160, 160, 185, 255);
    private Color textAccent = new Color(100, 180, 255, 255);

    // Dettaglio
    private int detailW = 135;

    // Tab info
    private static readonly (ItemCategory cat, string label)[] tabs = {
        (ItemCategory.Equipaggiabile, "Equip."),
        (ItemCategory.Cosmetico, "Cosmet."),
        (ItemCategory.Consumabile, "Consum."),
    };
    private int hoveredTab = -1;

    public Obj_GuiItemBoard() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -48;
    }

    // Posizione e dimensioni dello schermo
    private int ScreenX => 12;
    private int ScreenY => 28;
    private int ScreenW => Rendering.camera.screenWidth - 24;
    private int ScreenH
    {
        get
        {
            int screenHeight = Rendering.camera.screenHeight;
            int navBarHeight = 45;
            int floorY = screenHeight - 30 - navBarHeight;
            int cratesTopY = floorY - 80 - 65 - 25;
            return Math.Max(60, cratesTopY - ScreenY);
        }
    }

    private int TabBarH => 22;
    private int GridY => ScreenY + TabBarH + 6;
    private int GridH => ScreenH - TabBarH - 10;
    private int GridW => ScreenW - detailW - 12;

    private void RefreshItems()
    {
        var allItems = ItemInventory.get().GetAll();
        filteredItemIds = new();
        foreach (var id in allItems)
        {
            var def = ItemRegistry.Get(id);
            if (def != null && def.Category == activeTab)
                filteredItemIds.Add(id);
        }
    }

    private int GetColumns()
    {
        return Math.Max(1, (GridW - 8) / (cellSize + spacing));
    }

    public override void Update()
    {
        if (Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen)
            return;

        RefreshItems();

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        // --- Tab handling ---
        hoveredTab = -1;
        int tabW = (ScreenW - 8) / tabs.Length;
        for (int i = 0; i < tabs.Length; i++)
        {
            int tx = ScreenX + 4 + i * tabW;
            int ty = ScreenY + 3;
            int th = TabBarH - 4;
            if (mx >= tx && mx <= tx + tabW - 2 && my >= ty && my <= ty + th)
            {
                hoveredTab = i;
                if (clicked)
                {
                    activeTab = tabs[i].cat;
                    selectedIndex = -1;
                    scrollY = 0;
                    RefreshItems();
                }
                break;
            }
        }

        // --- Scroll ---
        float wheel = Input.GetMouseWheelMove();
        if (wheel != 0)
        {
            scrollY += (int)(wheel * 18);
            int cols = GetColumns();
            int rows = (int)Math.Ceiling((float)filteredItemIds.Count / cols);
            int contentH = rows * (cellSize + spacing);
            int minScroll = Math.Min(0, GridH - contentH - 4);
            scrollY = Math.Clamp(scrollY, minScroll, 0);
        }

        // --- Grid click ---
        hoveredIndex = -1;
        int columns = GetColumns();
        int gridX = ScreenX + 4;

        for (int i = 0; i < filteredItemIds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            int cx = gridX + col * (cellSize + spacing);
            int cy = GridY + 2 + row * (cellSize + spacing) + scrollY;

            if (cy + cellSize < GridY || cy > GridY + GridH)
                continue;

            if (mx >= cx && mx <= cx + cellSize && my >= cy && my <= cy + cellSize)
            {
                hoveredIndex = i;
                if (clicked)
                    selectedIndex = i;
                break;
            }
        }

        if (selectedIndex >= filteredItemIds.Count)
            selectedIndex = -1;
    }

    public override void Draw()
    {
        if (Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen)
            return;

        int sx = ScreenX;
        int sy = ScreenY;
        int sw = ScreenW;
        int sh = ScreenH;

        // === Schermo esterno (cornice) ===
        Graphics.DrawRectangleRounded(
            new Rectangle(sx - 3, sy - 3, sw + 6, sh + 6),
            0.04f, 8, screenBorder);
        Graphics.DrawRectangleRounded(
            new Rectangle(sx, sy, sw, sh),
            0.04f, 8, screenBg);

        // === Tab bar ===
        int tabW = (sw - 8) / tabs.Length;
        for (int i = 0; i < tabs.Length; i++)
        {
            int tx = sx + 4 + i * tabW;
            int ty = sy + 3;
            int th = TabBarH - 4;

            bool isActive = tabs[i].cat == activeTab;
            bool isHover = hoveredTab == i;

            Color bg = isActive ? tabActive : (isHover ? tabHover : tabInactive);
            Color text = isActive ? textWhite : textDim;

            Graphics.DrawRectangleRounded(
                new Rectangle(tx, ty, tabW - 2, th), 0.2f, 6, bg);

            string label = tabs[i].label;
            int labelW = label.Length * 5;
            Graphics.DrawText(label, tx + (tabW - 2 - labelW) / 2, ty + (th - 9) / 2, 9, text);

            if (isActive)
            {
                Graphics.DrawRectangle(tx + 4, ty + th - 2, tabW - 10, 2,
                    new Color(150, 210, 255, 255));
            }
        }

        // === Linea sotto i tab ===
        Graphics.DrawLine(sx + 4, sy + TabBarH + 1, sx + sw - 4, sy + TabBarH + 1, tabBorder);

        // === Griglia oggetti ===
        int gridX = sx + 4;
        int columns = GetColumns();

        // Sfondo griglia
        Graphics.DrawRectangleRounded(
            new Rectangle(gridX, GridY, GridW, GridH),
            0.04f, 6, screenInner);

        // Scissor clip: le celle vengono tagliate ai bordi della griglia
        Graphics.BeginScissorMode(gridX, GridY, GridW, GridH);

        for (int i = 0; i < filteredItemIds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            int cx = gridX + col * (cellSize + spacing) + 2;
            int cy = GridY + 2 + row * (cellSize + spacing) + scrollY;

            // Skip celle completamente fuori (ottimizzazione)
            if (cy + cellSize < GridY - cellSize || cy > GridY + GridH + cellSize)
                continue;

            var def = ItemRegistry.Get(filteredItemIds[i]);
            if (def == null) continue;

            Color bg = cellBg;
            Color border = cellBorder;
            if (i == selectedIndex)
            {
                bg = cellSelected;
                border = tabActive;
            }
            else if (i == hoveredIndex)
                bg = cellHover;

            Graphics.DrawRectangleRounded(
                new Rectangle(cx, cy, cellSize, cellSize),
                0.18f, 6, bg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(cx, cy, cellSize, cellSize),
                0.18f, 6, 2, border);

            // Iniziale
            string initial = def.Name.Length > 0 ? def.Name.Substring(0, 1) : "?";
            Graphics.DrawText(initial, cx + cellSize / 2 - 5, cy + cellSize / 2 - 8, 14, textWhite);

            // Nome corto sotto
            string sn = def.Name.Length > 5 ? def.Name.Substring(0, 4) + ".." : def.Name;
            Graphics.DrawText(sn, cx + 2, cy + cellSize - 9, 6, textDim);
        }

        if (filteredItemIds.Count == 0)
        {
            string empty = activeTab switch
            {
                ItemCategory.Cosmetico => "Nessun cosmetico",
                ItemCategory.Consumabile => "Nessun consumabile",
                _ => "Nessun equipaggiamento"
            };
            Graphics.DrawText(empty, gridX + 6, GridY + 14, 9, textDim);
        }

        Graphics.EndScissorMode();

        // === Pannello dettaglio a destra ===
        DrawDetailPanel(sx, sw);
    }

    private void DrawDetailPanel(int sx, int sw)
    {
        int panelX = sx + sw - detailW - 4;
        int panelY = GridY;
        int panelH = GridH;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, detailW, panelH),
            0.06f, 6, screenInner);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, detailW, panelH),
            0.06f, 6, 1, tabBorder);

        if (selectedIndex >= 0 && selectedIndex < filteredItemIds.Count)
        {
            var def = ItemRegistry.Get(filteredItemIds[selectedIndex]);
            if (def != null)
            {
                // Nome
                Graphics.DrawText(def.Name, panelX + 6, panelY + 6, 10, textAccent);

                // Categoria badge
                string catLabel = def.Category switch
                {
                    ItemCategory.Cosmetico => "Cosmetico",
                    ItemCategory.Consumabile => "Consumabile",
                    _ => "Equipaggiabile"
                };
                Graphics.DrawText(catLabel, panelX + 6, panelY + 20, 7, textDim);

                // Linea
                Graphics.DrawLine(panelX + 6, panelY + 30, panelX + detailW - 6, panelY + 30, tabBorder);

                // Descrizione
                DrawWrappedText(def.Description, panelX + 6, panelY + 35, detailW - 12, 8, textDim);
            }
        }
        else
        {
            Graphics.DrawText("Seleziona", panelX + 6, panelY + 12, 9, textDim);
            Graphics.DrawText("un oggetto", panelX + 6, panelY + 25, 9, textDim);
        }
    }

    private void DrawWrappedText(string text, int x, int y, int maxWidth, int fontSize, Color color)
    {
        int charWidth = Math.Max(1, fontSize - 2);
        int maxChars = Math.Max(1, maxWidth / charWidth);
        string[] words = text.Split(' ');
        string line = "";
        int lineY = y;

        foreach (var word in words)
        {
            if ((line + " " + word).Trim().Length > maxChars)
            {
                Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
                lineY += fontSize + 3;
                line = word;
            }
            else
            {
                line = line.Length > 0 ? line + " " + word : word;
            }
        }

        if (line.Trim().Length > 0)
            Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
    }
}

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
/// Mostra i 3 slot oggetto sul pannello dettaglio seme nell'inventario.
/// Cliccando su uno slot apre un picker per equipaggiare/dequipaggiare.
/// </summary>
public class Obj_GuiItemSlots : GameElement
{
    private int slotSize = 34;
    private int slotSpacing = 8;

    // Stato picker
    public bool IsPickerOpen => pickerOpen;
    private bool pickerOpen = false;
    private int pickerSlotIndex = -1;
    private int pickerScrollY = 0;
    private int pickerHoveredIndex = -1;

    // Colori
    private Color slotBg = new Color(70, 50, 35, 240);
    private Color slotEmpty = new Color(50, 35, 25, 200);
    private Color slotFilled = new Color(100, 160, 80, 240);
    private Color slotHover = new Color(120, 80, 50, 240);
    private Color slotBorder = new Color(40, 28, 18, 255);
    private Color pickerBg = new Color(60, 45, 30, 245);
    private Color pickerItemBg = new Color(90, 65, 40, 250);
    private Color pickerItemHover = new Color(120, 90, 55, 250);
    private Color pickerBorder = new Color(40, 28, 18, 255);
    private Color textColor = new Color(245, 235, 220, 255);
    private Color textDim = new Color(180, 160, 130, 255);
    private Color removeColor = new Color(200, 80, 60, 240);
    private Color removeHover = new Color(230, 100, 80, 240);
    private Color tooltipBg = new Color(50, 38, 25, 245);
    private Color tooltipBorder = new Color(80, 60, 35, 255);

    // Item hovered nel picker per mostrare descrizione
    private string hoveredItemId = null;

    public Obj_GuiItemSlots() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -110;
    }

    private Seed GetSelectedSeed()
    {
        if (Game.inventoryGrid == null) return null;
        int idx = Game.inventoryGrid.GetSelectedIndex();
        if (idx < 0) return null;
        return Game.inventoryGrid.GetSeedAtIndex(idx);
    }

    public override void Update()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
        {
            pickerOpen = false;
            return;
        }

        if (Game.seedDetailPanel == null || !Game.seedDetailPanel.IsOpen || Game.seedDetailPanel.SlideProgress < 0.5f)
        {
            pickerOpen = false;
            return;
        }

        var seed = GetSelectedSeed();
        if (seed == null)
        {
            pickerOpen = false;
            return;
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        int panelX = Rendering.camera.screenWidth - (int)(Game.seedDetailPanel.PanelWidth * Game.seedDetailPanel.SlideProgress);
        int slotsStartX = panelX + 12;
        int slotsStartY = 175;

        // Se picker e' aperto, gestisci click sul picker
        if (pickerOpen)
        {
            UpdatePicker(mx, my, clicked, seed, panelX);
            return;
        }

        // Click sugli slot
        for (int i = 0; i < Seed.MAX_ITEM_SLOTS; i++)
        {
            int sx = slotsStartX + i * (slotSize + slotSpacing);
            int sy = slotsStartY;

            if (mx >= sx && mx <= sx + slotSize && my >= sy && my <= sy + slotSize && clicked)
            {
                pickerOpen = true;
                pickerSlotIndex = i;
                pickerScrollY = 0;
                pickerHoveredIndex = -1;
                return;
            }
        }
    }

    private void UpdatePicker(int mx, int my, bool clicked, Seed seed, int panelX)
    {
        int pickerX = panelX + 8;
        int pickerY = 220;
        int pickerW = Game.seedDetailPanel.PanelWidth - 16;
        int itemH = 28;
        int pickerMaxH = 200;

        // Scroll
        float wheel = Input.GetMouseWheelMove();
        if (wheel != 0)
        {
            pickerScrollY += (int)(wheel * 15);
            pickerScrollY = Math.Min(0, pickerScrollY);
        }

        // Costruisci lista: prima "Rimuovi" se lo slot ha un oggetto, poi gli oggetti nell'inventario
        var items = ItemInventory.get().GetAll();
        bool hasItem = seed.equippedItems != null && pickerSlotIndex < seed.equippedItems.Count
                       && !string.IsNullOrEmpty(seed.equippedItems[pickerSlotIndex]);
        int totalEntries = (hasItem ? 1 : 0) + items.Count;

        pickerHoveredIndex = -1;
        hoveredItemId = null;

        for (int i = 0; i < totalEntries; i++)
        {
            int iy = pickerY + i * (itemH + 2) + pickerScrollY;
            if (iy + itemH < pickerY || iy > pickerY + pickerMaxH)
                continue;

            if (mx >= pickerX && mx <= pickerX + pickerW && my >= iy && my <= iy + itemH)
            {
                pickerHoveredIndex = i;
                // Traccia l'id dell'oggetto hoverato per il tooltip
                if (hasItem && i == 0)
                    hoveredItemId = seed.equippedItems[pickerSlotIndex];
                else
                {
                    int idx = hasItem ? i - 1 : i;
                    if (idx >= 0 && idx < items.Count)
                        hoveredItemId = items[idx];
                }
                if (clicked)
                {
                    if (hasItem && i == 0)
                    {
                        // Rimuovi oggetto dallo slot
                        string removedId = seed.equippedItems[pickerSlotIndex];
                        seed.equippedItems[pickerSlotIndex] = null;
                        ItemInventory.get().Add(removedId);
                        Inventario.get().Save();
                    }
                    else
                    {
                        int itemIndex = hasItem ? i - 1 : i;
                        if (itemIndex >= 0 && itemIndex < items.Count)
                        {
                            string itemId = items[itemIndex];

                            // Se lo slot aveva gia' un oggetto, rimettilo nell'inventario
                            if (seed.equippedItems != null && pickerSlotIndex < seed.equippedItems.Count
                                && !string.IsNullOrEmpty(seed.equippedItems[pickerSlotIndex]))
                            {
                                ItemInventory.get().Add(seed.equippedItems[pickerSlotIndex]);
                            }

                            seed.equippedItems[pickerSlotIndex] = itemId;
                            ItemInventory.get().Remove(itemId);
                            Inventario.get().Save();
                        }
                    }
                    pickerOpen = false;
                    return;
                }
                break;
            }
        }

        // Click fuori dal picker lo chiude
        if (clicked && (mx < pickerX || mx > pickerX + pickerW || my < pickerY || my > pickerY + pickerMaxH))
        {
            pickerOpen = false;
            hoveredItemId = null;
        }
    }

    public override void Draw()
    {
        if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
            return;
        if (Game.seedDetailPanel == null || !Game.seedDetailPanel.IsOpen || Game.seedDetailPanel.SlideProgress < 0.5f)
            return;

        var seed = GetSelectedSeed();
        if (seed == null) return;

        int panelX = Rendering.camera.screenWidth - (int)(Game.seedDetailPanel.PanelWidth * Game.seedDetailPanel.SlideProgress);
        int slotsStartX = panelX + 12;
        int slotsStartY = 175;

        // Label
        Graphics.DrawText("OGGETTI", panelX + 18, slotsStartY - 15, 10, textDim);

        // Disegna i 3 slot
        for (int i = 0; i < Seed.MAX_ITEM_SLOTS; i++)
        {
            int sx = slotsStartX + i * (slotSize + slotSpacing);
            int sy = slotsStartY;

            bool hasFill = seed.equippedItems != null && i < seed.equippedItems.Count
                           && !string.IsNullOrEmpty(seed.equippedItems[i]);

            Color bg = hasFill ? slotFilled : slotEmpty;

            // Hover
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();
            if (!pickerOpen && mx >= sx && mx <= sx + slotSize && my >= sy && my <= sy + slotSize)
                bg = slotHover;

            Graphics.DrawRectangleRounded(
                new Rectangle(sx, sy, slotSize, slotSize),
                0.2f, 6, bg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(sx, sy, slotSize, slotSize),
                0.2f, 6, 2, slotBorder);

            if (hasFill)
            {
                var def = ItemRegistry.Get(seed.equippedItems[i]);
                if (def != null)
                {
                    string initial = def.Name.Length > 0 ? def.Name.Substring(0, 1) : "?";
                    Graphics.DrawText(initial, sx + slotSize / 2 - 5, sy + slotSize / 2 - 7, 14, textColor);
                }
            }
            else
            {
                Graphics.DrawText("+", sx + slotSize / 2 - 3, sy + slotSize / 2 - 6, 12, textDim);
            }
        }

        // Disegna picker se aperto
        if (pickerOpen)
        {
            DrawPicker(seed, panelX);
            DrawItemTooltip(panelX);
        }
    }

    private void DrawPicker(Seed seed, int panelX)
    {
        int pickerX = panelX + 8;
        int pickerY = 220;
        int pickerW = Game.seedDetailPanel.PanelWidth - 16;
        int itemH = 28;
        int pickerMaxH = 200;

        // Sfondo picker
        Graphics.DrawRectangleRounded(
            new Rectangle(pickerX - 2, pickerY - 2, pickerW + 4, pickerMaxH + 4),
            0.08f, 6, pickerBorder);
        Graphics.DrawRectangleRounded(
            new Rectangle(pickerX, pickerY, pickerW, pickerMaxH),
            0.08f, 6, pickerBg);

        var items = ItemInventory.get().GetAll();
        bool hasItem = seed.equippedItems != null && pickerSlotIndex < seed.equippedItems.Count
                       && !string.IsNullOrEmpty(seed.equippedItems[pickerSlotIndex]);

        int entryIndex = 0;

        // Opzione "Rimuovi" se lo slot ha un oggetto
        if (hasItem)
        {
            int iy = pickerY + entryIndex * (itemH + 2) + pickerScrollY;
            if (iy >= pickerY && iy + itemH <= pickerY + pickerMaxH)
            {
                Color bg = pickerHoveredIndex == entryIndex ? removeHover : removeColor;
                Graphics.DrawRectangleRounded(
                    new Rectangle(pickerX + 2, iy, pickerW - 4, itemH),
                    0.15f, 4, bg);
                Graphics.DrawText("Rimuovi", pickerX + 8, iy + 8, 10, textColor);
            }
            entryIndex++;
        }

        // Lista oggetti nell'inventario
        for (int i = 0; i < items.Count; i++)
        {
            int iy = pickerY + entryIndex * (itemH + 2) + pickerScrollY;
            if (iy + itemH < pickerY || iy > pickerY + pickerMaxH)
            {
                entryIndex++;
                continue;
            }

            var def = ItemRegistry.Get(items[i]);
            if (def == null) { entryIndex++; continue; }

            Color bg = pickerHoveredIndex == entryIndex ? pickerItemHover : pickerItemBg;
            Graphics.DrawRectangleRounded(
                new Rectangle(pickerX + 2, iy, pickerW - 4, itemH),
                0.15f, 4, bg);

            string name = def.Name.Length > 16 ? def.Name.Substring(0, 14) + ".." : def.Name;
            Graphics.DrawText(name, pickerX + 8, iy + 8, 9, textColor);

            entryIndex++;
        }

        if (items.Count == 0 && !hasItem)
        {
            Graphics.DrawText("Nessun oggetto", pickerX + 8, pickerY + 10, 9, textDim);
        }
    }

    private void DrawItemTooltip(int panelX)
    {
        if (string.IsNullOrEmpty(hoveredItemId)) return;

        var def = ItemRegistry.Get(hoveredItemId);
        if (def == null) return;

        int tooltipW = 130;
        int tooltipH = 120;
        int tooltipX = panelX - tooltipW - 6;
        int tooltipY = 220;

        // Clamp a sinistra dello schermo
        if (tooltipX < 4) tooltipX = 4;

        Graphics.DrawRectangleRounded(
            new Rectangle(tooltipX - 2, tooltipY - 2, tooltipW + 4, tooltipH + 4),
            0.08f, 6, tooltipBorder);
        Graphics.DrawRectangleRounded(
            new Rectangle(tooltipX, tooltipY, tooltipW, tooltipH),
            0.08f, 6, tooltipBg);

        // Nome
        Graphics.DrawText(def.Name, tooltipX + 6, tooltipY + 6, 10, textColor);

        // Linea separatrice
        Graphics.DrawLine(tooltipX + 6, tooltipY + 20, tooltipX + tooltipW - 6, tooltipY + 20, tooltipBorder);

        // Descrizione con word wrap
        DrawWrappedText(def.Description, tooltipX + 6, tooltipY + 25, tooltipW - 12, 8, textDim);
    }

    private void DrawWrappedText(string text, int x, int y, int maxWidth, int fontSize, Color color)
    {
        int charWidth = Math.Max(1, fontSize - 3);
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

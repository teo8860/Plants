using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Obj_GuiInventoryGrid : GameElement
{
    private int cellSize = 50;
    private int spacing = 9;
    private int startX = 25;
    private int startY = 25;

    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    public Action<int> OnSeedSelected;
    public Obj_GuiSeedDetailPanel detailPanel; // Riferimento al pannello dettagli
    public List<Obj_Seed> visualSeedList = new();

    // Colori stile legno
    private Color cellColor = new Color(101, 67, 43, 250);        // Marrone medio
    private Color cellHoverColor = new Color(139, 90, 55, 250);   // Marrone chiaro hover
    private Color cellSelectedColor = new Color(166, 118, 76, 250); // Marrone highlight selezionato
    private Color borderColor = new Color(62, 39, 25, 255);       // Marrone scuro bordo
    private Color borderSelectedColor = new Color(200, 150, 80, 255); // Oro/giallo selezionato
    private Color innerShadow = new Color(41, 26, 17, 180);       // Ombra scura

    public Obj_GuiInventoryGrid() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -50;

        for(int i =0; i<100;i++)
        {
            int seedCount = GetSeedCount();

            int col = i % 100;
            int row = i / 100;
            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Obj_Seed seedVisual = new Obj_Seed();
            seedVisual.roomId = Game.room_inventory.id;
            seedVisual.scale = 1.8f;
            seedVisual.depth = -1000;
            seedVisual.position.X = x+(cellSize/2);
            seedVisual.position.Y = y+(cellSize/2);
			visualSeedList.Add(seedVisual);
		}
	}

    private int GetSeedCount()
    {
        return Inventario.get().seeds?.Count ?? 0;
    }

    private int GetPanelOffset()
    {
        if (detailPanel == null) return 0;
        // Usa slideProgress per adattamento graduale durante animazione
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
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredIndex = -1;
        int seedCount = GetSeedCount();
        int currentColumns = GetCurrentColumns();

        bool clickedOnCell = false;

        for (int i = 0; i < seedCount; i++)
        {
            Seed seedInfo = Inventario.get().seeds[i];

            if(seedInfo.type == SeedType.Glaciale)
			    visualSeedList[i].color = new Vector3(1.0f, 1.0f, 1.0f);

            if(seedInfo.type == SeedType.Magmatico)
			    visualSeedList[i].color = new Vector3(1.0f, 0.0f, 0.0f);

            if(seedInfo.type == SeedType.Cosmico)
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

        // Se click fuori da tutte le celle e il pannello è aperto, chiudilo
        if (clicked && !clickedOnCell && detailPanel != null && detailPanel.IsOpen)
        {
            // Controlla se il click è sul pannello dettagli
            int screenWidth = Rendering.camera.screenWidth;
            int panelX = screenWidth - (int)(detailPanel.PanelWidth * detailPanel.SlideProgress);
            
            if (mx < panelX) // Click fuori dal pannello
            {
                detailPanel.Close();
                selectedIndex = -1;
            }
        }
    }

    public override void Draw()
    {
        int seedCount = GetSeedCount();

        // Calcola colonne dinamicamente (usa slideProgress per adattamento graduale)
        int currentColumns = GetCurrentColumns();

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

            // Ombra interna (effetto profondità)
            Graphics.DrawRectangleRounded(
                new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                0.18f,
                8,
                innerShadow
            );

            // Cella principale
            Graphics.DrawRectangleRounded(
                new Rectangle(x, y, cellSize, cellSize),
                0.18f,
                8,
                bg
            );

            // Bordo
            Color border = (i == selectedIndex) ? borderSelectedColor : borderColor;
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(x, y, cellSize, cellSize),
                0.18f,
                8,
                3,
                border
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

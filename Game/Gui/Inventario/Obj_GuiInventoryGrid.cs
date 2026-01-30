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
	private Color borderColor = new Color(62, 39, 25, 255);
	private Color borderSelectedColor = new Color(200, 150, 80, 255);
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
		int usableWidth = GameProperties.windowWidth-detailPanel.panelWidth - startX; // spazio tra l'inizio e il pannello
		return Math.Max(1, usableWidth / (cellSize + spacing));
	}

	public void Populate()
	{
		if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
			return;

		UpdateFilteredSeeds();

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
				depth = -1000,
				position = new Vector2(x + (cellSize / 2), y + (cellSize / 2))
			};

			Seed seedInfo = filteredSeeds[i];
			seedVisual.color = seedInfo.color; // Imposta il colore di tutti i semi uguale all'ultimo seme (da correggere)
            visualSeedList.Add(seedVisual);
		}
	}

	public override void Update()
	{
		float wheelDelta = Input.GetMouseWheelMove();

		if(wheelDelta != 0)
		{
			scrollY += (int)(wheelDelta * 20);
			scrollY = Math.Clamp(scrollY, -(cellSize*visualSeedList.Count/3), -cellSize+(cellSize/2));

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
			int y = startY + row * (cellSize + spacing)  + scrollY;

			if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize)
			{
				hoveredIndex = i;

				if (clicked)
				{
					clickedOnCell = true;
					selectedIndex = i;

					// Mostra info nel pannello
					detailPanel?.Open(i);
					//detailPanel?.Open();
					OnSeedSelected?.Invoke(i);
				}
				break;
			}
		}

		// Click fuori dalle celle non chiude il pannello
	}

	public override void Draw()
{
	if (Game.inventoryCrates == null || !Game.inventoryCrates.IsInventoryOpen)
		return;

	int columns = GetCurrentColumns(); // calcola quante colonne possono stare nello spazio

	int maxX = detailPanel != null ? GameProperties.windowWidth - detailPanel.panelWidth : GameProperties.windowWidth;

	// Disegna solo le slot che stanno nello spazio disponibile fino al pannello
	int maxRows = (int)Math.Ceiling(100.0 / columns);

	for (int row = 0; row < maxRows; row++)
	{
		for (int col = 0; col < columns; col++)
		{
			int i = row * columns + col;
			if (i >= 100) break;

			int x = startX + col * (cellSize + spacing);
			int y = startY + row * (cellSize + spacing) + scrollY;

			// Se la cella supera il bordo del pannello, non disegnarla
			if (x + cellSize > maxX) continue;

			Color bg = cellColor;
			if (i == selectedIndex)
				bg = cellSelectedColor;
			else if (i == hoveredIndex)
				bg = cellHoverColor;

			Graphics.DrawRectangleRounded(new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
				0.18f, 8, innerShadow);

			Graphics.DrawRectangleRounded(new Rectangle(x, y, cellSize, cellSize),
				0.18f, 8, bg);

			Color border = (i == selectedIndex) ? borderSelectedColor : borderColor;
			Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, cellSize, cellSize),
				0.18f, 8, 3, border);
		}
	}

	// Disegna solo i semi presenti, limitandosi allo spazio disponibile
	for (int i = 0; i < visualSeedList.Count; i++)
	{
		int col = i % columns;
		int row = i / columns;
		int x = startX + col * (cellSize + spacing);

		// Limita i semi allo stesso spazio delle celle
		if (x + cellSize > maxX) continue;

		visualSeedList[i].Draw();
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

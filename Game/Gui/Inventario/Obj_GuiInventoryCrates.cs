using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Linq;
using System.Numerics;

namespace Plants;

public class Obj_GuiInventoryCrates : GameElement
{
    // Colori
    private Color woodDark = new Color(62, 39, 25, 255);
    private Color woodMedium = new Color(101, 67, 43, 255);
    private Color woodLight = new Color(139, 90, 55, 255);

    // Casse e rarità
    private Rectangle[] crateClickAreas = new Rectangle[5];
    private SeedRarity[] rarities = {
        SeedRarity.Comune,
        SeedRarity.NonComune,
        SeedRarity.Raro,
        SeedRarity.Epico,
        SeedRarity.Leggendario
    };

    private Color[] rarityColors = {
        new Color(200, 200, 200, 255),  // Comune
        new Color(80, 200, 80, 255),    // NonComune
        new Color(80, 150, 255, 255),   // Raro
        new Color(180, 80, 255, 255),   // Epico
        new Color(255, 180, 50, 255)    // Leggendario
    };

    private string[] rarityNames = {
        "Comune",
        "Non Comune",
        "Raro",
        "Epico",
        "Leggendario"
    };

    private int hoveredCrate = -1;
    public SeedRarity? SelectedRarity { get; private set; } = null;
    public bool IsInventoryOpen { get; private set; } = false;

    public Obj_GuiInventoryCrates() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -45; // Sopra al background ma sotto la grid
    }

    public override void Update()
    {
        if (IsInventoryOpen)
        {
            // Chiudi inventario
            if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.B))
            {
                CloseInventory();
            }
            return;
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        hoveredCrate = -1;

        // Controlla hover sulle casse
        for (int i = 0; i < crateClickAreas.Length; i++)
        {
            if (mx >= crateClickAreas[i].X && mx <= crateClickAreas[i].X + crateClickAreas[i].Width &&
                my >= crateClickAreas[i].Y && my <= crateClickAreas[i].Y + crateClickAreas[i].Height)
            {
                hoveredCrate = i;
                break;
            }
        }

        // Click su cassa
        if (Input.IsMouseButtonPressed(MouseButton.Left) && hoveredCrate != -1)
        {
            OpenInventory(rarities[hoveredCrate]);
        }
    }

    public void OpenInventory(SeedRarity rarity)
    {
        SelectedRarity = rarity;
        IsInventoryOpen = true;

        if (Game.inventoryGrid != null)
        {
            Game.inventoryGrid.Populate();
            Game.inventoryGrid.SetRarityFilter(rarity);
        }
    }

    public void CloseInventory()
    {
        IsInventoryOpen = false;
        SelectedRarity = null;

        if (Game.inventoryGrid != null)
        {
            Game.inventoryGrid.ClearRarityFilter();
        }
    }

    public override void Draw()
    {
        if (IsInventoryOpen)
        {
            
            return;
        }

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        // === CASSE 3D ===
        DrawCrates3D();
    }

    private void DrawCrates3D()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        // Spazio per la barra di navigazione (35px + padding)
        int navBarHeight = 45;

        // Pavimento base (y dove inizia il pavimento)
        int floorY = screenHeight - 30 - navBarHeight;

        // === 2 CASSE A SINISTRA (impilate) ===
        int leftX = 60;
        int leftZ = floorY - 80; // Profondità

        // Cassa Comune (base)
        DrawCrate3D(leftX, leftZ, 70, 60, rarityColors[0], rarityNames[0], 0, hoveredCrate == 0);

        // Cassa NonComune (sopra)
        DrawCrate3D(leftX, leftZ - 65, 70, 60, rarityColors[1], rarityNames[1], 1, hoveredCrate == 1);

        // === 3 CASSE A DESTRA (piramide) ===
        int rightX = screenWidth - 180;
        int rightZ = floorY - 80;

        // Cassa Raro (base sinistra)
        DrawCrate3D(rightX - 40, rightZ, 70, 60, rarityColors[2], rarityNames[2], 2, hoveredCrate == 2);

        // Cassa Epico (base destra)
        DrawCrate3D(rightX + 45, rightZ, 70, 60, rarityColors[3], rarityNames[3], 3, hoveredCrate == 3);

        // Cassa Leggendario (top piramide)
        DrawCrate3D(rightX, rightZ - 65, 70, 60, rarityColors[4], rarityNames[4], 4, hoveredCrate == 4);

        DrawCrate3D(rightX, rightZ - 65, 70, 60, rarityColors[4], rarityNames[4], 4, hoveredCrate == 4);

    }

    private void DrawCrate3D(int x, int y, int width, int height, Color rarityColor, string name, int index, bool hovered)
    {
        // Dimensioni isometriche
        int depth = 50; // Profondità della cassa

        // Area cliccabile (un po' più grande della visual)
        crateClickAreas[index] = new Rectangle(x - 10, y - 10, width + 20, height + 20);

        // Offset isometrico
        int isoOffsetX = (int)(depth * 0.5f);
        int isoOffsetY = (int)(depth * 0.3f);

        // Colori cassa
        Color crateFront = hovered ? new Color(120, 90, 60, 255) : woodMedium;
        Color crateTop = woodLight;
        Color crateSide = woodDark;

        // === OMBRA ===
        Vector2[] shadowPoints = {
            new Vector2(x, y + height),
            new Vector2(x + width, y + height),
            new Vector2(x + width + isoOffsetX, y + height - isoOffsetY),
            new Vector2(x + isoOffsetX, y + height - isoOffsetY)
        };

        for (int i = 0; i < shadowPoints.Length; i++)
        {
            Vector2 p1 = shadowPoints[i];
            Vector2 p2 = shadowPoints[(i + 1) % shadowPoints.Length];
            Graphics.DrawLineEx(p1, p2, 1, new Color(0, 0, 0, 60));
        }
        Graphics.DrawTriangle(shadowPoints[0], shadowPoints[1], shadowPoints[2], new Color(0, 0, 0, 40));
        Graphics.DrawTriangle(shadowPoints[0], shadowPoints[2], shadowPoints[3], new Color(0, 0, 0, 40));

        // === FACCIA FRONTALE ===
        Graphics.DrawRectangle(x, y, width, height, crateFront);

        // Tavole verticali
        for (int i = 1; i < 4; i++)
        {
            int plankX = x + (width / 4) * i;
            Graphics.DrawLine(plankX, y, plankX, y + height, new Color(woodDark.R, woodDark.G, woodDark.B, 80));
        }

        // === FACCIA LATERALE DESTRA ===
        Vector2[] sidePoints = {
            new Vector2(x + width, y),
            new Vector2(x + width + isoOffsetX, y - isoOffsetY),
            new Vector2(x + width + isoOffsetX, y + height - isoOffsetY),
            new Vector2(x + width, y + height)
        };

        Graphics.DrawTriangle(sidePoints[0], sidePoints[1], sidePoints[2], crateSide);
        Graphics.DrawTriangle(sidePoints[0], sidePoints[2], sidePoints[3], crateSide);

        // Linee laterali
        Graphics.DrawLineEx(sidePoints[0], sidePoints[1], 1, new Color(woodMedium.R, woodMedium.G, woodMedium.B, 100));
        Graphics.DrawLineEx(sidePoints[1], sidePoints[2], 1, woodDark);
        Graphics.DrawLineEx(sidePoints[2], sidePoints[3], 1, new Color(woodDark.R, woodDark.G, woodDark.B, 150));

        // === FACCIA SUPERIORE ===
        Vector2[] topPoints = {
            new Vector2(x, y),
            new Vector2(x + width, y),
            new Vector2(x + width + isoOffsetX, y - isoOffsetY),
            new Vector2(x + isoOffsetX, y - isoOffsetY)
        };

        Graphics.DrawTriangle(topPoints[0], topPoints[1], topPoints[2], crateTop);
        Graphics.DrawTriangle(topPoints[0], topPoints[2], topPoints[3], crateTop);

        // Linee superiori
        Graphics.DrawLineEx(topPoints[0], topPoints[1], 1, woodMedium);
        Graphics.DrawLineEx(topPoints[1], topPoints[2], 1, new Color(woodDark.R, woodDark.G, woodDark.B, 120));
        Graphics.DrawLineEx(topPoints[2], topPoints[3], 1, woodDark);
        Graphics.DrawLineEx(topPoints[3], topPoints[0], 1, new Color(woodMedium.R, woodMedium.G, woodMedium.B, 150));

        // === CHIODI ===
        int[] nailPositions = { 10, height - 15 };
        foreach (int nailY in nailPositions)
        {
            Graphics.DrawCircle(x + 10, y + nailY, 2, new Color(40, 40, 40, 255));
            Graphics.DrawCircle(x + width - 10, y + nailY, 2, new Color(40, 40, 40, 255));
        }

        // === ETICHETTA RARITÀ (sul fronte) ===
        int labelWidth = width - 16;
        int labelHeight = 20;
        int labelX = x + 8;
        int labelY = y + height / 2 - labelHeight / 2;

        Graphics.DrawRectangle(labelX, labelY, labelWidth, labelHeight,
            new Color(rarityColor.R, rarityColor.G, rarityColor.B, 220));
        Graphics.DrawRectangleLines(labelX, labelY, labelWidth, labelHeight,
            new Color(woodDark.R, woodDark.G, woodDark.B, 255));

        int textWidth = name.Length * 5;
        Graphics.DrawText(name, labelX + labelWidth / 2 - textWidth / 2, labelY + 6, 9, Color.White);

        // === HOVER EFFECTS ===
        if (hovered)
        {
            // Bordo luminoso
            Graphics.DrawRectangleLines(x - 1, y - 1, width + 2, height + 2,
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, 200));

            // Tooltip
            Graphics.DrawText("Clicca per aprire", x + width / 2 - 40, y - 15, 9, rarityColor);
        }

        // === EFFETTO BRILLANTEZZA (Epico/Leggendario) ===
        if (name == "Epico" || name == "Leggendario")
        {
            float time = (float)Time.GetTime();
            byte alpha = (byte)(100 + Math.Sin(time * 2.5f) * 80);

            Color color = new Color(255, 255, 255, alpha);

		

			GameFunctions.DrawSprite(AssetLoader.spriteShine, new Vector2(x + width - 12, y + 12), time*20, (float)(Math.Sin(time*2)), rarityColor, 0.7f);
			GameFunctions.DrawSprite(AssetLoader.spriteShine, new Vector2(x + 15, y + height - 18), time*15, (float)(Math.Sin(time*1.7)*0.6), rarityColor, 0.8f );

           // Graphics.DrawCircle(x + width - 12, y + 12, 3, new Color(255, 255, 255, alpha));
          //  Graphics.DrawCircle(x + 15, y + height - 18, 2, new Color(255, 255, 255, (byte)(alpha * 0.6f)));
        }
    }
}
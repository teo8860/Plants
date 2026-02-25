using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Interact;
using System;

namespace Plants;

public class Obj_GuiInventoryBackground : GameElement
{
    // Colori palette
    private Color wallColor = new Color(230, 220, 200, 255);
    private Color wallShadow = new Color(200, 190, 170, 255);
    private Color woodDark = new Color(62, 39, 25, 255);
    private Color woodMedium = new Color(101, 67, 43, 255);
    private Color woodLight = new Color(139, 90, 55, 255);
    private Color floorLight = new Color(180, 140, 100, 255);
    private Color floorDark = new Color(150, 110, 70, 255);


    public Obj_GuiInventoryBackground() : base()
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -40;

    }


    public override void Update()
    {

    }


    public override void Draw()
    {

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        // === MURO ===
        Graphics.DrawRectangle(0, 0, screenWidth, screenHeight, wallColor);

        for (int y = 0; y < screenHeight; y += 12)
        {
            Color lineColor = new Color(wallShadow.R, wallShadow.G, wallShadow.B, 20);
            Graphics.DrawLine(0, y, screenWidth, y, lineColor);
        }

        // === PAVIMENTO ===
        int navBarHeight = 45; // Spazio per la barra di navigazione (35px + padding)
        int floorHeight = 120;
        int floorY = screenHeight - floorHeight - navBarHeight;

        int plankHeight = 15;
        for (int i = 0; i < floorHeight / plankHeight; i++)
        {
            Color plankColor = (i % 2 == 0) ? floorLight : floorDark;
            Graphics.DrawRectangle(0, floorY + i * plankHeight, screenWidth, plankHeight, plankColor);
            Graphics.DrawLine(0, floorY + i * plankHeight, screenWidth, floorY + i * plankHeight, woodDark);
        }

        Graphics.DrawRectangleGradientV(floorY, floorY, screenWidth, 30,
            new Color(0, 0, 0, 40), new Color(0, 0, 0, 0));

        // === BATTISCOPA ===
        Graphics.DrawRectangle(0, floorY - 8, screenWidth, 8, woodDark);
        Graphics.DrawLine(0, floorY - 8, screenWidth, floorY - 8, woodMedium);

        // === TITOLO ===
        Graphics.DrawText("MAGAZZINO SEMI", screenWidth / 2 - 70, 20, 18, woodDark);
        Graphics.DrawText("Scegli una cassa", screenWidth / 2 - 45, 42, 11, new Color(100, 80, 60, 255));

        // === BORDI STANZA ===
        DrawRoomBorders();
    }

    private void DrawRoomBorders()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;
        int borderThickness = 4;

        Color borderColor = new Color(woodDark.R, woodDark.G, woodDark.B, 150);

        Graphics.DrawRectangle(0, 0, screenWidth, borderThickness, borderColor);
        Graphics.DrawRectangle(0, 0, borderThickness, screenHeight, borderColor);
        Graphics.DrawRectangle(screenWidth - borderThickness, 0, borderThickness, screenHeight, borderColor);
    }
}
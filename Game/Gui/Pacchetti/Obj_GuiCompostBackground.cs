using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiCompostBackground : GameElement
{
    // Colori palette casa accogliente
    private Color wallColor = new Color(230, 220, 200, 255);
    private Color wallShadow = new Color(200, 190, 170, 255);
    private Color woodDark = new Color(62, 39, 25, 255);
    private Color woodMedium = new Color(101, 67, 43, 255);
    private Color woodLight = new Color(139, 90, 55, 255);
    private Color floorLight = new Color(180, 140, 100, 255);
    private Color floorDark = new Color(150, 110, 70, 255);
    private Color windowFrame = new Color(80, 60, 40, 255);
    private Color skyColor = new Color(135, 206, 235, 255);
    private Color grassColor = new Color(100, 180, 100, 255);

    private float timeOfDay = 0f;

    public Obj_GuiCompostBackground() : base()
    {
        this.roomId = Game.room_compost.id;
        this.guiLayer = true;
        this.depth = -40;
    }

    public override void Update()
    {
        timeOfDay += Time.GetFrameTime() * 0.1f;
    }

    public override void Draw()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        // === MURO PRINCIPALE ===
        Graphics.DrawRectangle(0, 0, screenWidth, screenHeight, wallColor);

        for (int y = 0; y < screenHeight; y += 12)
        {
            Color lineColor = new Color(wallShadow.R, wallShadow.G, wallShadow.B, 20);
            Graphics.DrawLine(0, y, screenWidth, y, lineColor);
        }

        // === PAVIMENTO IN LEGNO ===
        int floorHeight = 120;
        int floorY = screenHeight - floorHeight;

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

        // === FINESTRA A SINISTRA ===
        int windowX = 30;
        int windowY = 40;
        int windowWidth = 140;
        int windowHeight = 180;

        DrawWindow(windowX, windowY, windowWidth, windowHeight);

        // === TAVOLINO PI� LUNGO ===
        int tableX = screenWidth / 2 - 140;  // Pi� largo
        int tableY = screenHeight - 180;
        int tableWidth = 280;  // Molto pi� largo
        int tableHeight = 60;

        DrawTable(tableX, tableY, tableWidth, tableHeight);

        // === SECCHIO COMPOST ===
        int binX = tableX + 15;
        int binY = tableY - 45;
        DrawCompostBin(binX, binY);

        // === DECORAZIONI ===
        DrawWallDecoration();

        // === BORDI STANZA ===
        DrawRoomBorders();

        // === RIGA DI PAVIMENTO IN BASSO PER COPRIRE LA BARRA BIANCA DELLA SCROLLBAR ===
        int bottomFloorHeight = 45;
        Graphics.DrawRectangle(0, screenHeight - bottomFloorHeight, screenWidth, bottomFloorHeight, woodDark);
        // Riga superiore della striscia di pavimento
        Graphics.DrawRectangle(0, screenHeight - bottomFloorHeight, screenWidth, 4, woodMedium);
    }

    private void DrawWindow(int x, int y, int width, int height)
    {
        int frameThickness = 8;
        Graphics.DrawRectangle(x - frameThickness, y - frameThickness,
            width + frameThickness * 2, height + frameThickness * 2, windowFrame);

        Graphics.DrawRectangleGradientV(x, y, width, height / 2,
            skyColor, new Color(180, 220, 255, 255));

        Graphics.DrawRectangle(x, y + height / 2, width, height / 2, grassColor);

        Graphics.DrawLine(x, y + height / 2, x + width, y + height / 2,
            new Color(150, 200, 150, 255));

        DrawDistantPlant(x + width / 2, y + height / 2 + 20);

        Graphics.DrawRectangle(x + width / 2 - 2, y, 4, height, windowFrame);
        Graphics.DrawRectangle(x, y + height / 2 - 2, width, 4, windowFrame);

        Color glassReflection = new Color(255, 255, 255, 30);
        Graphics.DrawRectangle(x + 10, y + 10, 40, 60, glassReflection);

        Graphics.DrawRectangle(x - frameThickness, y + height + frameThickness - 4,
            width + frameThickness * 2, 12, woodMedium);
        Graphics.DrawRectangleGradientH(x - frameThickness, y + height + frameThickness - 4,
            width + frameThickness * 2, 6, woodLight, woodMedium);
    }

    private void DrawDistantPlant(int centerX, int baseY)
    {
        float plantHeight = Game.pianta.Stats.Altezza;
        float maxHeight = Game.pianta.Stats.EffectiveMaxHeight;
        float heightRatio = Math.Clamp(plantHeight / maxHeight, 0f, 1f);

        int visibleHeight = (int)(80 * heightRatio);

        Color plantColor = new Color(80, 150, 80, 255);

        Graphics.DrawRectangle(centerX - 1, baseY - visibleHeight, 2, visibleHeight, plantColor);

        int numLeaves = (int)(heightRatio * 6);
        for (int i = 0; i < numLeaves; i++)
        {
            int leafY = baseY - (visibleHeight * (i + 1) / (numLeaves + 1));
            int leafX = centerX + ((i % 2 == 0) ? -3 : 3);

            Graphics.DrawTriangle(
                new System.Numerics.Vector2(centerX, leafY),
                new System.Numerics.Vector2(leafX - 2, leafY + 3),
                new System.Numerics.Vector2(leafX + 2, leafY + 3),
                new Color(90, 170, 90, 255)
            );
        }
    }

    private void DrawTable(int x, int y, int width, int height)
    {
        Color tableTop = woodMedium;
        Color tableSide = woodDark;

        Graphics.DrawRectangle(x, y, width, 10, tableTop);
        Graphics.DrawRectangle(x, y + 5, width, 5, new Color(woodDark.R, woodDark.G, woodDark.B, 100));

        Graphics.DrawRectangle(x, y + 10, width, 4, tableSide);

        int legWidth = 8;
        int legHeight = 50;

        // Gamba sinistra
        Graphics.DrawRectangle(x + 15, y + 14, legWidth, legHeight, tableSide);
        Graphics.DrawRectangle(x + 15, y + 14, legWidth / 2, legHeight, woodMedium);

        // Gamba centro-sinistra
        Graphics.DrawRectangle(x + width / 3, y + 14, legWidth, legHeight, tableSide);
        Graphics.DrawRectangle(x + width / 3, y + 14, legWidth / 2, legHeight, woodMedium);

        // Gamba centro-destra
        Graphics.DrawRectangle(x + 2 * width / 3, y + 14, legWidth, legHeight, tableSide);
        Graphics.DrawRectangle(x + 2 * width / 3, y + 14, legWidth / 2, legHeight, woodMedium);

        // Gamba destra
        Graphics.DrawRectangle(x + width - 23, y + 14, legWidth, legHeight, tableSide);
        Graphics.DrawRectangle(x + width - 23, y + 14, legWidth / 2, legHeight, woodMedium);

        Graphics.DrawEllipse(x + width / 2, y + legHeight + 20, width / 2, 15,
            new Color(0, 0, 0, 40));
    }

    private void DrawCompostBin(int x, int y)
    {
        int binWidth = 35;
        int binHeight = 40;

        Color binBody = new Color(90, 70, 50, 255);
        Color binRim = new Color(110, 90, 70, 255);
        Color binShadow = new Color(50, 40, 30, 255);

        Graphics.DrawEllipse(x + binWidth / 2, y + binHeight + 2, binWidth / 2 + 4, 6,
            new Color(0, 0, 0, 60));

        Graphics.DrawRectangle(x + 3, y + 5, binWidth - 6, binHeight - 5, binBody);
        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x + 3, y + 5),
            new System.Numerics.Vector2(x, y + binHeight),
            new System.Numerics.Vector2(x + 3, y + binHeight),
            binShadow
        );
        Graphics.DrawTriangle(
            new System.Numerics.Vector2(x + binWidth - 3, y + 5),
            new System.Numerics.Vector2(x + binWidth, y + binHeight),
            new System.Numerics.Vector2(x + binWidth - 3, y + binHeight),
            binShadow
        );

        Graphics.DrawEllipse(x + binWidth / 2, y + 5, binWidth / 2, 4, binRim);
        Graphics.DrawEllipse(x + binWidth / 2, y + 3, binWidth / 2, 3,
            new Color(binRim.R, binRim.G, binRim.B, 200));

        Color leafSymbol = new Color(100, 180, 100, 200);
        Graphics.DrawCircle(x + binWidth / 2 - 5, y + 20, 4, leafSymbol);
        Graphics.DrawCircle(x + binWidth / 2 + 5, y + 20, 4, leafSymbol);
        Graphics.DrawCircle(x + binWidth / 2, y + 26, 4, leafSymbol);
    }

    private void DrawWallDecoration()
    {
        int frameX = Rendering.camera.screenWidth - 80;
        int frameY = 50;
        int frameW = 50;
        int frameH = 40;

        Graphics.DrawRectangle(frameX, frameY, frameW, frameH, woodDark);
        Graphics.DrawRectangle(frameX + 3, frameY + 3, frameW - 6, frameH - 6,
            new Color(200, 220, 200, 255));

        Graphics.DrawCircle(frameX + frameW / 2, frameY + frameH / 2, 8,
            new Color(100, 180, 100, 255));
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
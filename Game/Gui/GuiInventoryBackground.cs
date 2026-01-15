using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;

namespace Plants;

public class GuiInventoryBackground : GameElement
{
    // Colori stile legno
    private Color woodDark = new Color(62, 39, 25, 255);       // Marrone scuro cornice
    private Color woodMedium = new Color(101, 67, 43, 255);    // Marrone medio
    private Color woodLight = new Color(139, 90, 55, 255);     // Marrone chiaro
    private Color woodHighlight = new Color(166, 118, 76, 255); // Highlight legno
    private Color woodShadow = new Color(41, 26, 17, 255);     // Ombra scura

    private int borderThickness = 12;

    public GuiInventoryBackground() : base()
    {
        this.roomId = Game.inventoryRoom.id;
        this.guiLayer = true;
        this.depth = -40; // Dietro alla griglia ma davanti ad altri elementi
    }

    public override void Update()
    {
        // Nessuna logica di update necessaria
    }

    public override void Draw()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        // Background principale legno chiaro
        Graphics.DrawRectangle(0, 0, screenWidth, screenHeight, woodMedium);

        // Pattern venature legno (linee orizzontali sottili)
        for (int y = 0; y < screenHeight; y += 8)
        {
            Color lineColor = (y % 24 == 0) ? woodLight : woodHighlight;
            lineColor.A = 40;
            Graphics.DrawLine(0, y, screenWidth, y, lineColor);
        }

        // Cornice esterna marrone scuro
        // Top
        Graphics.DrawRectangle(0, 0, screenWidth, borderThickness, woodDark);
        // Bottom
        Graphics.DrawRectangle(0, screenHeight - borderThickness, screenWidth, borderThickness, woodDark);
        // Left
        Graphics.DrawRectangle(0, 0, borderThickness, screenHeight, woodDark);
        // Right
        Graphics.DrawRectangle(screenWidth - borderThickness, 0, borderThickness, screenHeight, woodDark);

        // Bordo interno evidenziato (effetto rilievo)
        int innerX = borderThickness;
        int innerY = borderThickness;
        int innerW = screenWidth - borderThickness * 2;
        int innerH = screenHeight - borderThickness * 2;

        // Linea chiara in alto e sinistra (luce)
        Graphics.DrawLine(innerX, innerY, innerX + innerW, innerY, woodHighlight);
        Graphics.DrawLine(innerX, innerY, innerX, innerY + innerH, woodHighlight);

        // Linea scura in basso e destra (ombra)
        Graphics.DrawLine(innerX, innerY + innerH, innerX + innerW, innerY + innerH, woodShadow);
        Graphics.DrawLine(innerX + innerW, innerY, innerX + innerW, innerY + innerH, woodShadow);

        // Angoli decorativi (piccoli quadrati scuri)
        int cornerSize = borderThickness + 4;
        Graphics.DrawRectangle(0, 0, cornerSize, cornerSize, woodShadow);
        Graphics.DrawRectangle(screenWidth - cornerSize, 0, cornerSize, cornerSize, woodShadow);
        Graphics.DrawRectangle(0, screenHeight - cornerSize, cornerSize, cornerSize, woodShadow);
        Graphics.DrawRectangle(screenWidth - cornerSize, screenHeight - cornerSize, cornerSize, cornerSize, woodShadow);
    }
}

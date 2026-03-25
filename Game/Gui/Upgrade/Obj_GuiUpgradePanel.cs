using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Interact;
using System;

namespace Plants;

public class Obj_GuiUpgradePanel : GameElement
{
    private Color wallColor = new Color(230, 220, 200, 255);
    private Color wallShadow = new Color(200, 190, 170, 255);
    private Color woodDark = new Color(62, 39, 25, 255);
    private Color woodMedium = new Color(101, 67, 43, 255);
    private Color woodLight = new Color(139, 90, 55, 255);
    private Color floorLight = new Color(180, 140, 100, 255);
    private Color floorDark = new Color(150, 110, 70, 255);

    private Color panelBg = new Color(40, 35, 30, 230);
    private Color panelBorder = new Color(101, 67, 43, 255);
    private Color textWhite = new Color(255, 255, 255, 255);
    private Color textGray = new Color(180, 180, 180, 255);
    private Color tickFilled = new Color(100, 200, 100, 255);
    private Color tickEmpty = new Color(60, 60, 60, 200);
    private Color btnGreen = new Color(80, 160, 80, 255);
    private Color btnGreenHover = new Color(100, 190, 100, 255);
    private Color btnDisabled = new Color(80, 70, 60, 255);
    private Color leafColor = new Color(100, 180, 100, 255);

    private UpgradeType[] upgradeTypes = {
        UpgradeType.Innaffiatoio,
        UpgradeType.Inventario,
        UpgradeType.SpazioPacchetti
    };

    private string[] descriptions = {
        "Capacita' acqua",
        "Spazio semi",
        "Slot pacchetti"
    };

    private Rectangle[] buyButtons = new Rectangle[3];
    private int hoveredButton = -1;

    public Obj_GuiUpgradePanel() : base()
    {
        this.roomId = Game.room_upgrade.id;
        this.guiLayer = true;
        this.depth = -50;
    }

    public override void Update()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        hoveredButton = -1;
        for (int i = 0; i < buyButtons.Length; i++)
        {
            var b = buyButtons[i];
            if (b.Width > 0 && mx >= b.X && mx <= b.X + b.Width &&
                my >= b.Y && my <= b.Y + b.Height)
            {
                hoveredButton = i;
                break;
            }
        }

        if (Input.IsMouseButtonPressed(MouseButton.Left) && hoveredButton != -1)
        {
            var type = upgradeTypes[hoveredButton];
            if (UpgradeSystem.TryUpgrade(type))
            {
                Console.WriteLine($"Upgrade {UpgradeSystem.GetName(type)} -> Livello {UpgradeSystem.GetLevel(type)}");
            }
        }
    }

    public override void Draw()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        DrawBackground(screenW, screenH);

        // Titolo foglie
        Graphics.DrawText($"Foglie: {Game.pianta.Stats.FoglieAccumulate}", 10, 10, 16, leafColor);

        // Titolo
        string title = "Upgrade";
        int titleW = title.Length * 8;
        Graphics.DrawText(title, screenW / 2 - titleW / 2, 40, 18, textWhite);

        // Pannello upgrade
        int panelX = 20;
        int panelY = 70;
        int panelW = screenW - 40;
        int rowHeight = 70;
        int panelH = upgradeTypes.Length * rowHeight + 20;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.08f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.08f, 8, 2, panelBorder);

        for (int i = 0; i < upgradeTypes.Length; i++)
        {
            DrawUpgradeRow(panelX + 10, panelY + 10 + i * rowHeight, panelW - 20, rowHeight - 10, i);

            if (i < upgradeTypes.Length - 1)
            {
                int lineY = panelY + 10 + (i + 1) * rowHeight - 5;
                Graphics.DrawLine(panelX + 15, lineY, panelX + panelW - 15, lineY,
                    new Color(80, 70, 60, 150));
            }
        }
    }

    private void DrawUpgradeRow(int x, int y, int width, int height, int index)
    {
        var type = upgradeTypes[index];
        int level = UpgradeSystem.GetLevel(type);
        int cost = UpgradeSystem.GetCost(type);
        bool maxed = level >= UpgradeSystem.MaxLevel;
        bool canBuy = UpgradeSystem.CanUpgrade(type);

        // Nome upgrade
        Graphics.DrawText(UpgradeSystem.GetName(type), x + 5, y + 5, 12, textWhite);

        // Descrizione
        string desc = descriptions[index];
        string valueText = type switch
        {
            UpgradeType.Innaffiatoio => $"{(int)UpgradeSystem.GetWaterMax()}",
            UpgradeType.Inventario => $"{UpgradeSystem.GetMaxSeeds()}",
            UpgradeType.SpazioPacchetti => $"{UpgradeSystem.GetMaxPackages()}",
            _ => ""
        };
        Graphics.DrawText($"{desc}: {valueText}", x + 5, y + 20, 9, textGray);

        // Tacche livello
        int tickStartX = x + 5;
        int tickY = y + 36;
        int tickW = 18;
        int tickH = 10;
        int tickSpacing = 4;

        for (int t = 0; t < UpgradeSystem.MaxLevel; t++)
        {
            int tx = tickStartX + t * (tickW + tickSpacing);
            Color tickColor = t < level ? tickFilled : tickEmpty;

            Graphics.DrawRectangleRounded(
                new Rectangle(tx, tickY, tickW, tickH),
                0.3f, 4, tickColor);

            if (t < level)
            {
                Graphics.DrawRectangleRoundedLines(
                    new Rectangle(tx, tickY, tickW, tickH),
                    0.3f, 4, 1, new Color(130, 230, 130, 255));
            }
        }

        // Bottone acquisto
        int btnW = 70;
        int btnH = 28;
        int btnX = x + width - btnW - 5;
        int btnY = y + (height - btnH) / 2;

        buyButtons[index] = new Rectangle(btnX, btnY, btnW, btnH);

        if (maxed)
        {
            Graphics.DrawRectangleRounded(
                new Rectangle(btnX, btnY, btnW, btnH),
                0.3f, 6, btnDisabled);
            string maxText = "MAX";
            Graphics.DrawText(maxText, btnX + btnW / 2 - 12, btnY + 8, 11, textGray);
        }
        else
        {
            bool hovered = hoveredButton == index;
            Color btnColor = canBuy
                ? (hovered ? btnGreenHover : btnGreen)
                : btnDisabled;

            Graphics.DrawRectangleRounded(
                new Rectangle(btnX, btnY, btnW, btnH),
                0.3f, 6, btnColor);

            if (canBuy)
            {
                Graphics.DrawRectangleRoundedLines(
                    new Rectangle(btnX, btnY, btnW, btnH),
                    0.3f, 6, 2, new Color(130, 220, 130, 255));
            }

            string btnText = $"+ {cost}";
            int textW = btnText.Length * 5;
            Graphics.DrawText(btnText, btnX + btnW / 2 - textW / 2, btnY + 5, 10,
                canBuy ? textWhite : textGray);

            // Icona foglia piccola
            Graphics.DrawCircle(btnX + btnW / 2 + textW / 2 + 5, btnY + 19, 3, leafColor);
        }
    }

    private void DrawBackground(int screenW, int screenH)
    {
        // Muro
        Graphics.DrawRectangle(0, 0, screenW, screenH, wallColor);
        for (int y = 0; y < screenH; y += 12)
        {
            Graphics.DrawLine(0, y, screenW, y, new Color(wallShadow.R, wallShadow.G, wallShadow.B, 20));
        }

        // Pavimento
        int floorHeight = 120;
        int floorY = screenH - floorHeight;
        int plankHeight = 15;
        for (int i = 0; i < floorHeight / plankHeight; i++)
        {
            Color plankColor = (i % 2 == 0) ? floorLight : floorDark;
            Graphics.DrawRectangle(0, floorY + i * plankHeight, screenW, plankHeight, plankColor);
            Graphics.DrawLine(0, floorY + i * plankHeight, screenW, floorY + i * plankHeight, woodDark);
        }

        // Battiscopa
        Graphics.DrawRectangle(0, floorY - 8, screenW, 8, woodDark);
        Graphics.DrawLine(0, floorY - 8, screenW, floorY - 8, woodMedium);

        // Bordi stanza
        int borderThickness = 4;
        Color borderColor = new Color(woodDark.R, woodDark.G, woodDark.B, 150);
        Graphics.DrawRectangle(0, 0, screenW, borderThickness, borderColor);
        Graphics.DrawRectangle(0, 0, borderThickness, screenH, borderColor);
        Graphics.DrawRectangle(screenW - borderThickness, 0, borderThickness, screenH, borderColor);

        // Pavimento basso
        int bottomFloorHeight = 45;
        Graphics.DrawRectangle(0, screenH - bottomFloorHeight, screenW, bottomFloorHeight, woodDark);
        Graphics.DrawRectangle(0, screenH - bottomFloorHeight, screenW, 4, woodMedium);
    }
}

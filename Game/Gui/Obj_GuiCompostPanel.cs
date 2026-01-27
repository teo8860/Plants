using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Interact;
using System;
using System.Collections.Generic;

namespace Plants;

public class Obj_GuiCompostPanel : GameElement
{
    private SeedPackageRarity[] rarities = {
        SeedPackageRarity.Common,
        SeedPackageRarity.Uncommon,
        SeedPackageRarity.Rare,
        SeedPackageRarity.Epic,
        SeedPackageRarity.Legendary
    };

    // Colori corretti per le rarità
    private Dictionary<SeedPackageRarity, Color> rarityColors = new()
    {
        { SeedPackageRarity.Common, new Color(200, 200, 200, 255) },
        { SeedPackageRarity.Uncommon, new Color(80, 200, 80, 255) },
        { SeedPackageRarity.Rare, new Color(80, 150, 255, 255) },
        { SeedPackageRarity.Epic, new Color(180, 80, 255, 255) },
        { SeedPackageRarity.Legendary, new Color(255, 180, 50, 255) }
    };

    private Dictionary<SeedPackageRarity, string> rarityNames = new()
    {
        { SeedPackageRarity.Common, "Comune" },
        { SeedPackageRarity.Uncommon, "Non Comune" },
        { SeedPackageRarity.Rare, "Raro" },
        { SeedPackageRarity.Epic, "Epico" },
        { SeedPackageRarity.Legendary, "Leggendario" }
    };

    // Menu combobox
    private bool isMenuOpen = false;
    private Rectangle binRect;
    private Rectangle[] menuItems = new Rectangle[5];
    private Rectangle[] packageButtons = new Rectangle[10];

    public Obj_GuiCompostPanel() : base()
    {
        this.guiLayer = true;
        this.roomId = Game.room_compost.id;
        this.depth = -50;
    }

    public override void Update()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        int tableX = screenWidth / 2 - 140;
        int tableY = screenHeight - 180;
        int binX = tableX + 15;
        int binY = tableY - 45;

        // Area cliccabile del secchio
        binRect = new Rectangle(binX, binY, 35, 40);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Click sul secchio - apri/chiudi menu
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (mx >= binRect.X && mx <= binRect.X + binRect.Width &&
                my >= binRect.Y && my <= binRect.Y + binRect.Height)
            {
                isMenuOpen = !isMenuOpen;
            }
            else if (isMenuOpen)
            {
                // Click su item del menu
                bool clickedOnMenu = false;
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (mx >= menuItems[i].X && mx <= menuItems[i].X + menuItems[i].Width &&
                        my >= menuItems[i].Y && my <= menuItems[i].Y + menuItems[i].Height)
                    {
                        clickedOnMenu = true;

                        // Crea pacchetto se possibile
                        if (CompostSystem.CanCreatePackage(rarities[i]))
                        {
                            CompostSystem.CreatePackage(rarities[i]);
                            Console.WriteLine($"Pacchetto {rarities[i]} creato!");
                        }
                        isMenuOpen = false;
                        break;
                    }
                }

                // Chiudi menu se click fuori
                if (!clickedOnMenu)
                {
                    isMenuOpen = false;
                }
            }
            else
            {
                // Click sui pacchetti sul tavolo per aprirli
                var packages = CompostSystem.GetAvailablePackages();
                for (int i = 0; i < packages.Count && i < packageButtons.Length; i++)
                {
                    if (mx >= packageButtons[i].X && mx <= packageButtons[i].X + packageButtons[i].Width &&
                        my >= packageButtons[i].Y && my <= packageButtons[i].Y + packageButtons[i].Height)
                    {
                        SeedType seedType = CompostSystem.OpenPackage(packages[i]);
                        Seed newSeed = new Seed(seedType);
                        Inventario.get().AddSeed(newSeed);
                        Console.WriteLine($"Pacchetto aperto! Ricevuto: {seedType}");
                        break;
                    }
                }
            }
        }
    }

    public override void Draw()
    {
        // === INFO FOGLIE ===
        Graphics.DrawText($"Foglie: {Game.pianta.Stats.FoglieAttuali}", 10, 10, 16, new Color(100, 180, 100, 255));

        // === HOVER SUL SECCHIO ===
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        bool binHovered = mx >= binRect.X && mx <= binRect.X + binRect.Width &&
                         my >= binRect.Y && my <= binRect.Y + binRect.Height;

        if (binHovered && !isMenuOpen)
        {
            // Indicatore hover
            Graphics.DrawRectangleRoundedLines(binRect, 0.2f, 6, 2, new Color(255, 255, 100, 200));
            Graphics.DrawText("Clicca per creare", (int)binRect.X - 15, (int)binRect.Y - 15, 9, Color.White);
        }


        // === PACCHETTI SUL TAVOLO ===
        DrawPackagesOnTable();

        // === MENU COMBOBOX ===
        if (isMenuOpen)
        {
            DrawComboboxMenu();
        }

    }

    private void DrawComboboxMenu()
    {
        int menuX = (int)binRect.X + (int)binRect.Width + 5;
        int menuY = (int)binRect.Y;
        int menuWidth = 140;
        int itemHeight = 32;
        int menuHeight = rarities.Length * itemHeight;

        // Sfondo menu
        Graphics.DrawRectangleRounded(
            new Rectangle(menuX, menuY, menuWidth, menuHeight),
            0.15f, 8,
            new Color(40, 35, 30, 240)
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(menuX, menuY, menuWidth, menuHeight),
            0.15f, 8, 2,
            new Color(101, 67, 43, 255)
        );

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Items del menu
        for (int i = 0; i < rarities.Length; i++)
        {
            int itemY = menuY + i * itemHeight;
            menuItems[i] = new Rectangle(menuX, itemY, menuWidth, itemHeight);

            bool canCreate = CompostSystem.CanCreatePackage(rarities[i]);
            bool hovered = mx >= menuItems[i].X && mx <= menuItems[i].X + menuItems[i].Width &&
                          my >= menuItems[i].Y && my <= menuItems[i].Y + menuItems[i].Height;

            // Background item
            if (hovered && canCreate)
            {
                Graphics.DrawRectangle(menuX + 2, itemY + 2, menuWidth - 4, itemHeight - 4,
                    new Color(60, 50, 40, 255));
            }

            // Colore rarità (indicatore)
            Graphics.DrawRectangle(menuX + 5, itemY + 8, 4, itemHeight - 16, rarityColors[rarities[i]]);

            // Testo
            var package = new SeedPackage(rarities[i]);
            string itemText = $"{rarityNames[rarities[i]]}";
            string costText = $"{package.LeavesRequired} foglie";

            Color textColor = canCreate ? Color.White : new Color(120, 100, 80, 255);
            Color costColor = canCreate ? rarityColors[rarities[i]] : new Color(100, 80, 60, 255);

            Graphics.DrawText(itemText, menuX + 15, itemY + 6, 10, textColor);
            Graphics.DrawText(costText, menuX + 15, itemY + 18, 8, costColor);

            // Icona "lock" se non disponibile
            if (!canCreate)
            {
                Graphics.DrawText("X", menuX + menuWidth - 18, itemY + 10, 12, new Color(200, 80, 80, 255));
            }
        }
    }

    private void DrawPackagesOnTable()
    {
        var packages = CompostSystem.GetAvailablePackages();

        if (packages.Count == 0)
        {
            Console.WriteLine("Nessun pacchetto da disegnare");
            return;
        }

        Console.WriteLine($"Disegno {packages.Count} pacchetti");

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        int tableX = screenWidth / 2 - 140;
        int tableY = screenHeight - 180;

        // Posizione iniziale per i pacchetti (dopo il secchio)
        int startX = tableX + 70;
        int startY = tableY - 35;

        int packageWidth = 30;
        int packageHeight = 38;
        int spacing = 8;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        for (int i = 0; i < packages.Count && i < 10; i++)
        {
            int pkgX = startX + i * (packageWidth + spacing);
            int pkgY = startY;

            packageButtons[i] = new Rectangle(pkgX, pkgY, packageWidth, packageHeight);

            bool hovered = mx >= pkgX && mx <= pkgX + packageWidth &&
                          my >= pkgY && my <= pkgY + packageHeight;

            DrawPackage(pkgX, pkgY, packageWidth, packageHeight, packages[i].Rarity, hovered);
        }
    }

    private void DrawPackage(int x, int y, int width, int height, SeedPackageRarity rarity, bool hovered)
    {
        Color pkgColor = rarityColors[rarity];

        // Ombra
        Graphics.DrawEllipse(x + width / 2, y + height + 2, width / 2 + 2, 4,
            new Color(0, 0, 0, 80));

        // Corpo pacchetto (busta)
        Color bodyColor = hovered ?
            new Color((byte)(pkgColor.R * 0.9f), (byte)(pkgColor.G * 0.9f), (byte)(pkgColor.B * 0.9f), 255) :
            new Color((byte)(pkgColor.R * 0.7f), (byte)(pkgColor.G * 0.7f), (byte)(pkgColor.B * 0.7f), 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, width, height),
            0.15f, 6, bodyColor
        );

        // Bordo luminoso
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(x, y, width, height),
            0.15f, 6, hovered ? 3 : 2, pkgColor
        );

        // Lembo superiore della busta
        Graphics.DrawRectangle(x + 3, y + 3, width - 6, 8,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 120));

        // Simbolo seme (tre cerchi)
        int centerX = x + width / 2;
        int centerY = y + height / 2 + 4;

        Graphics.DrawCircle(centerX, centerY, 4,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 200));
        Graphics.DrawCircle(centerX - 3, centerY - 3, 3,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 180));
        Graphics.DrawCircle(centerX + 3, centerY - 3, 3,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 180));

        // Effetto brillantezza per rarità alte
        if (rarity == SeedPackageRarity.Legendary || rarity == SeedPackageRarity.Epic)
        {
            float time = (float)Raylib_CSharp.Time.GetTime();
            byte sparkleAlpha = (byte)(100 + Math.Sin(time * 3) * 80);

            Graphics.DrawCircle(x + width - 6, y + 6, 2, new Color(255, 255, 255, sparkleAlpha));
            Graphics.DrawCircle(x + 6, y + height - 8, 1, new Color(255, 255, 255, (byte)(sparkleAlpha * 0.7f)));
        }

        // Tooltip hover
        if (hovered)
        {
            string tooltip = $"Clicca per aprire\n{rarityNames[rarity]}";
            int tooltipX = x - 20;
            int tooltipY = y - 25;

            Graphics.DrawText(tooltip, tooltipX, tooltipY, 8, Color.White);
        }
    }
}
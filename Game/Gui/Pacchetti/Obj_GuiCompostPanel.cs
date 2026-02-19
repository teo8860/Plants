using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Interact;
using Raylib_CSharp;
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

    private bool isMenuOpen = false;
    private Rectangle binRect;
    private Rectangle[] menuItems = new Rectangle[5];
    private Rectangle[] packageButtons = new Rectangle[4]; // Massimo 4
    private Rectangle[] progressButtons = new Rectangle[4]; // Per pacchetti in creazione

    public Obj_GuiCompostPanel() : base()
    {
        this.guiLayer = true;
        this.roomId = Game.room_compost.id;
        this.depth = -50;
    }

    public override void Update()
    {
        // Aggiorna il sistema compost
        CompostSystem.Update(Time.GetFrameTime());

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        int tableX = screenWidth / 2 - 140;
        int tableY = screenHeight - 180;
        int binX = tableX + 15;
        int binY = tableY - 45;

        binRect = new Rectangle(binX, binY, 35, 40);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (mx >= binRect.X && mx <= binRect.X + binRect.Width &&
                my >= binRect.Y && my <= binRect.Y + binRect.Height)
            {
                isMenuOpen = !isMenuOpen;
            }
            else if (isMenuOpen)
            {
                bool clickedOnMenu = false;
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (mx >= menuItems[i].X && mx <= menuItems[i].X + menuItems[i].Width &&
                        my >= menuItems[i].Y && my <= menuItems[i].Y + menuItems[i].Height)
                    {
                        clickedOnMenu = true;

                        if (CompostSystem.StartPackageCreation(rarities[i]))
                        {
                            Console.WriteLine($"Creazione pacchetto {rarities[i]} avviata!");
                        }
                        isMenuOpen = false;
                        break;
                    }
                }

                if (!clickedOnMenu)
                {
                    isMenuOpen = false;
                }
            }
            else
            {
                // Click sui pacchetti pronti per aprirli
                var packages = CompostSystem.GetAvailablePackages();
                for (int i = 0; i < packages.Count && i < packageButtons.Length; i++)
                {
                    if (mx >= packageButtons[i].X && mx <= packageButtons[i].X + packageButtons[i].Width &&
                        my >= packageButtons[i].Y && my <= packageButtons[i].Y + packageButtons[i].Height)
                    {
                        Seed seed = CompostSystem.OpenPackage(packages[i]);
                        Inventario.get().AddSeed(seed);
                        Console.WriteLine($"Pacchetto aperto! Ricevuto: {seed}");

                        Game.packOpening.StartAnimation(seed, packages[i].Rarity);

                        break;
                    }
                }
            }
        }
    }

    public override void Draw()
    {
        // Info foglie e slot
        int totalPackages = CompostSystem.GetTotalPackageCount();
        Graphics.DrawText($"Foglie: {Game.pianta.Stats.FoglieAccumulate}", 10, 10, 16, new Color(100, 180, 100, 255));
        Graphics.DrawText($"Slot: {totalPackages}/4", 10, 30, 14,
            totalPackages >= 4 ? new Color(255, 100, 100, 255) : new Color(200, 200, 200, 255));

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        bool binHovered = mx >= binRect.X && mx <= binRect.X + binRect.Width &&
                         my >= binRect.Y && my <= binRect.Y + binRect.Height;

        if (binHovered && !isMenuOpen)
        {
            Graphics.DrawRectangleRoundedLines(binRect, 0.2f, 6, 2, new Color(255, 255, 100, 200));

            string hoverText = totalPackages >= 4 ? "Slot pieni!" : "Clicca per creare";
            Graphics.DrawText(hoverText, (int)binRect.X - 15, (int)binRect.Y - 15, 9, Color.White);
        }

        DrawPackagesOnTable();

        if (isMenuOpen)
        {
            DrawComboboxMenu();
        }

    }

    private void DrawComboboxMenu()
    {
        int menuX = (int)binRect.X + (int)binRect.Width + 5;
        int menuY = (int)binRect.Y;
        int menuWidth = 150;
        int itemHeight = 36;
        int menuHeight = rarities.Length * itemHeight;

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

        for (int i = 0; i < rarities.Length; i++)
        {
            int itemY = menuY + i * itemHeight;
            menuItems[i] = new Rectangle(menuX, itemY, menuWidth, itemHeight);

            bool canCreate = CompostSystem.CanCreatePackage(rarities[i]);
            bool hovered = mx >= menuItems[i].X && mx <= menuItems[i].X + menuItems[i].Width &&
                          my >= menuItems[i].Y && my <= menuItems[i].Y + menuItems[i].Height;

            if (hovered && canCreate)
            {
                Graphics.DrawRectangle(menuX + 2, itemY + 2, menuWidth - 4, itemHeight - 4,
                    new Color(60, 50, 40, 255));
            }

            Graphics.DrawRectangle(menuX + 5, itemY + 10, 4, itemHeight - 20, rarityColors[rarities[i]]);

            var package = new SeedPackage(rarities[i]);
            string itemText = rarityNames[rarities[i]];
            string costText = $"{package.LeavesRequired} foglie";

            // Tempo di creazione
            var tempPkg = new PackageInProgress(rarities[i]);
            string timeText = $"{tempPkg.TimeRequired}s";

            Color textColor = canCreate ? Color.White : new Color(120, 100, 80, 255);
            Color costColor = canCreate ? rarityColors[rarities[i]] : new Color(100, 80, 60, 255);

            Graphics.DrawText(itemText, menuX + 15, itemY + 4, 10, textColor);
            Graphics.DrawText(costText, menuX + 15, itemY + 16, 8, costColor);
            Graphics.DrawText(timeText, menuX + 15, itemY + 26, 7, new Color(200, 180, 160, 255));

            if (!canCreate)
            {
                string blockReason = CompostSystem.GetTotalPackageCount() >= 4 ? "PIENO" : "X";
                Graphics.DrawText(blockReason, menuX + menuWidth - 35, itemY + 12, 10, new Color(200, 80, 80, 255));
            }
        }
    }

    private void DrawPackagesOnTable()
    {
        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;

        int tableX = screenWidth / 2 - 140;
        int tableY = screenHeight - 180;

        int startX = tableX + 70;
        int startY = tableY - 35;

        int packageWidth = 32;
        int packageHeight = 40;
        int spacing = 10;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        int slotIndex = 0;

        // Disegna pacchetti in creazione (con barra progresso)
        var inProgress = CompostSystem.GetPackagesInProgress();
        for (int i = 0; i < inProgress.Count && slotIndex < 4; i++)
        {
            int pkgX = startX + slotIndex * (packageWidth + spacing);
            int pkgY = startY;

            progressButtons[slotIndex] = new Rectangle(pkgX, pkgY, packageWidth, packageHeight);

            DrawPackageInProgress(pkgX, pkgY, packageWidth, packageHeight, inProgress[i]);
            slotIndex++;
        }

        // Disegna pacchetti pronti
        var packages = CompostSystem.GetAvailablePackages();
        for (int i = 0; i < packages.Count && slotIndex < 4; i++)
        {
            int pkgX = startX + slotIndex * (packageWidth + spacing);
            int pkgY = startY;

            packageButtons[i] = new Rectangle(pkgX, pkgY, packageWidth, packageHeight);

            bool hovered = mx >= pkgX && mx <= pkgX + packageWidth &&
                          my >= pkgY && my <= pkgY + packageHeight;

            DrawPackage(pkgX, pkgY, packageWidth, packageHeight, packages[i].Rarity, hovered);
            slotIndex++;
        }
    }

    private void DrawPackageInProgress(int x, int y, int width, int height, PackageInProgress package)
    {
        Color pkgColor = rarityColors[package.Rarity];

        // Ombra
        Graphics.DrawEllipse(x + width / 2, y + height + 2, width / 2 + 2, 4,
            new Color(0, 0, 0, 60));

        // Corpo pacchetto (più trasparente)
        Color bodyColor = new Color((byte)(pkgColor.R * 0.4f), (byte)(pkgColor.G * 0.4f), (byte)(pkgColor.B * 0.4f), 200);

        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, width, height),
            0.15f, 6, bodyColor
        );

        // Bordo pulsante
        float pulseAlpha = (float)(Math.Sin(Time.GetTime() * 3) * 0.3f + 0.7f);
        Color borderColor = new Color(pkgColor.R, pkgColor.G, pkgColor.B, (byte)(255 * pulseAlpha));

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(x, y, width, height),
            0.15f, 6, 2, borderColor
        );

        // Barra progresso
        int barHeight = 6;
        int barY = y + height - barHeight - 4;
        int barWidth = width - 8;
        int barX = x + 4;

        // Background barra
        Graphics.DrawRectangle(barX, barY, barWidth, barHeight, new Color(40, 40, 40, 200));

        // Progresso
        int progressWidth = (int)(barWidth * package.Progress);
        Graphics.DrawRectangle(barX, barY, progressWidth, barHeight, pkgColor);

        // Tempo rimanente
        float timeLeft = package.TimeRequired - package.TimeElapsed;
        string timeText = $"{(int)timeLeft}s";
        Graphics.DrawText(timeText, x + width / 2 - 6, y + height / 2 - 4, 8, Color.White);
    }

    private void DrawPackage(int x, int y, int width, int height, SeedPackageRarity rarity, bool hovered)
    {
        Color pkgColor = rarityColors[rarity];

        Graphics.DrawEllipse(x + width / 2, y + height + 2, width / 2 + 2, 4,
            new Color(0, 0, 0, 80));

        Color bodyColor = hovered ?
            new Color((byte)(pkgColor.R * 0.9f), (byte)(pkgColor.G * 0.9f), (byte)(pkgColor.B * 0.9f), 255) :
            new Color((byte)(pkgColor.R * 0.7f), (byte)(pkgColor.G * 0.7f), (byte)(pkgColor.B * 0.7f), 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, width, height),
            0.15f, 6, bodyColor
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(x, y, width, height),
            0.15f, 6, hovered ? 3 : 2, pkgColor
        );

        Graphics.DrawRectangle(x + 3, y + 3, width - 6, 8,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 120));

        int centerX = x + width / 2;
        int centerY = y + height / 2 + 4;

        Graphics.DrawCircle(centerX, centerY, 4,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 200));
        Graphics.DrawCircle(centerX - 3, centerY - 3, 3,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 180));
        Graphics.DrawCircle(centerX + 3, centerY - 3, 3,
            new Color(pkgColor.R, pkgColor.G, pkgColor.B, 180));

        if (rarity == SeedPackageRarity.Legendary || rarity == SeedPackageRarity.Epic)
        {
            float time = (float)Time.GetTime();
            byte sparkleAlpha = (byte)(100 + Math.Sin(time * 3) * 80);

            Graphics.DrawCircle(x + width - 6, y + 6, 2, new Color(255, 255, 255, sparkleAlpha));
            Graphics.DrawCircle(x + 6, y + height - 8, 1, new Color(255, 255, 255, (byte)(sparkleAlpha * 0.7f)));
        }

        if (hovered)
        {
            string tooltip = $"Clicca per aprire";
            Graphics.DrawText(tooltip, x - 15, y - 15, 8, Color.White);
        }
    }
}
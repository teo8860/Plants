using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Interact;

namespace Plants;

public class Obj_GuiCompostPanel : GameElement
{
    private Rectangle[] createButtons;
    private Rectangle[] openButtons;
    private SeedPackageRarity[] rarities = { SeedPackageRarity.Common, SeedPackageRarity.Uncommon, SeedPackageRarity.Rare, SeedPackageRarity.Epic, SeedPackageRarity.Legendary };

    public Obj_GuiCompostPanel() : base()
    {
        this.guiLayer = true;
        this.roomId = Game.room_compost.id;
        createButtons = new Rectangle[rarities.Length];
        for (int i = 0; i < rarities.Length; i++)
        {
            createButtons[i] = new Rectangle(10, 100 + i * 40, 150, 30);
        }
        openButtons = new Rectangle[10]; // Assume max 10 packages
    }

    public override void Update()
    {
        // Create buttons
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        for (int i = 0; i < rarities.Length; i++)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left) && 
                (mx >= createButtons[i].X && mx <= createButtons[i].X + createButtons[i].Width && my >= createButtons[i].Y && my <= createButtons[i].Y + createButtons[i].Height ))
            {
                if (CompostSystem.CanCreatePackage(rarities[i]))
                {
                    CompostSystem.CreatePackage(rarities[i]);
                }
            }
        }

        // Open buttons
        var packages = CompostSystem.GetAvailablePackages();
        for (int i = 0; i < packages.Count && i < openButtons.Length; i++)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left) && (mx >= openButtons[i].X && mx <= openButtons[i].X + openButtons[i].Width && my >= openButtons[i].Y && my <= openButtons[i].Y + openButtons[i].Height))
            {
                SeedType seedType = CompostSystem.OpenPackage(packages[i]);
                Seed newSeed = new Seed(seedType);
                Inventario.get().AddSeed(newSeed);
            }
        }
    }

    public override void Draw()
    {
        // Draw leaves count
        Graphics.DrawText($"Leaves: {CompostSystem.GetCollectedLeaves()}", 10, 10, 20, Color.White);

        // Draw available packages and open buttons
        var packages = CompostSystem.GetAvailablePackages();
        for (int i = 0; i < packages.Count && i < openButtons.Length; i++)
        {
            Graphics.DrawText($"{packages[i]} Package", 10, 40 + i * 30, 20, Color.White);
            openButtons[i] = new Rectangle(200, 40 + i * 30, 80, 20);
            Graphics.DrawRectangleRec(openButtons[i], Color.Blue);
            Graphics.DrawText("Open", (int)openButtons[i].X + 5, (int)openButtons[i].Y + 2, 16, Color.White);
        }

        // Draw create buttons
        for (int i = 0; i < rarities.Length; i++)
        {
            Color buttonColor = CompostSystem.CanCreatePackage(rarities[i]) ? Color.Green : Color.Gray;
            Graphics.DrawRectangleRec(createButtons[i], buttonColor);
            Graphics.DrawText($"Create {rarities[i]}", (int)createButtons[i].X + 5, (int)createButtons[i].Y + 5, 20, Color.Black);
        }
    }
}
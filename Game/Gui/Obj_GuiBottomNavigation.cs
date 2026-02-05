using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;

namespace Plants;

public enum NavigationTab
{
    MainGame,
    Inventory,
    Compost
}

public class Obj_GuiBottomNavigation : GameElement
{
    private List<NavTab> tabs = new();
    private NavigationTab currentTab = NavigationTab.MainGame;

    private int barHeight = 35;
    private int tabWidth = 120;
    private int tabSpacing = 10;
    private int hoveredTabIndex = -1;
    private bool wasPressed = false;

    // Colori
    private Color barBg = new Color(30, 30, 40, 240);
    private Color barBorder = new Color(80, 80, 100, 255);
    private Color tabActive = new Color(100, 180, 255, 255);
    private Color tabInactive = new Color(60, 60, 80, 255);
    private Color tabHover = new Color(80, 100, 140, 255);
    private Color textActive = new Color(255, 255, 255, 255);
    private Color textInactive = new Color(180, 180, 200, 255);

    private class NavTab
    {
        public string Label;
        public NavigationTab TabType;
        public Action OnClick;
        public Sprite Icon;
    }

    public Obj_GuiBottomNavigation()
    {
        this.guiLayer = true;
        this.depth = -500; // Sempre sopra tutto
        this.persistent = true;

        // Setup tabs
        tabs.Add(new NavTab
        {
            Label = "Compost",
            TabType = NavigationTab.Compost,
            OnClick = () => SwitchToCompost(),
            Icon = null
        });

        tabs.Add(new NavTab
        {
            Label = "Giardino",
            TabType = NavigationTab.MainGame,
            OnClick = () => SwitchToMainGame(),
            Icon = null // Puoi aggiungere icone
        });

        tabs.Add(new NavTab
        {
            Label = "Inventario",
            TabType = NavigationTab.Inventory,
            OnClick = () => SwitchToInventory(),
            Icon = null
        });

    }

    public override void Update()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int barY = screenH - barHeight;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);

        hoveredTabIndex = -1;

        // Controlla hover sui tab
        int totalWidth = tabs.Count * tabWidth + (tabs.Count - 1) * tabSpacing;
        int startX = (screenW - totalWidth) / 2;

        for (int i = 0; i < tabs.Count; i++)
        {
            int tabX = startX + i * (tabWidth + tabSpacing);
            int tabY = barY + 10;

            if (mx >= tabX && mx <= tabX + tabWidth &&
                my >= tabY && my <= tabY + (barHeight - 20))
            {
                hoveredTabIndex = i;
                break;
            }
        }

        // Click detection
        if (hoveredTabIndex != -1 && wasPressed && !isPressed)
        {
            tabs[hoveredTabIndex].OnClick?.Invoke();
            currentTab = tabs[hoveredTabIndex].TabType;
        }

        wasPressed = isPressed && hoveredTabIndex != -1;
    }

    public override void Draw()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int barY = screenH - barHeight;

        // Barra principale
        Graphics.DrawRectangle(0, barY, screenW, barHeight, barBg);

        // Bordo superiore
        Graphics.DrawLine(0, barY, screenW, barY, barBorder);

        // Tabs
        int totalWidth = tabs.Count * tabWidth + (tabs.Count - 1) * tabSpacing;
        int startX = (screenW - totalWidth) / 2;

        for (int i = 0; i < tabs.Count; i++)
        {
            DrawTab(tabs[i], i, startX + i * (tabWidth + tabSpacing), barY + 10);
        }
    }

    private void DrawTab(NavTab tab, int index, int x, int y)
    {
        bool isActive = tab.TabType == currentTab;
        bool isHovered = hoveredTabIndex == index;

        Color bgColor = isActive ? tabActive : (isHovered ? tabHover : tabInactive);
        Color textColor = isActive ? textActive : textInactive;

        int tabHeight = barHeight - 20;

        // Background tab
        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, tabWidth, tabHeight),
            0.2f, 8, bgColor
        );

        // Bordo tab
        if (isActive)
        {
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(x, y, tabWidth, tabHeight),
                0.2f, 8, 3, new Color(150, 200, 255, 255)
            );
        }

        // Icona (se presente)
        if (tab.Icon != null)
        {
            // TODO: Disegna icona
        }

        // Testo
        int textWidth = tab.Label.Length * 6;
        int textX = x + (tabWidth - textWidth) / 2;
        int textY = y + (tabHeight - 12) / 2;

        Graphics.DrawText(tab.Label, textX, textY, 11, textColor);

        // Indicatore attivo (linea sotto)
        if (isActive)
        {
            Graphics.DrawRectangle(x + 10, y + tabHeight - 3, tabWidth - 20, 3,
                new Color(150, 200, 255, 255));
        }

        // Effetto hover
        if (isHovered && !isActive)
        {
            float pulse = (MathF.Sin((float)Time.GetTime() * 5f) + 1f) * 0.5f;
            byte alpha = (byte)(50 + pulse * 30);
            Graphics.DrawRectangleRounded(
                new Rectangle(x, y, tabWidth, tabHeight),
                0.2f, 8, new Color(255, 255, 255, alpha)
            );
        }
    }

    private void SwitchToMainGame()
    {
        Game.room_main.SetActiveRoom();

        // Chiudi pannelli aperti se necessario
        if (Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen)
        {
            Game.inventoryCrates.CloseInventory();
        }

        Console.WriteLine("Switched to Main Game");
    }

    private void SwitchToInventory()
    {
        Game.room_inventory.SetActiveRoom();

        // Chiudi inventario se aperto per forzare vista casse
        if (Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen)
        {
            Game.inventoryCrates.CloseInventory();
        }

        Console.WriteLine("Switched to Inventory");
    }

    private void SwitchToCompost()
    {
        Game.room_compost.SetActiveRoom();
        Console.WriteLine("Switched to Compost");
    }
}
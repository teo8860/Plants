using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public enum NavigationTab
{
    MainGame,
    Inventory,
    Compost,
    Upgrade
}

public class Obj_GuiBottomNavigation : GameElement
{
    private List<NavTab> tabs = new();
    private NavigationTab currentTab = NavigationTab.MainGame;

    public const int BAR_HEIGHT = 40;
    private int barHeight = BAR_HEIGHT;
    private int tabSpacing = 2;
    private int tabVPadding = 4;
    private int tabHPadding = 6;
    private int tabOutline = 2;
    private int fontSize = 10;
    private int hoveredTabIndex = -1;
    private bool wasPressed = false;

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
            Label = "COMPOST",
            TabType = NavigationTab.Compost,
            OnClick = () => SwitchToCompost(),
            Icon = null
        });

        tabs.Add(new NavTab
        {
            Label = "GIARDINO",
            TabType = NavigationTab.MainGame,
            OnClick = () => SwitchToMainGame(),
            Icon = null // Puoi aggiungere icone
        });

        tabs.Add(new NavTab
        {
            Label = "INVENTARIO",
            TabType = NavigationTab.Inventory,
            OnClick = () => SwitchToInventory(),
            Icon = null
        });

        tabs.Add(new NavTab
        {
            Label = "UPGRADE",
            TabType = NavigationTab.Upgrade,
            OnClick = () => SwitchToUpgrade(),
            Icon = null
        });

    }

    public override void Update()
    {
        if (Game.guiMorte != null && Game.guiMorte.active)
            return;

        // Blocca navigazione durante selezione seme e animazione caduta
        if (Game.IsModalitaPiantaggio && Game.guiPiantaggio != null && Game.guiPiantaggio.isFalling)
            return;

        // Blocca navigazione durante il rewind visivo o conferma (countdown: giocatore gioca normalmente)
        if (SeedRecoverySystem.IsRewinding || SeedRecoverySystem.IsConfirming)
            return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int barY = screenH - barHeight;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);

        hoveredTabIndex = -1;

        var (positions, widths) = LayoutTabs(screenW);
        int tabHeight = barHeight - tabVPadding * 2;
        int tabY = barY + tabVPadding;
        for (int i = 0; i < tabs.Count; i++)
        {
            int tabX = positions[i];
            if (mx >= tabX && mx < tabX + widths[i] &&
                my >= tabY && my <= tabY + tabHeight)
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
        if (Game.guiMorte != null && Game.guiMorte.active)
            return;
        if (SeedRecoverySystem.IsConfirming || SeedRecoverySystem.IsRewinding)
            return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int barY = screenH - barHeight;

        // Barra principale
        Graphics.DrawRectangle(0, barY, screenW, barHeight, GuiTheme.NavBarBg);

        // Bordo superiore
        Graphics.DrawRectangle(0, barY, screenW, 2, GuiTheme.NavBarOutline);

        var (positions, widths) = LayoutTabs(screenW);
        for (int i = 0; i < tabs.Count; i++)
        {
            DrawTab(tabs[i], i, positions[i], barY + tabVPadding, widths[i]);
        }
    }

    // Layout equal-width. Tab riempiono intera larghezza bar.
    private (int[] positions, int[] widths) LayoutTabs(int screenW)
    {
        int margin = 2;
        int usable = screenW - margin * 2 - (tabs.Count - 1) * tabSpacing;
        int baseWidth = usable / tabs.Count;
        int remainder = usable - baseWidth * tabs.Count;
        int[] widths = new int[tabs.Count];
        int[] positions = new int[tabs.Count];
        int x = margin;
        for (int i = 0; i < tabs.Count; i++)
        {
            widths[i] = baseWidth + (i < remainder ? 1 : 0); // distribuisce pixel avanzati
            positions[i] = x;
            x += widths[i] + tabSpacing;
        }
        return (positions, widths);
    }

    private void DrawTab(NavTab tab, int index, int x, int y, int tabWidth)
    {
        bool isActive = tab.TabType == currentTab;
        bool isHovered = hoveredTabIndex == index;

        Color bgColor = isActive ? GuiTheme.TabActiveBg
                     : (isHovered ? GuiTheme.TabHoverBg : GuiTheme.TabInactiveBg);
        Color textColor = isActive ? GuiTheme.TabTextActive : GuiTheme.TabTextInactive;

        int tabHeight = barHeight - tabVPadding * 2;

        // Background flat
        Graphics.DrawRectangle(x, y, tabWidth, tabHeight, bgColor);

        // Outline pixel 2px
        Graphics.DrawRectangle(x, y, tabWidth, tabOutline, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x, y + tabHeight - tabOutline, tabWidth, tabOutline, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x, y, tabOutline, tabHeight, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x + tabWidth - tabOutline, y, tabOutline, tabHeight, GuiTheme.TabOutline);

        // Testo centrato + faux bold (double draw offset 1px) per cicciotto senza ingrandire
        int textW = GuiTheme.MeasureText(tab.Label);
        int textX = x + (tabWidth - textW) / 2;
        int textY = y + (tabHeight - fontSize) / 2;
        GuiTheme.DrawText(tab.Label, textX, textY, textColor);
    }

    private void SwitchToMainGame()
    {
        Game.room_main.SetActiveRoom();

        // Chiudi pannelli aperti se necessario
        if (Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen)
        {
            Game.inventoryCrates.CloseInventory();
        }

        // Se siamo in modalita piantaggio, rimostra la selezione semi
        if (Game.IsModalitaPiantaggio)
        {
            Game.guiPiantaggio.Aggiorna();
            Game.guiPiantaggio.Mostra();
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

    private void SwitchToUpgrade()
    {
        Game.room_upgrade.SetActiveRoom();
        Console.WriteLine("Switched to Upgrade");
    }
}
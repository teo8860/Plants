using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiOpzioniPopup : GameElement
{
    private bool isVisible = false;

    private readonly Color overlayColor = new Color(0, 0, 0, 160);
    private readonly Color panelBg = new Color(82, 54, 35, 245);
    private readonly Color panelBorder = new Color(62, 39, 25, 255);
    private readonly Color headerBg = new Color(62, 39, 25, 255);
    private readonly Color textColor = new Color(230, 230, 235, 255);
    private readonly Color subTextColor = new Color(160, 165, 180, 255);
    private readonly Color buttonColor = new Color(101, 67, 43, 255);
    private readonly Color buttonHoverColor = new Color(139, 90, 55, 255);
    private readonly Color buttonActiveColor = new Color(90, 160, 110, 255);
    private readonly Color buttonActiveHoverColor = new Color(120, 190, 140, 255);
    private readonly Color toggleOffColor = new Color(70, 48, 32, 255);
    private readonly Color toggleOnColor = new Color(90, 160, 110, 255);
    private readonly Color toggleKnobColor = new Color(230, 230, 235, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    private const int PanelW = 300;
    private const int PanelH = 320;
    private const int ScaleBtnW = 48;
    private const int ScaleBtnH = 26;
    private const int ScaleBtnGap = 6;

    private int panelX => (sw - PanelW) / 2;
    private int panelY => (sh - PanelH) / 2;

    public Obj_GuiOpzioniPopup()
    {
        this.guiLayer = true;
        this.depth = -2000;
        this.persistent = true;
    }

    public void Show() { isVisible = true; }
    public void Hide() { isVisible = false; }
    public bool IsVisible => isVisible;

    private Rectangle ScaleBtnRect(int index)
    {
        int y = panelY + 78;
        int totalW = ScaleBtnW * GameProperties.MaxUiScaleLevel + ScaleBtnGap * (GameProperties.MaxUiScaleLevel - 1);
        int startX = panelX + (PanelW - totalW) / 2;
        int x = startX + index * (ScaleBtnW + ScaleBtnGap);
        return new Rectangle(x, y, ScaleBtnW, ScaleBtnH);
    }

    public override void Update()
    {
        if (!isVisible) return;
        InputGate.ConsumeMouse();

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();
            bool insidePanel = mx >= panelX && mx <= panelX + PanelW &&
                               my >= panelY && my <= panelY + PanelH;
            if (!insidePanel)
                Hide();
        }
    }

    public override void Draw()
    {
        if (!isVisible) return;

        var cfg = GameConfig.get();

        Hud.Overlay(overlayColor);

        Hud.Panel(panelX, panelY, PanelW, PanelH, new Hud.PanelOptions
        {
            Bg = panelBg, Border = panelBorder, Roundness = 0.08f
        });

        Hud.Panel(panelX, panelY, PanelW, 26, new Hud.PanelOptions
        {
            Bg = headerBg, Roundness = 0.2f, BorderThickness = 0
        });

        Hud.Label(panelX, panelY + 7, PanelW, "OPZIONI", new Hud.LabelOptions
        {
            FontSize = 16, Color = textColor, Align = TextAlign.Center
        });

        Hud.Label(panelX, panelY + 58, PanelW, "Scala finestra", new Hud.LabelOptions
        {
            FontSize = 12, Color = textColor, Align = TextAlign.Center
        });

        for (int i = 0; i < GameProperties.MaxUiScaleLevel; i++)
        {
            int lvl = i + 1;
            bool active = cfg.UiScale == lvl;
            var r = ScaleBtnRect(i);
            int capturedLvl = lvl;

            Hud.Button((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, "x" + lvl,
                () => { if (cfg.UiScale != capturedLvl) cfg.UiScale = capturedLvl; },
                new Hud.ButtonOptions
                {
                    Bg = active ? buttonActiveColor : buttonColor,
                    HoverBg = active ? buttonActiveHoverColor : buttonHoverColor,
                    TextColor = textColor, FontSize = 12, Roundness = 0.3f,
                    BorderThickness = 0
                });
        }

        Hud.Divider(panelX + 12, panelY + 122, PanelW - 24, panelBorder);

        Hud.Label(panelX + 12, panelY + 140, "Avvio nascosto", new Hud.LabelOptions
        {
            FontSize = 12, Color = textColor
        });

        int tW = 44, tH = 22;
        int toggleX = panelX + PanelW - 10 - tW;

        Hud.Toggle(toggleX, panelY + 136, tW, tH, cfg.StartHidden,
            () => { cfg.StartHidden = !cfg.StartHidden; },
            new Hud.ToggleOptions
            {
                OnColor = toggleOnColor, OffColor = toggleOffColor, KnobColor = toggleKnobColor
            });

        string hint = cfg.StartHidden
            ? "Il gioco partira' nella tray."
            : "Il gioco partira' visibile.";
        Hud.Label(panelX + 12, panelY + 164, hint, new Hud.LabelOptions
        {
            FontSize = 9, Color = subTextColor
        });

        Hud.Divider(panelX + 12, panelY + 186, PanelW - 24, panelBorder);

        Hud.Label(panelX + 12, panelY + 204, "Chiudi con X", new Hud.LabelOptions
        {
            FontSize = 12, Color = textColor
        });

        Hud.Toggle(toggleX, panelY + 200, tW, tH, cfg.CloseOnX,
            () => { cfg.CloseOnX = !cfg.CloseOnX; },
            new Hud.ToggleOptions
            {
                OnColor = toggleOnColor, OffColor = toggleOffColor, KnobColor = toggleKnobColor
            });

        string hintX = cfg.CloseOnX
            ? "La X chiudera' il gioco."
            : "La X nascondera' il gioco nella tray.";
        Hud.Label(panelX + 12, panelY + 228, hintX, new Hud.LabelOptions
        {
            FontSize = 9, Color = subTextColor
        });

        int btnW = 90, btnH = 26;
        int btnX = panelX + (PanelW - btnW) / 2;
        int btnY = panelY + PanelH - btnH - 12;

        Hud.Button(btnX, btnY, btnW, btnH, "Chiudi", () => Hide(), new Hud.ButtonOptions
        {
            Bg = buttonColor, HoverBg = buttonHoverColor,
            TextColor = textColor, FontSize = 12, Roundness = 0.3f,
            BorderThickness = 0
        });
    }
}

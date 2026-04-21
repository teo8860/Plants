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
    private bool closeHovered = false;

    private readonly Color overlayColor = new Color(0, 0, 0, 160);
    private readonly Color panelBg = new Color(30, 32, 45, 245);
    private readonly Color panelBorder = new Color(120, 130, 160, 255);
    private readonly Color headerBg = new Color(55, 60, 85, 255);
    private readonly Color textColor = new Color(230, 230, 235, 255);
    private readonly Color subTextColor = new Color(160, 165, 180, 255);
    private readonly Color buttonColor = new Color(80, 90, 120, 255);
    private readonly Color buttonHoverColor = new Color(110, 120, 155, 255);
    private readonly Color buttonActiveColor = new Color(90, 160, 110, 255);
    private readonly Color buttonActiveHoverColor = new Color(120, 190, 140, 255);
    private readonly Color toggleOffColor = new Color(70, 75, 95, 255);
    private readonly Color toggleOnColor = new Color(90, 160, 110, 255);
    private readonly Color toggleKnobColor = new Color(230, 230, 235, 255);
    private readonly Color noteColor = new Color(200, 170, 90, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    // Layout dinamico
    private const int PanelW = 300;
    private const int PanelH = 320;
    private const int ScaleBtnW = 48;
    private const int ScaleBtnH = 26;
    private const int ScaleBtnGap = 6;

    private int panelX => (sw - PanelW) / 2;
    private int panelY => (sh - PanelH) / 2;

    // Hover state per i controlli
    private bool[] hoverScale = new bool[GameProperties.MaxUiScaleLevel];
    private bool hoverStartHidden = false;
    private bool hoverCloseOnX = false;

    public Obj_GuiOpzioniPopup()
    {
        this.guiLayer = true;
        this.depth = -2000;
        this.persistent = true;
    }

    public void Show()
    {
        isVisible = true;
    }

    public void Hide()
    {
        isVisible = false;
    }

    public bool IsVisible => isVisible;

    // Indice 0..MaxUiScaleLevel-1 (livelli 1..MaxUiScaleLevel)
    private Rectangle ScaleBtnRect(int index)
    {
        int y = panelY + 78;
        int totalW = ScaleBtnW * GameProperties.MaxUiScaleLevel + ScaleBtnGap * (GameProperties.MaxUiScaleLevel - 1);
        int startX = panelX + (PanelW - totalW) / 2;
        int x = startX + index * (ScaleBtnW + ScaleBtnGap);
        return new Rectangle(x, y, ScaleBtnW, ScaleBtnH);
    }

    private Rectangle StartHiddenToggleRect()
    {
        int tW = 44;
        int tH = 22;
        int y = panelY + 136;
        int x = panelX + PanelW - 10 - tW;
        return new Rectangle(x, y, tW, tH);
    }

    private Rectangle CloseOnXToggleRect()
    {
        int tW = 44;
        int tH = 22;
        int y = panelY + 200;
        int x = panelX + PanelW - 10 - tW;
        return new Rectangle(x, y, tW, tH);
    }

    private Rectangle CloseButtonRect()
    {
        int btnW = 90;
        int btnH = 26;
        int x = panelX + (PanelW - btnW) / 2;
        int y = panelY + PanelH - btnH - 12;
        return new Rectangle(x, y, btnW, btnH);
    }

    private static bool Inside(Rectangle r, int mx, int my)
    {
        return mx >= r.X && mx <= r.X + r.Width && my >= r.Y && my <= r.Y + r.Height;
    }

    public override void Update()
    {
        if (!isVisible) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        var rHidden = StartHiddenToggleRect();
        var rCloseOnX = CloseOnXToggleRect();
        var rClose = CloseButtonRect();

        for (int i = 0; i < GameProperties.MaxUiScaleLevel; i++)
            hoverScale[i] = Inside(ScaleBtnRect(i), mx, my);

        hoverStartHidden = Inside(rHidden, mx, my);
        hoverCloseOnX = Inside(rCloseOnX, mx, my);
        closeHovered = Inside(rClose, mx, my);

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            var cfg = GameConfig.get();

            for (int i = 0; i < GameProperties.MaxUiScaleLevel; i++)
            {
                if (hoverScale[i])
                {
                    int lvl = i + 1;
                    if (cfg.UiScale != lvl)
                        cfg.UiScale = lvl; // setter applica anche a runtime
                    return;
                }
            }

            if (hoverStartHidden)
            {
                cfg.StartHidden = !cfg.StartHidden;
                return;
            }

            if (hoverCloseOnX)
            {
                cfg.CloseOnX = !cfg.CloseOnX;
                return;
            }

            if (closeHovered)
            {
                Hide();
                return;
            }

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

        Graphics.DrawRectangle(0, 0, sw, sh, overlayColor);

        // Pannello
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, PanelW, PanelH), 0.08f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, PanelW, PanelH), 0.08f, 8, 2, panelBorder);

        // Header
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, PanelW, 26), 0.2f, 8, headerBg);

        string title = "OPZIONI";
        int titleW = title.Length * 8;
        Graphics.DrawText(title, panelX + (PanelW - titleW) / 2, panelY + 7, 16, textColor);

        // --- Scala ---
        string scaleLabel = "Scala finestra";
        int scaleLabelW = scaleLabel.Length * 7;
        Graphics.DrawText(scaleLabel, panelX + (PanelW - scaleLabelW) / 2, panelY + 58, 12, textColor);

        for (int i = 0; i < GameProperties.MaxUiScaleLevel; i++)
        {
            int lvl = i + 1;
            DrawScaleButton(ScaleBtnRect(i), "x" + lvl, cfg.UiScale == lvl, hoverScale[i]);
        }

        // Separatore
        Graphics.DrawRectangle(panelX + 12, panelY + 122, PanelW - 24, 1, panelBorder);

        // --- Avvio nascosto ---
        Graphics.DrawText("Avvio nascosto", panelX + 12, panelY + 140, 12, textColor);
        DrawToggle(StartHiddenToggleRect(), cfg.StartHidden, hoverStartHidden);

        string hint = cfg.StartHidden
            ? "Il gioco partira' nella tray."
            : "Il gioco partira' visibile.";
        Graphics.DrawText(hint, panelX + 12, panelY + 164, 9, subTextColor);

        // Separatore
        Graphics.DrawRectangle(panelX + 12, panelY + 186, PanelW - 24, 1, panelBorder);

        // --- Azione su X finestra ---
        Graphics.DrawText("Chiudi con X", panelX + 12, panelY + 204, 12, textColor);
        DrawToggle(CloseOnXToggleRect(), cfg.CloseOnX, hoverCloseOnX);

        string hintX = cfg.CloseOnX
            ? "La X chiudera' il gioco."
            : "La X nascondera' il gioco nella tray.";
        Graphics.DrawText(hintX, panelX + 12, panelY + 228, 9, subTextColor);

        // --- Bottone chiudi ---
        var rClose = CloseButtonRect();
        Color bc = closeHovered ? buttonHoverColor : buttonColor;
        Graphics.DrawRectangleRounded(rClose, 0.3f, 8, bc);

        string btnText = "Chiudi";
        int btnTextW = btnText.Length * 6;
        Graphics.DrawText(btnText, (int)(rClose.X + (rClose.Width - btnTextW) / 2), (int)(rClose.Y + 7), 12, textColor);
    }

    private void DrawScaleButton(Rectangle r, string label, bool active, bool hover)
    {
        Color bg;
        if (active)
            bg = hover ? buttonActiveHoverColor : buttonActiveColor;
        else
            bg = hover ? buttonHoverColor : buttonColor;

        Graphics.DrawRectangleRounded(r, 0.3f, 8, bg);
        int tW = label.Length * 7;
        Graphics.DrawText(label, (int)(r.X + (r.Width - tW) / 2), (int)(r.Y + 8), 12, textColor);
    }

    private void DrawToggle(Rectangle r, bool on, bool hover)
    {
        Color bg = on ? toggleOnColor : toggleOffColor;
        if (hover)
            bg = new Color((byte)Math.Min(255, bg.R + 25), (byte)Math.Min(255, bg.G + 25), (byte)Math.Min(255, bg.B + 25), bg.A);

        Graphics.DrawRectangleRounded(r, 1f, 8, bg);

        int knobSize = (int)r.Height - 4;
        int knobX = on ? (int)(r.X + r.Width - knobSize - 2) : (int)(r.X + 2);
        int knobY = (int)(r.Y + 2);
        Graphics.DrawRectangleRounded(new Rectangle(knobX, knobY, knobSize, knobSize), 1f, 8, toggleKnobColor);
    }
}

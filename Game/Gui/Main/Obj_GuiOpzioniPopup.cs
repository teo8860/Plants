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

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

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

    public override void Update()
    {
        if (!isVisible) return;

        int panelW = 220;
        int panelH = 140;
        int panelX = (sw - panelW) / 2;
        int panelY = (sh - panelH) / 2;

        int btnW = 70;
        int btnH = 22;
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - btnH - 10;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        closeHovered = mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH;

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (closeHovered)
            {
                Hide();
                return;
            }

            bool insidePanel = mx >= panelX && mx <= panelX + panelW &&
                               my >= panelY && my <= panelY + panelH;
            if (!insidePanel)
                Hide();
        }
    }

    public override void Draw()
    {
        if (!isVisible) return;

        Graphics.DrawRectangle(0, 0, sw, sh, overlayColor);

        int panelW = 220;
        int panelH = 140;
        int panelX = (sw - panelW) / 2;
        int panelY = (sh - panelH) / 2;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH), 0.12f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH), 0.12f, 8, 2, panelBorder);

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, 22), 0.25f, 8, headerBg);

        string title = "OPZIONI";
        int titleW = title.Length * 7;
        Graphics.DrawText(title, panelX + (panelW - titleW) / 2, panelY + 6, 14, textColor);

        string msg = "Presto disponibili";
        int msgW = msg.Length * 5;
        Graphics.DrawText(msg, panelX + (panelW - msgW) / 2, panelY + panelH / 2 - 6, 10, subTextColor);

        int btnW = 70;
        int btnH = 22;
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - btnH - 10;

        Color bc = closeHovered ? buttonHoverColor : buttonColor;
        Graphics.DrawRectangleRounded(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, bc);

        string btnText = "Chiudi";
        int btnTextW = btnText.Length * 6;
        Graphics.DrawText(btnText, btnX + (btnW - btnTextW) / 2, btnY + 5, 12, textColor);
    }
}

using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Obj_GuiToolbar : GameElement
{
    private int baseX;
    private int baseY;
    private int buttonSize;
    private int spacing;

    private bool isDropdownOpen = false;
    private bool isMenuOpen = false;

    private float menuProgress = 0f;
    private float animationSpeed = 8f;

    private List<GuiIconButton> toolButtons = new();

    private Sprite arrowDownIcon;
    private Sprite arrowUpIcon;
    private Sprite menuIcon;

    private Color panelColor = new Color(40, 40, 50, 220);
    private Color panelBorder = new Color(80, 80, 100, 255);

    private int menuWidth = 180;

    private bool arrowWasPressed = false;
    private bool menuWasPressed = false;

    private float menuButtonAlpha = 255f;

    public Obj_GuiToolbar(int x, int y, int buttonSize = 40, int spacing = 5) : base()
    {
        this.baseX = x;
        this.baseY = y;
        this.buttonSize = buttonSize;
        this.spacing = spacing;
        this.guiLayer = true;
    }

    public void SetIcons(Sprite arrowDown, Sprite arrowUp, Sprite menu)
    {
        arrowDownIcon = arrowDown;
        arrowUpIcon = arrowUp;
        menuIcon = menu;
    }

    public void AddButton(Sprite iconInactive, Sprite iconActive, string text, Action<bool> onToggle, bool startActive = false)
    {
        int startY = baseY + buttonSize + spacing;

        var button = new GuiIconButton(
            baseX,
            startY,
            buttonSize,
            iconInactive,
            iconActive,
            text,
            true
        );

        button.TargetY = startY;
        button.OnToggle = onToggle;
        button.IsVisible = false;
        button.CurrentAlpha = 0;

        if (startActive)
            button.SetActive(true);

        toolButtons.Add(button);
    }

    public void AddActionButton(Sprite icon, string text, Action onClick)
    {
        int startY = baseY + buttonSize + spacing;

        var button = new GuiIconButton(
            baseX,
            startY,
            buttonSize,
            icon,
            icon,
            text,
            false
        );

        button.TargetY = startY;
        button.OnClick = onClick;
        button.IsVisible = false;
        button.CurrentAlpha = 0;
        button.BorderInactive = new Color(100, 100, 120, 255);
        button.BorderActive = new Color(100, 100, 120, 255);

        toolButtons.Add(button);
    }

    private void UpdateButtonPositions()
    {
        for (int i = 0; i < toolButtons.Count; i++)
        {
            var btn = toolButtons[i];

            if (isDropdownOpen)
            {
                btn.TargetY = baseY + buttonSize + spacing + i * (buttonSize + spacing);
                btn.IsVisible = true;
                btn.X = baseX;
            }
            else if (isMenuOpen)
            {
                btn.TargetY = baseY + buttonSize + spacing + 10 + i * (buttonSize + spacing);
                btn.IsVisible = true;
                btn.X = baseX + 10;
            }
            else
            {
                btn.TargetY = baseY + buttonSize + spacing;
                btn.IsVisible = false;
            }
        }
    }

    public override void Update()
    {
        float targetMenu = isMenuOpen ? 1f : 0f;
        menuProgress += (targetMenu - menuProgress) * Time.GetFrameTime() * animationSpeed;
        menuProgress = Math.Clamp(menuProgress, 0f, 1f);

        float targetMenuButtonAlpha = (isDropdownOpen || isMenuOpen) ? 255f : 0f;
        menuButtonAlpha += (targetMenuButtonAlpha - menuButtonAlpha) * Time.GetFrameTime() * 10f;
        menuButtonAlpha = Math.Clamp(menuButtonAlpha, 0f, 255f);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);

        bool arrowHovered = mx >= baseX && mx <= baseX + buttonSize &&
                           my >= baseY && my <= baseY + buttonSize;

        if (arrowHovered && arrowWasPressed && !isPressed)
        {
            isDropdownOpen = !isDropdownOpen;
            if (isDropdownOpen) isMenuOpen = false;
            UpdateButtonPositions();
        }
        arrowWasPressed = arrowHovered && isPressed;

        int menuBtnX = baseX + buttonSize + spacing;
        bool menuHovered = menuButtonAlpha > 100 &&
                          mx >= menuBtnX && mx <= menuBtnX + buttonSize &&
                          my >= baseY && my <= baseY + buttonSize;

        if (menuHovered && menuWasPressed && !isPressed)
        {
            if (isMenuOpen)
            {
                isMenuOpen = false;
                isDropdownOpen = true;
            }
            else
            {
                isMenuOpen = true;
                isDropdownOpen = false;
            }
            UpdateButtonPositions();
        }
        menuWasPressed = menuHovered && isPressed;

        foreach (var btn in toolButtons)
        {
            btn.UpdateAnimation();

            int width = isMenuOpen ? menuWidth - 20 : -1;
            btn.CheckClick(width);
        }
    }

    public override void Draw()
    {
        if (isMenuOpen && menuProgress > 0.01f)
        {
            float easedProgress = EaseOutBack(menuProgress);
            int panelWidth = (int)(menuWidth * easedProgress);
            int panelHeight = 20 + toolButtons.Count * (buttonSize + spacing);

            byte panelAlpha = (byte)(220 * menuProgress);
            Color bgColor = new Color(panelColor.R, panelColor.G, panelColor.B, panelAlpha);
            Color borderCol = new Color(panelBorder.R, panelBorder.G, panelBorder.B, panelAlpha);

            Graphics.DrawRectangleRounded(
                new Rectangle(baseX, baseY + buttonSize + spacing, panelWidth, panelHeight),
                0.1f,
                8,
                bgColor
            );

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(baseX, baseY + buttonSize + spacing, panelWidth, panelHeight),
                0.1f,
                8,
                2,
                borderCol
            );
        }

        foreach (var btn in toolButtons)
        {
            if (isMenuOpen)
                btn.DrawExpanded(menuWidth - 20);
            else
                btn.Draw();
        }

        DrawMainButton(baseX, baseY, isDropdownOpen ? arrowUpIcon : arrowDownIcon, isDropdownOpen, 255);

        if (menuButtonAlpha > 5)
            DrawMainButton(baseX + buttonSize + spacing, baseY, menuIcon, isMenuOpen, (byte)menuButtonAlpha);

    }

    private void DrawMainButton(int x, int y, Sprite icon, bool isActive, byte alpha)
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool hovered = mx >= x && mx <= x + buttonSize && my >= y && my <= y + buttonSize;
        bool pressed = hovered && Input.IsMouseButtonDown(MouseButton.Left);

        Color bgColor = pressed ? new Color(60, 60, 75, alpha) :
                       (hovered ? new Color(70, 70, 90, alpha) : new Color(50, 50, 65, alpha));

        Color borderColor = isActive ? new Color(100, 180, 255, alpha) : new Color(100, 100, 120, alpha);

        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, buttonSize, buttonSize),
            0.25f,
            8,
            bgColor
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(x, y, buttonSize, buttonSize),
            0.25f,
            8,
            2,
            borderColor
        );

        if (icon != null && icon.texture.Width > 0 && alpha > 100)
        {
            float iconScale = (buttonSize - 16) / (float)icon.texture.Width;
            Vector2 iconPos = new Vector2(x + buttonSize / 2, y + buttonSize / 2);
            GameFunctions.DrawSprite(icon, iconPos, 0, iconScale);
        }
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}
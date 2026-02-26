using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

public class Obj_GuiToolbarBottom : GameElement
{
    private int baseX;
    private int baseY;
    private int buttonSize;

    private Sprite iconInactive;
    private Sprite iconActive;
    private Action<bool> onToggle;

    private bool wasPressed = false;
    private bool isActive = false;

    public Obj_GuiToolbarBottom(int x, int y, int buttonSize = 40) : base()
    {
        this.baseX = x;
        this.baseY = y;
        this.buttonSize = buttonSize;
        this.guiLayer = true;
    }

    public void SetToggleButton(Sprite iconOff, Sprite iconOn, Action<bool> onToggleCallback)
    {
        this.iconInactive = iconOff;
        this.iconActive = iconOn;
        this.onToggle = onToggleCallback;
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public override void Update()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);

        // Click sul bottone - toggle diretto dell'innafiatoio
        bool hovered = mx >= baseX && mx <= baseX + buttonSize &&
                       my >= baseY && my <= baseY + buttonSize;

        if (hovered && wasPressed && !isPressed)
        {
            isActive = !isActive;
            onToggle?.Invoke(isActive);
        }
        wasPressed = hovered && isPressed;
    }

    public override void Draw()
    {
        DrawMainButton(baseX, baseY, isActive);
    }

    private void DrawMainButton(int x, int y, bool active)
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool hovered = mx >= x && mx <= x + buttonSize && my >= y && my <= y + buttonSize;
        bool pressed = hovered && Input.IsMouseButtonDown(MouseButton.Left);

        Color bgColor = pressed ? new Color(60, 60, 75, 255) :
                       (hovered ? new Color(70, 70, 90, 255) : new Color(50, 50, 65, 255));

        Color borderColor = active ? new Color(100, 180, 255, 255) : new Color(100, 100, 120, 255);

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

        // Usa gli sprite passati
        Sprite icon = active ? iconActive : iconInactive;
        
        // Debug: se icon è null o texture.Width è 0, disegna un quadrato rosso
        if (icon == null || icon.texture.Width == 0)
        {
            // Fallback: disegna un rettangolo colorato come placeholder
            Color placeholderColor = active ? new Color(50, 150, 255, 255) : new Color(100, 150, 200, 255);
            Graphics.DrawRectangle(x + 8, y + 8, buttonSize - 16, buttonSize - 16, placeholderColor);
            return;
        }

        float iconScale = (buttonSize - 16) / (float)icon.texture.Width;
        Vector2 iconPos = new Vector2(x + buttonSize / 2, y + buttonSize / 2);
        GameFunctions.DrawSprite(icon, iconPos, 0, iconScale);
    }
}

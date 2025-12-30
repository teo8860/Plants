using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

public class GuiIconButton
{
    public int X;
    public float Y;
    public int Size;
    
    public Sprite IconInactive;
    public Sprite IconActive;
    public string Text;
    
    public bool IsActive = false;
    public bool IsToggle = true;
    
    public Action OnClick;
    public Action<bool> OnToggle;
    
    public float TargetY;
    public float AnimationSpeed = 12f;
    public float CurrentAlpha = 0f;
    public bool IsVisible = false;
    
    private bool isHovered = false;
    private bool wasPressed = false;
    
    public Color BorderInactive = new Color(180, 60, 60, 255);
    public Color BorderActive = new Color(60, 180, 60, 255);
    public Color FillColor = new Color(50, 50, 60, 240);
    public Color HoverColor = new Color(70, 70, 85, 240);
    public Color PressedColor = new Color(40, 40, 50, 240);

    public GuiIconButton(int x, int y, int size, Sprite iconInactive, Sprite iconActive = null, string text = "", bool isToggle = true)
    {
        X = x;
        Y = y;
        TargetY = y;
        Size = size;
        IconInactive = iconInactive;
        IconActive = iconActive ?? iconInactive;
        Text = text;
        IsToggle = isToggle;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void Toggle()
    {
        IsActive = !IsActive;
        OnToggle?.Invoke(IsActive);
    }

    public bool CheckClick(int expandedWidth = -1)
    {
        if (!IsVisible || CurrentAlpha < 50) return false;
        
        int width = expandedWidth > 0 ? expandedWidth : Size;
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        int drawY = (int)Y;
        
        isHovered = mx >= X && mx <= X + width && my >= drawY && my <= drawY + Size;

        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);
        
        if (isHovered && wasPressed && !isPressed)
        {

            wasPressed = false;
            
            if (IsToggle)
                Toggle();
            
            OnClick?.Invoke();
            return true;
        }
        
        if (isHovered && isPressed)
            wasPressed = true;
        
        if (!isHovered || !isPressed)
            wasPressed = false;
            
        return false;
    }

    public void UpdateAnimation()
    {
        if (IsVisible)
            CurrentAlpha = Math.Min(255, CurrentAlpha + 25f);
        else
            CurrentAlpha = Math.Max(0, CurrentAlpha - 25f);

        float diff = TargetY - Y;
        if (Math.Abs(diff) > 1f)
            Y += diff * Time.GetFrameTime() * AnimationSpeed;
        else
            Y = TargetY;
    }

    public void Draw()
    {
        if (CurrentAlpha < 5) return;

        byte alpha = (byte)CurrentAlpha;
        int drawY = (int)Y;
        
        bool pressed = isHovered && Input.IsMouseButtonDown(MouseButton.Left);
        Color bgColor = pressed ? PressedColor : (isHovered ? HoverColor : FillColor);
        bgColor = new Color(bgColor.R, bgColor.G, bgColor.B, alpha);
        
        Graphics.DrawRectangleRounded(
            new Rectangle(X, drawY, Size, Size),
            0.25f,
            8,
            bgColor
        );

        Color borderColor = IsActive ? BorderActive : BorderInactive;
        borderColor = new Color(borderColor.R, borderColor.G, borderColor.B, alpha);
        
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(X, drawY, Size, Size),
            0.25f,
            8,
            3,
            borderColor
        );

        Sprite icon = IsActive ? IconActive : IconInactive;
        if (icon != null && icon.texture.Width > 0)
        {
            float iconScale = (Size - 10) / (float)icon.texture.Width;
            Vector2 iconPos = new Vector2(X + Size / 2, drawY + Size / 2);
            GameFunctions.DrawSprite(icon, iconPos, 0, iconScale);
        }
    }

    public void DrawExpanded(int expandedWidth)
    {
        if (CurrentAlpha < 5) return;

        byte alpha = (byte)CurrentAlpha;
        int drawY = (int)Y;
        
        bool pressed = isHovered && Input.IsMouseButtonDown(MouseButton.Left);
        Color bgColor = pressed ? PressedColor : (isHovered ? HoverColor : FillColor);
        bgColor = new Color(bgColor.R, bgColor.G, bgColor.B, alpha);
        
        Graphics.DrawRectangleRounded(
            new Rectangle(X, drawY, expandedWidth, Size),
            0.15f,
            8,
            bgColor
        );

        Color borderColor = IsActive ? BorderActive : BorderInactive;
        borderColor = new Color(borderColor.R, borderColor.G, borderColor.B, alpha);
        
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(X, drawY, expandedWidth, Size),
            0.15f,
            8,
            3,
            borderColor
        );

        Sprite icon = IsActive ? IconActive : IconInactive;
        if (icon != null && icon.texture.Width > 0)
        {
            float iconScale = (Size - 14) / (float)icon.texture.Width;
            Vector2 iconPos = new Vector2(X + Size / 2, drawY + Size / 2);
            GameFunctions.DrawSprite(icon, iconPos, 0, iconScale);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            Color textColor = new Color(255, 255, 255, alpha);
            int textX = X + Size + 8;
            int textY = drawY + (Size - 12) / 2;
            Graphics.DrawText(Text, textX, textY, 12, textColor);
        }
    }
}

using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.ComponentModel.Design;
using System.Numerics;
using System.Reflection;


namespace Plants;



public class GuiButton: GameElement
{
    private containerSize container;
    private Color fillColor;
    private Color borderColor;
    private string text;
    private Sprite image;
    private Action  OnClick;
    private bool Mark = false;
    public Color HoverColor  = new Color(255, 220, 100, 255);
    public Color PressedColor = new Color(200, 160, 50, 255);
    private bool isHovered = false;
    private bool isPressed = false;
    private bool isActive = false;

    public GuiButton(int x, int y, int width, int height, string text, Action OnClick, bool mark)
    {
        container = new containerSize
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        };
        fillColor = Color.Gold;
        borderColor = Color.Black;
        this.text = text;
        this.OnClick = OnClick;
        Mark = mark;
    }

 

    public override void Update()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        isHovered = mx >= container.X && mx <= container.X + container.Width && my >= container.Y && my <= container.Y + container.Height;

        if (isHovered)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                isPressed = true;
            }

            if (Input.IsMouseButtonReleased(MouseButton.Left) && isPressed)
            {
                OnClick?.Invoke();
                isPressed = false;
                isActive = !isActive;
            }
        }
        else
        {
            isPressed = false;
        }
    }

    public override void Draw()
    {
        Color Color = isPressed ? PressedColor : (isHovered ? HoverColor : fillColor);
        Graphics.DrawRectangleRounded(
            new Rectangle(container.X, container.Y, container.Width, container.Height),
            0.2f,
            16,
            Color
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(container.X, container.Y, container.Width, container.Height),
            0.2f,
            16,
            2,
            borderColor
        );

        if(image != null)
        {
            
        }

        if (text != null && text != "")
        {           
            Vector2 textSize = TextManager.MeasureTextEx(Font.GetDefault(), text, 14, 0);
            int xx = (int)(container.X + (container.Width /2) - (textSize.X/2));
            int yy = (int)(container.Y + (container.Height /2)- (textSize.Y/2));

            Sprite spriteCheck = AssetLoader.spriteCheck;
            Sprite spriteCross = AssetLoader.spriteCross;
            if (Mark)
            {
                if (isActive)
                    GameFunctions.DrawSprite(spriteCheck, new Vector2(xx - 6, yy + 11), 0f, new Vector2(0.6f, 0.6f));
                else
                    GameFunctions.DrawSprite(spriteCross, new Vector2(xx - 6, yy + 13), 0f, new Vector2(0.6f, 0.6f));
            }
            Graphics.DrawText(text, xx, yy, 12, Color.Black);
        }
     }
}


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


namespace Plants;



public class GuiButton: GameElement
{
    private containerSize container;
    private Color fillColor;
    private Color borderColor;
    private string text;
    private Sprite image;
    private Action  OnClick;

    public GuiButton(int x, int y, int width, int height, string text,Action  OnClick)
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
    }

 

    public override void Update()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if(Input.GetMouseX() > container.X && Input.GetMouseY() > container.Y && Input.GetMouseX() < container.X+container.Width && Input.GetMouseY() < container.Y+container.Height)
            {
                OnClick();
            }
        }
    }

    public override void Draw()
    {

        Graphics.DrawRectangleRounded(
            new Rectangle(container.X, container.Y, container.Width, container.Height),
            0.2f,
            16,
            fillColor
        );

        Graphics.DrawRectangleRoundedLinesEx(
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

            Graphics.DrawText(text, xx, yy, 12, Color.Black);
        }
     }
}


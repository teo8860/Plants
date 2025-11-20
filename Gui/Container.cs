using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;


namespace Plants;

public struct containerSize
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
}

public class GuiBar: GameElement
{
    private containerSize container;
    private float fillLevel;
    private Color waterColor;
    private Color borderColor;

    public GuiBar(int x, int y, int width, int height)
    {
        container = new containerSize
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        };
        fillLevel = 0.0f;
        waterColor = Color.Blue;
        borderColor = Color.Black;
    }

    public void SetValue(float value)
    {
        fillLevel = value;
    }

    public override void Update()
    {
        
    }

    public override void Draw()
    {
        float waterHeight = container.Height * fillLevel;

        float containerBottomY = container.Y + container.Height;
        float waterY = containerBottomY - waterHeight;

        Graphics.DrawRectangle(
            container.X,
            (int)waterY,
            container.Width,
            (int)waterHeight,
            waterColor
        );

        Graphics.DrawRectangleLines(
            container.X,
            container.Y,
            container.Width,
            container.Height,
            borderColor
        );

        string text = $"{fillLevel * 100:F0}%";
        Graphics.DrawText(text, container.X, container.Y + container.Height + 5, 10, Color.Black);
    }
}


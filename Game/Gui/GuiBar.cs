using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.ComponentModel.Design;


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
    private Color glass;
    private bool text;


    public GuiBar(int x, int y, int width, int height, bool Active)
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
        borderColor = Color.DarkGray;
        glass = Color.Beige;
        text = Active;
        this.guiLayer = true;
    }

    public void SetValue(float value)
    {
        fillLevel = value;
    }

    public float GetValue()
    {
        return fillLevel;
    }


    public override void Update()
    {
        
    }

    public override void Draw()
    {
        float waterHeight = container.Height * fillLevel;

        float containerBottomY = container.Y + container.Height;
        float waterY = containerBottomY - waterHeight;

        Graphics.DrawRectangleRounded(
             new Rectangle(container.X, container.Y, container.Width, container.Height),
             0.2f,
             16,
             glass
         );

        Graphics.DrawRectangleRounded(
            new Rectangle(container.X, waterY, container.Width, waterHeight),
            0.2f,
            16,
            waterColor
        );

        Graphics.DrawRectangleRoundedLines(
            new Rectangle(container.X, container.Y, container.Width, container.Height),
            1.0f,
            16,
            2,
            borderColor
        );

        if (!text) return;
        string testo = $"{fillLevel * 100:F0}%";

        
        Graphics.DrawText(testo, container.X-1, container.Y + container.Height + 5, 10, Color.White);
        Graphics.DrawText(testo, container.X+1, container.Y + container.Height + 5, 10, Color.White);
        Graphics.DrawText(testo, container.X, container.Y + container.Height + 5-1, 10, Color.White);
        Graphics.DrawText(testo, container.X, container.Y + container.Height + 5+1, 10, Color.White);

        Graphics.DrawText(testo, container.X, container.Y + container.Height + 5, 10, Color.Black);
    }
}


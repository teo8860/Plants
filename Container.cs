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

public class indicator
{
    private containerSize _container;
    private float _fillLevel;
    private Color _waterColor;
    private Color _borderColor;

    public indicator(int x, int y, int width, int height)
    {
        _container = new containerSize
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        };
        _fillLevel = 0.0f;
        _waterColor = Color.Blue;
        _borderColor = Color.Black;
    }

    public void Update(float newPercentage)
    {
        _fillLevel = RayMath.Clamp(newPercentage, 0.0f, 1.0f);
    }

    public void Draw()
    {
        float waterHeight = _container.Height * _fillLevel;

        float containerBottomY = _container.Y + _container.Height;
        float waterY = containerBottomY - waterHeight;

        Graphics.DrawRectangle(
            _container.X,
            (int)waterY,
            _container.Width,
            (int)waterHeight,
            _waterColor
        );

        Graphics.DrawRectangleLines(
            _container.X,
            _container.Y,
            _container.Width,
            _container.Height,
            _borderColor
        );

        string text = $"{(_fillLevel * 100):F0}%";
        Graphics.DrawText(text, _container.X, _container.Y + _container.Height + 5, 10, Color.Black);
    }
}


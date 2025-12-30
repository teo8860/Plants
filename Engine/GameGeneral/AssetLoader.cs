using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Textures;
using System;
using System.Numerics;

namespace Plants;

public static class AssetLoader
{
    public static Sprite spriteLeaf;
    public static Sprite spriteCross;
    public static Sprite spriteCheck;

    public static Sprite spriteArrowDown;
    public static Sprite spriteArrowUp;
    public static Sprite spriteMenu;

    public static Sprite spriteWaterOff;
    public static Sprite spriteWaterOn;
    public static Sprite spritePhaseOff;
    public static Sprite spritePhaseOn;
    public static Sprite spriteWeatherOff;
    public static Sprite spriteWeatherOn;
    public static Sprite spriteWorldIcon;

    public static void LoadAll()
    {

        spriteLeaf = new Sprite("leaf.png", 0.5f, new(0.5f, 0.0f));
        spriteCross = new Sprite("x.png", 0.05f, new(0.5f, 0.5f));
        spriteCheck = new Sprite("v.png", 0.05f, new(0.5f, 0.5f));
        
        spriteArrowDown = CreateColoredPlaceholder(new Color(200, 200, 220, 255));
        spriteArrowUp = CreateColoredPlaceholder(new Color(220, 200, 200, 255));
        spriteMenu = CreateColoredPlaceholder(new Color(200, 220, 200, 255));

        spriteWaterOff = CreateColoredPlaceholder(new Color(100, 150, 200, 255));
        spriteWaterOn = CreateColoredPlaceholder(new Color(50, 150, 255, 255));
        spritePhaseOff = CreateColoredPlaceholder(new Color(200, 180, 100, 255));
        spritePhaseOn = CreateColoredPlaceholder(new Color(255, 220, 80, 255));
        spriteWeatherOff = CreateColoredPlaceholder(new Color(150, 150, 180, 255));
        spriteWeatherOn = CreateColoredPlaceholder(new Color(200, 220, 255, 255));
        spriteWorldIcon = CreateColoredPlaceholder(new Color(100, 200, 100, 255));
    }
    
    private static Sprite CreateColoredPlaceholder(Color color)
    {
        int size = 24;
        Image img = Image.GenColor(size, size, color);
        var texture = Texture2D.LoadFromImage(img);
        img.Unload();
        return new Sprite(texture, new Vector2(size / 2f, size / 2f));
    }
}

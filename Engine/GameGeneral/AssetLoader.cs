using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Textures;
using System;
using System.Numerics;

namespace Plants;

public static class AssetLoader
{
    public static Sprite spriteLeaf;
    public static Sprite spriteCross;
    public static Sprite spriteCheck;

    public static Sprite spriteLogo;

    public static Sprite spriteArrowDown;
    public static Sprite spriteArrowUp;
    public static Sprite spriteMenu;

    public static Sprite spriteWaterOff;
    public static Sprite spriteWaterOn;
    public static Sprite spriteWateringOff;
    public static Sprite spriteWateringOn;
    public static Sprite spritePhaseOff;
    public static Sprite spritePhaseOn;
    public static Sprite spriteWeatherOff;
    public static Sprite spriteWeatherOn;
    public static Sprite spriteWorldIcon;

    
    public static Sprite spriteSeed1;
    public static Sprite spriteSeed2;
    
    public static Sprite spriteNoise1;
    public static Sprite spriteNoise2;
    public static Sprite spriteNoise3;
    public static Sprite spriteNoise4;
    public static Sprite spriteNoise5;
    public static Sprite spriteNoise6;
    
    public static Sprite spriteShine;

    public static Shader shaderBase;
    public static Shader shaderRecolor;
    public static Shader shaderSeed;

    public static void LoadAll()
    {

        spriteLeaf = new Sprite("leaf.png", 0.5f, new(0.5f, 0.0f));
        spriteCross = new Sprite("x.png", 0.05f, new(0.5f, 0.5f));
        spriteCheck = new Sprite("v.png", 0.05f, new(0.5f, 0.5f));
        
        spriteLogo = new Sprite("logo.png", 1f, new(0.5f, 0.5f));
        
        spriteSeed1 = new Sprite("seme1.png", 0.05f, new(0.5f, 0.5f));
        spriteSeed2 = new Sprite("seme2.png", 0.05f, new(0.5f, 0.5f));

        spriteNoise1 = new Sprite("texture/tex_noise1.png", 0.05f, new(0.5f, 0.5f));
        spriteNoise2 = new Sprite("texture/tex_noise2.png", 0.05f, new(0.5f, 0.5f));
        spriteNoise3 = new Sprite("texture/tex_noise3.png", 0.05f, new(0.5f, 0.5f));
        spriteNoise4 = new Sprite("texture/tex_noise4.png", 0.05f, new(0.5f, 0.5f));
        spriteNoise5 = new Sprite("texture/tex_noise5.png", 0.05f, new(0.5f, 0.5f));
        spriteNoise6 = new Sprite("texture/tex_noise6.png", 0.05f, new(0.5f, 0.5f));

        spriteShine = new Sprite("shine.png", 0.05f, new(0.5f, 0.5f));  

        shaderBase = LoadShader("base");
        shaderRecolor = LoadShader("recolor");
        shaderSeed = LoadShader("seed");
        
        spriteArrowDown = CreateColoredPlaceholder(new Color(200, 200, 220, 255));
        spriteArrowUp = CreateColoredPlaceholder(new Color(220, 200, 200, 255));
        spriteMenu = CreateColoredPlaceholder(new Color(200, 220, 200, 255));

        spriteWaterOff = CreateColoredPlaceholder(new Color(100, 150, 200, 255));
        spriteWaterOn = CreateColoredPlaceholder(new Color(50, 150, 255, 255));
        spriteWateringOff = new Sprite("watering_off.png", 0.5f, new(0.5f, 0.5f));
        spriteWateringOn = new Sprite("watering_on.png", 0.5f, new(0.5f, 0.5f));
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

    private static Shader LoadShader(string name)
    {
        return Shader.LoadFromMemory(Utility.LoadTextFromEmbedded("base.vert", "Assets/shader"), Utility.LoadTextFromEmbedded(name+".frag", "Assets/shader"));
    }
}

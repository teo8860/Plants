using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiWorldTransition : GameElement
{
    private bool isVisible = false;
    private float animationProgress = 0f;
    private float animationSpeed = 5f;
    private bool buttonHovered = false;
    private float buttonPulse = 0f;
    private bool hasTriggered = false;

    public Obj_GuiWorldTransition()
    {
        this.guiLayer = true;
    }


    public override void Update()
    {
        float maxHeight = Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        float currentHeight = Game.pianta.Stats.Altezza;

        bool shouldShow = currentHeight >= maxHeight && !hasTriggered;

        if (shouldShow && !isVisible)
        {
            isVisible = true;
            WorldManager.PrepareNextWorld();
        }
        {
            isVisible = true;
        }

        if (isVisible)
            animationProgress = Math.Min(1f, animationProgress + Time.GetFrameTime() * animationSpeed);
        else
            animationProgress = Math.Max(0f, animationProgress - Time.GetFrameTime() * animationSpeed);

        buttonPulse += Time.GetFrameTime() * 3f;

        if (animationProgress > 0.9f)
            CheckButtonClick();
    }

    private void CheckButtonClick()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        int panelW = 160;
        int panelH = 220;
        int panelX = (screenW - panelW) / 2;
        int panelY = (screenH - panelH) / 2;

        int buttonW = 100;
        int buttonH = 24;
        int buttonX = panelX + (panelW - buttonW) / 2;
        int buttonY = panelY + panelH - 35;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        buttonHovered = mx >= buttonX && mx <= buttonX + buttonW &&
                       my >= buttonY && my <= buttonY + buttonH;

        if (buttonHovered && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            OnTravelClick();
        }
    }

    private void OnTravelClick()
    {
        isVisible = false;
        animationProgress = 0f;
        hasTriggered = true;

        WorldManager.SetNextWorld();

        Game.pianta.Reset();

        UpdatePlantColors();

        hasTriggered = false;
    }

    private void UpdatePlantColors()
    {
        WorldType world = WorldManager.GetCurrentWorld();
        Color color1, color2;

        switch (world)
        {
            case WorldType.Luna:
                color1 = Color.FromHSV(180, 0.2f, 0.6f);
                color2 = Color.FromHSV(180, 0.15f, 0.4f);
                break;
            case WorldType.Marte:
                color1 = Color.FromHSV(15, 0.6f, 0.55f);
                color2 = Color.FromHSV(15, 0.5f, 0.35f);
                break;
            case WorldType.Europa:
                color1 = Color.FromHSV(200, 0.4f, 0.7f);
                color2 = Color.FromHSV(200, 0.3f, 0.5f);
                break;
            case WorldType.Venere:
                color1 = Color.FromHSV(40, 0.7f, 0.6f);
                color2 = Color.FromHSV(35, 0.6f, 0.4f);
                break;
            case WorldType.Titano:
                color1 = Color.FromHSV(30, 0.5f, 0.5f);
                color2 = Color.FromHSV(25, 0.4f, 0.3f);
                break;
            case WorldType.ReameMistico:
                color1 = Color.FromHSV(280, 0.5f, 0.65f);
                color2 = Color.FromHSV(275, 0.4f, 0.45f);
                break;
            case WorldType.GiardinoMistico:
                color1 = Color.FromHSV(140, 0.7f, 0.7f);
                color2 = Color.FromHSV(135, 0.6f, 0.5f);
                break;
            case WorldType.Origine:
                color1 = Color.FromHSV(0, 0.0f, 0.9f);
                color2 = Color.FromHSV(0, 0.0f, 0.7f);
                break;
            default:
                color1 = Color.FromHSV(130, 0.45f, 0.68f);
                color2 = Color.FromHSV(133, 0.47f, 0.44f);
                break;
        }

        Game.pianta.setColori(color1, color2);
    }

    public override void Draw()
    {
        if (animationProgress <= 0.01f) return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        byte overlayAlpha = (byte)(120 * animationProgress);
        Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, overlayAlpha));

        float eased = EaseOutBack(animationProgress);
        int panelW = (int)(160 * eased);
        int panelH = (int)(220 * eased);
        int panelX = (screenW - panelW) / 2;
        int panelY = (screenH - panelH) / 2;

        if (panelW < 40) return;

        Color panelBg = new Color(30, 30, 45, 230);
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.1f, 8, panelBg
        );

        Color borderColor = new Color(100, 100, 150, 255);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.1f, 8, 2, borderColor
        );

        if (animationProgress < 0.5f) return;

        WorldType nextWorld = WorldManager.GetNextWorld(WorldManager.GetCurrentWorld());
        WorldModifier nextMod = WorldManager.GetModifiers(nextWorld);

        string title = GetWorldName(nextWorld);
        Color titleColor = GetWorldColor(nextWorld);
        int titleWidth = title.Length * 7;
        Graphics.DrawText(title, panelX + (panelW - titleWidth) / 2, panelY + 12, 12, titleColor);

        Graphics.DrawText("Nuovo Mondo", panelX + (panelW - 65) / 2, panelY + 28, 8, new Color(150, 150, 180, 255));

        int statsY = panelY + 48;
        int statsX = panelX + 15;
        int lineHeight = 18;

        DrawStat("Luce Solare", nextMod.SolarMultiplier, statsX, statsY, panelW - 30);
        DrawStat("Gravità", nextMod.GravityMultiplier, statsX, statsY + lineHeight, panelW - 30);
        DrawStat("Ossigeno", nextMod.OxygenLevel, statsX, statsY + lineHeight * 2, panelW - 30);
        DrawStat("Temperatura", NormalizeTemp(nextMod.TemperatureModifier), statsX, statsY + lineHeight * 3, panelW - 30);
        DrawStat("Crescita", nextMod.GrowthRateMultiplier, statsX, statsY + lineHeight * 4, panelW - 30);
        DrawStat("Acqua", 1f / nextMod.WaterConsumption, statsX, statsY + lineHeight * 5, panelW - 30);

        int buttonW = 100;
        int buttonH = 24;
        int buttonX = panelX + (panelW - buttonW) / 2;
        int buttonY = panelY + panelH - 35;

        float pulse = (MathF.Sin(buttonPulse) + 1f) * 0.5f;
        Color buttonBg = buttonHovered
            ? new Color(80, 150, 80, 255)
            : new Color((byte)(50 + pulse * 15), (byte)(120 + pulse * 25), (byte)(50 + pulse * 15), 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(buttonX, buttonY, buttonW, buttonH),
            0.3f, 8, buttonBg
        );

        Color buttonBorder = buttonHovered ? Color.White : new Color(100, 200, 100, 255);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(buttonX, buttonY, buttonW, buttonH),
            0.3f, 8, 2, buttonBorder
        );

        Graphics.DrawText("VIAGGIA", buttonX + 28, buttonY + 6, 11, Color.White);
    }

    private void DrawStat(string name, float value, int x, int y, int width)
    {
        Graphics.DrawText(name, x, y, 8, new Color(180, 180, 200, 255));

        bool isGood = value >= 0.5f;
        Color indicatorColor = isGood ? new Color(100, 200, 100, 255) : new Color(255, 150, 50, 255);

        Graphics.DrawText(value.ToString("F1"), x + width - 12, y, 8, indicatorColor);
    }

    private float NormalizeTemp(float temp)
    {
        return Math.Clamp((temp + 200) / 250f, 0f, 1f);
    }

    private string GetWorldName(WorldType world) => world switch
    {
        WorldType.Terra => "Terra",
        WorldType.Luna => "Luna",
        WorldType.Marte => "Marte",
        WorldType.Europa => "Europa",
        WorldType.Venere => "Venere",
        WorldType.Titano => "Titano",
        WorldType.ReameMistico => "Reame Mistico",
        WorldType.GiardinoMistico => "Giardino Mistico",
        WorldType.Origine => "Origine",
        _ => "Sconosciuto"
    };

    private Color GetWorldColor(WorldType world) => world switch
    {
        WorldType.Terra => new Color(100, 200, 100, 255),
        WorldType.Luna => new Color(200, 200, 220, 255),
        WorldType.Marte => new Color(255, 120, 80, 255),
        WorldType.Europa => new Color(150, 200, 255, 255),
        WorldType.Venere => new Color(255, 200, 100, 255),
        WorldType.Titano => new Color(200, 150, 100, 255),
        WorldType.ReameMistico => new Color(180, 100, 255, 255),
        WorldType.GiardinoMistico => new Color(100, 255, 150, 255),
        WorldType.Origine => new Color(255, 255, 255, 255),
        _ => Color.White
    };

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

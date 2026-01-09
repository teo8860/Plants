using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class GuiScrollbar : GameElement
{
    private static readonly (float percent, string name, Color color)[] Milestones = new[]
    {
        (0.00f, "Terra", new Color(139, 90, 43, 255)),
        (0.10f, "Radici", new Color(101, 67, 33, 255)),
        (0.20f, "Germoglio", new Color(50, 150, 50, 255)),
        (0.30f, "Crescita", new Color(60, 180, 60, 255)),
        (0.40f, "Maturità", new Color(80, 200, 80, 255)),
        (0.50f, "Fioritura", new Color(255, 180, 200, 255)),
        (0.60f, "Cielo", new Color(135, 206, 235, 255)),
        (0.70f, "Nuvole", new Color(200, 200, 220, 255)),
        (0.80f, "Stratosfera", new Color(70, 100, 180, 255)),
        (0.90f, "Spazio", new Color(20, 20, 50, 255)),
        (1.00f, "Nuovo Mondo", new Color(255, 215, 0, 255))
    };

    private float displayedProgress = 0f;
    private float animationSpeed = 3f;
    private float pulseTime = 0f;

    public GuiScrollbar()
    {
        this.guiLayer = true;
    }

    public override void Update()
    {
        float maxCamera = Game.controller.offsetMaxY;
        float currentCamera = Rendering.camera.position.Y;
        float targetProgress = maxCamera > 0 ? Math.Clamp(currentCamera / maxCamera, 0f, 1f) : 0f;

        float diff = targetProgress - displayedProgress;
        displayedProgress += diff * Time.GetFrameTime() * animationSpeed;
        pulseTime += Time.GetFrameTime();
    }

    public override void Draw()
    {
        int trackTop = Rendering.camera.screenHeight - 10;
        int trackBottom = 50;
        int trackHeight = trackTop - trackBottom;
        int trackX = 10;
        int trackWidth = 6;

        DrawTrackBackground(trackX, trackBottom, trackWidth, trackHeight);

        DrawMilestones(trackX + trackWidth + 2, trackBottom, trackHeight);

        DrawProgressLine(trackX, trackBottom, trackWidth, trackHeight);

        DrawCurrentMarker(trackX, trackBottom, trackWidth, trackHeight);

        DrawAltitudeInfo(trackX + 22, trackBottom);
    }

    private void DrawTrackBackground(int x, int y, int width, int height)
    {
        Color bgColor = new Color(20, 20, 30, 180);
        Graphics.DrawRectangleRounded(
            new Rectangle(x - 1, y - 1, width + 2, height + 2),
            0.3f, 4, bgColor
        );

        int segments = Milestones.Length - 1;
        int segmentHeight = height / segments;

        for (int i = 0; i < segments; i++)
        {
            Color color = LerpColor(Milestones[i].color, Milestones[i + 1].color, 0.5f);
            color = new Color(color.R, color.G, color.B, (byte)50);
            int segY = y + height - (i + 1) * segmentHeight;
            Graphics.DrawRectangle(x, segY, width, segmentHeight, color);
        }
    }

    private void DrawMilestones(int x, int y, int height)
    {
        foreach (var milestone in Milestones)
        {
            int markerY = y + (int)(height * (1f - milestone.percent));

            Color tickColor = new Color(milestone.color.R, milestone.color.G, milestone.color.B, (byte)150);
            Graphics.DrawLine(x - 4, markerY, x, markerY, tickColor);

            if (milestone.percent == 0f || milestone.percent == 0.5f || milestone.percent == 1f ||
                Math.Abs(displayedProgress - milestone.percent) < 0.08f)
            {
                Color textColor = new Color(200, 200, 200, 180);
                Graphics.DrawText(milestone.name, x + 2, markerY - 4, 8, textColor);
            }
        }
    }

    private void DrawProgressLine(int x, int y, int width, int height)
    {
        int progressHeight = (int)(height * displayedProgress);
        int progressY = y + height - progressHeight;

        Color redGlow = new Color(255, 50, 50, 180);
        Graphics.DrawRectangle(x - 1, progressY, width + 2, progressHeight, redGlow);

        Color redCore = new Color(255, 100, 100, 255);
        Graphics.DrawRectangle(x + 1, progressY, width - 2, progressHeight, redCore);
    }

    private void DrawCurrentMarker(int x, int y, int width, int height)
    {
        int markerY = y + (int)(height * (1f - displayedProgress));

        float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;
        int pulseSize = (int)(2 + pulse * 2);

        Color glowColor = new Color(255, 100, 100, (byte)(80 + pulse * 40));
        Graphics.DrawCircle(x + width / 2, markerY, 5 + pulseSize, glowColor);

        Graphics.DrawCircle(x + width / 2, markerY, 3, new Color(255, 200, 200, 255));
        Graphics.DrawCircle(x + width / 2, markerY, 1, Color.White);
    }

    private void DrawAltitudeInfo(int x, int y)
    {
        float maxHeight = Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        float currentHeight = Rendering.camera.position.Y;
        float percent = displayedProgress * 100f;

        string heightStr = currentHeight >= 1000
            ? $"{currentHeight / 1000f:F1}km"
            : $"{currentHeight:F0}m";

        string currentMilestone = "Terra";
        foreach (var m in Milestones)
        {
            if (displayedProgress >= m.percent)
                currentMilestone = m.name;
        }

        Color panelBg = new Color(20, 20, 30, 180);
        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, 55, 38),
            0.2f, 4, panelBg
        );

        Graphics.DrawText(heightStr, x + 4, y + 3, 9, Color.White);
        Graphics.DrawText($"{percent:F0}%", x + 4, y + 14, 9, new Color(255, 150, 150, 255));
        Graphics.DrawText(currentMilestone, x + 4, y + 25, 7, new Color(180, 180, 200, 255));
    }

    private Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }
}

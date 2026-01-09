using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Numerics;

namespace Plants;

public class Background : GameElement
{
    private static readonly (float altitude, Color color)[] AltitudeGradient = new[]
    {
        (0.00f, new Color(135, 206, 235, 255)),  // Sky blue - ground level
        (0.15f, new Color(100, 180, 220, 255)),  // Light blue
        (0.30f, new Color(70, 140, 200, 255)),   // Medium blue
        (0.45f, new Color(50, 100, 170, 255)),   // Deeper blue
        (0.60f, new Color(30, 60, 140, 255)),    // Dark blue
        (0.75f, new Color(20, 30, 80, 255)),     // Very dark blue
        (0.90f, new Color(10, 15, 40, 255)),     // Almost black
        (1.00f, new Color(5, 5, 20, 255)),       // Space black
    };

    private Vector2[] starPositions;
    private float[] starSizes;
    private float[] starTwinkle;
    private bool starsInitialized = false;

    public Background()
    {
        this.depth = 100;
    }

    private void InitializeStars()
    {
        Random rand = new Random(42);
        int starCount = 80;
        starPositions = new Vector2[starCount];
        starSizes = new float[starCount];
        starTwinkle = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            starPositions[i] = new Vector2(
                rand.Next(0, GameProperties.cameraWidth),
                rand.Next(0, GameProperties.cameraHeight)
            );
            starSizes[i] = (float)(rand.NextDouble() * 1.5 + 0.5);
            starTwinkle[i] = (float)(rand.NextDouble() * 2 + 1);
        }
        starsInitialized = true;
    }

    public override void Update() { }

    public override void Draw()
    {
        if (!starsInitialized) InitializeStars();

        DayPhase phase = Game.Phase;
        Weather weather = WeatherManager.GetCurrentWeather();

        float maxHeight = Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        float currentAltitude = Rendering.camera.position.Y;
        float altitudePercent = Math.Clamp(currentAltitude / maxHeight, 0f, 1f);

        float phaseModifier = GetPhaseModifier(phase);
        float weatherModifier = GetWeatherModifier(weather);

        DrawGradientBackground(altitudePercent, phaseModifier, weatherModifier);

        if (altitudePercent > 0.4f)
        {
            float starVisibility = (altitudePercent - 0.4f) / 0.6f;
            DrawStars(starVisibility * phaseModifier);
        }

        if (altitudePercent > 0.2f && altitudePercent < 0.6f)
        {
            DrawAtmosphereGlow(altitudePercent);
        }
    }

    private void DrawGradientBackground(float altitudePercent, float phaseModifier, float weatherModifier)
    {
        Color topColor = GetColorAtAltitude(altitudePercent + 0.1f);
        Color bottomColor = GetColorAtAltitude(altitudePercent);

        topColor = ApplyModifiers(topColor, phaseModifier, weatherModifier);
        bottomColor = ApplyModifiers(bottomColor, phaseModifier, weatherModifier);

        int segments = 24;
        int segmentHeight = GameProperties.cameraHeight / segments;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            Color segmentColor = LerpColor(bottomColor, topColor, t);
            int y = GameProperties.cameraHeight - (i + 1) * segmentHeight;
            Graphics.DrawRectangle(0, y, GameProperties.cameraWidth, segmentHeight + 1, segmentColor);
        }
    }

    private Color GetColorAtAltitude(float altitude)
    {
        altitude = Math.Clamp(altitude, 0f, 1f);

        for (int i = 0; i < AltitudeGradient.Length - 1; i++)
        {
            if (altitude >= AltitudeGradient[i].altitude && altitude <= AltitudeGradient[i + 1].altitude)
            {
                float localT = (altitude - AltitudeGradient[i].altitude) /
                               (AltitudeGradient[i + 1].altitude - AltitudeGradient[i].altitude);
                return LerpColor(AltitudeGradient[i].color, AltitudeGradient[i + 1].color, localT);
            }
        }
        return AltitudeGradient[^1].color;
    }

    private float GetPhaseModifier(DayPhase phase) => phase switch
    {
        DayPhase.Night => 0.3f,
        DayPhase.Dawn => 0.7f,
        DayPhase.Morning => 1.0f,
        DayPhase.Afternoon => 1.0f,
        DayPhase.Dusk => 0.6f,
        DayPhase.Evening => 0.4f,
        _ => 1.0f
    };

    private float GetWeatherModifier(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.7f,
        Weather.Rainy => 0.5f,
        Weather.Stormy => 0.3f,
        Weather.Foggy => 0.6f,
        Weather.Snowy => 0.8f,
        _ => 1.0f
    };

    private Color ApplyModifiers(Color color, float phaseModifier, float weatherModifier)
    {
        float combined = phaseModifier * weatherModifier;
        return new Color(
            (byte)(color.R * combined),
            (byte)(color.G * combined),
            (byte)(color.B * combined),
            color.A
        );
    }

    private void DrawStars(float visibility)
    {
        float time = (float)Time.GetTime();

        for (int i = 0; i < starPositions.Length; i++)
        {
            float twinkle = (MathF.Sin(time * starTwinkle[i]) + 1f) * 0.5f;
            byte alpha = (byte)(255 * starSizes[i] * twinkle * visibility * 0.8f);
            Color starColor = new Color(255, 255, 255, alpha);
            Graphics.DrawCircleV(starPositions[i], starSizes[i], starColor);
        }
    }

    private void DrawAtmosphereGlow(float altitudePercent)
    {
        float glowIntensity = 1f - Math.Abs(altitudePercent - 0.4f) * 5f;
        glowIntensity = Math.Clamp(glowIntensity, 0f, 0.25f);

        Color glowColor = new Color(100, 150, 255, (byte)(glowIntensity * 80));
        int glowHeight = 25;
        int glowY = GameProperties.cameraHeight - glowHeight;
        Graphics.DrawRectangle(0, glowY, GameProperties.cameraWidth, glowHeight, glowColor);
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

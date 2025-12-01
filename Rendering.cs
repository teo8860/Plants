using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;

namespace Plants;

internal class Rendering
{
    private static WeatherSystem weatherSystem;

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        while (true)
        {
            if (Window.ShouldClose())
            {
                Window.Close();
            }

            var elements = GameElement.GetList();
            foreach (var item in elements)
            {
                item.Update();
            }

            Graphics.BeginDrawing();

            if (Game.cambiaPhase)
            {
                Game.Phase = FaseGiorno.ChangeDayPhase();
                Game.cambiaPhase = false;
            }

            DayPhase currentPhase = Game.Phase;
            Weather currentWeather = MeteoManager.GetCurrentWeather();

            DrawBackground(currentPhase, currentWeather);

            if (Game.controller.offsetY <= 49) { 
                Graphics.DrawRectangle(0, GameProperties.screenHeight - 70, GameProperties.screenWidth, GameProperties.screenHeight, Color.DarkGreen);
            }

            Game.weatherSystem.UpdateAndDrawWeather(currentWeather);

            foreach (var item in elements)
            {
                item.Draw();
            }

            Graphics.EndDrawing();
        }
    }

    private static void DrawBackground(DayPhase phase, Weather weather)
    {
        Color baseColor;

        switch (phase)
        {
            case DayPhase.Night:
                baseColor = new Color(15, 15, 35, 255);
                break;
            case DayPhase.Dawn:
                baseColor = new Color(255, 160, 122, 255);
                break;
            case DayPhase.Morning:
                baseColor = new Color(135, 206, 235, 255);
                break;
            case DayPhase.Afternoon:
                baseColor = new Color(100, 149, 237, 255);
                break;
            case DayPhase.Dusk:
                baseColor = new Color(255, 140, 105, 255);
                break;
            case DayPhase.Evening:
                baseColor = new Color(25, 25, 60, 255);
                break;
            default:
                baseColor = new Color(135, 206, 235, 255);
                break;
        }

        switch (weather)
        {
            case Weather.Cloudy:
                baseColor = DarkenColor(baseColor, 0.8f);
                break;
            case Weather.Rainy:
                baseColor = DarkenColor(baseColor, 0.6f);
                break;
            case Weather.Stormy:
                baseColor = DarkenColor(baseColor, 0.4f);
                break;
            case Weather.Foggy:
                baseColor = DarkenColor(baseColor, 0.7f);
                break;
            case Weather.Snowy:
                baseColor = new Color(200, 210, 220, 255);
                break;
        }

        Graphics.ClearBackground(baseColor);
    }

    private static Color DarkenColor(Color color, float factor)
    {
        return new Color(
            (byte)(color.R * factor),
            (byte)(color.G * factor),
            (byte)(color.B * factor),
            color.A
        );
    }
}
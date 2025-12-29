using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System.Numerics;

namespace Plants;



public class Background: GameElement
{

    public override void Update()
    {
        

    }

    public override void Draw()
    {
        DayPhase phase = Game.Phase;
        Weather weather = WeatherManager.GetCurrentWeather();

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

    private Color DarkenColor(Color color, float factor)
    {
        return new Color(
            (byte)(color.R * factor),
            (byte)(color.G * factor),
            (byte)(color.B * factor),
            color.A
        );
    }
}

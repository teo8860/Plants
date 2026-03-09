using System;

namespace Plants;

public static class WeatherManager
{
    private static Weather currentWeather = Weather.Sunny;
    private static Random random = new Random();
    private static DateTime lastWeatherChange = DateTime.Now;
    private static int weatherDurationMinutes = 30;

    private static Weather? weatherOverride = null;

    public static void SetSimulationOverride(Weather? weather)
    {
        weatherOverride = weather;
    }

    public static Weather GetCurrentWeather()
    {
        if (weatherOverride.HasValue) return weatherOverride.Value;
        UpdateWeatherIfNeeded();
        return currentWeather;
    }

    public static void SetWeather(Weather weather)
    {
        currentWeather = weather;
        lastWeatherChange = DateTime.Now;
        GameSave.get().Save();
    }

    public static void SetWeatherDuration(int minutes)
    {
        weatherDurationMinutes = minutes;
    }

    private static void UpdateWeatherIfNeeded()
    {
        TimeSpan timeSinceChange = DateTime.Now - lastWeatherChange;

        if (timeSinceChange.TotalMinutes >= weatherDurationMinutes)
        {
            ChangeWeatherRandomly();
        }
    }

    public static Weather GetNextWeather(Weather current, Random rng)
    {
        int roll = rng.Next(100);
        return current switch
        {
            Weather.Sunny => roll < 60 ? Weather.Sunny : roll < 80 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Snowy : Weather.Foggy,
            Weather.Cloudy => roll < 40 ? Weather.Sunny : roll < 60 ? Weather.Cloudy : roll < 85 ? Weather.Rainy : roll < 95 ? Weather.Stormy : Weather.Foggy,
            Weather.Rainy => roll < 50 ? Weather.Rainy : roll < 75 ? Weather.Cloudy : roll < 90 ? Weather.Stormy : Weather.Snowy,
            Weather.Stormy => roll < 40 ? Weather.Stormy : roll < 80 ? Weather.Rainy : roll < 90 ? Weather.Snowy : Weather.Cloudy,
            Weather.Foggy => roll < 40 ? Weather.Foggy : roll < 70 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Stormy : Weather.Snowy,
            Weather.Snowy => roll < 50 ? Weather.Snowy : roll < 80 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Foggy : Weather.Sunny,
            _ => Weather.Sunny
        };
    }

    private static void ChangeWeatherRandomly()
    {
        Weather newWeather = GetNextWeather(currentWeather, random);
        if (newWeather != currentWeather)
        {
            currentWeather = newWeather;
            lastWeatherChange = DateTime.Now;
        }
    }

    public static void SetCurrentWeather(Weather weather)
    {
        currentWeather = weather;
    }

    public static void ForceWeatherChange()
    {
        ChangeWeatherRandomly();
    }

    public static DateTime GetLastWeatherChange() => lastWeatherChange;
    public static void SetLastWeatherChange(DateTime time) { lastWeatherChange = time; }
    public static void SetWeatherDirect(Weather weather) { currentWeather = weather; }
}
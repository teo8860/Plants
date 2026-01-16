using System;

namespace Plants;

public static class WeatherManager
{
    private static Weather currentWeather = Weather.Sunny;
    private static Random random = new Random();
    private static DateTime lastWeatherChange = DateTime.Now;
    private static int weatherDurationMinutes = 30;

    public static Weather GetCurrentWeather()
    {
        UpdateWeatherIfNeeded();
        return currentWeather;
    }

    public static void SetWeather(Weather weather)
    {
        currentWeather = weather;
        lastWeatherChange = DateTime.Now;
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

    private static void ChangeWeatherRandomly()
    {
        Weather newWeather = currentWeather;

        switch (currentWeather)
        {
            case Weather.Sunny:
                int sunnyRoll = random.Next(100);
                if (sunnyRoll < 60) newWeather = Weather.Sunny;      
                else if (sunnyRoll < 80) newWeather = Weather.Cloudy;
                else if (sunnyRoll < 90) newWeather = Weather.Rainy;
                else if (sunnyRoll < 95) newWeather = Weather.Snowy;
                else newWeather = Weather.Foggy;                      
                break;

            case Weather.Cloudy:
                int cloudyRoll = random.Next(100);
                if (cloudyRoll < 40) newWeather = Weather.Sunny;    
                else if (cloudyRoll < 60) newWeather = Weather.Cloudy;
                else if (cloudyRoll < 85) newWeather = Weather.Rainy; 
                else if (cloudyRoll < 95) newWeather = Weather.Stormy; 
                else newWeather = Weather.Foggy;                    
                break;

            case Weather.Rainy:
                int rainyRoll = random.Next(100);
                if (rainyRoll < 50) newWeather = Weather.Rainy;     
                else if (rainyRoll < 75) newWeather = Weather.Cloudy; 
                else if (rainyRoll < 90) newWeather = Weather.Stormy; 
                else if (rainyRoll < 100) newWeather = Weather.Snowy;
                else newWeather = Weather.Sunny;                      
                break;

            case Weather.Stormy:
                int stormyRoll = random.Next(100);
                if (stormyRoll < 40) newWeather = Weather.Stormy;    
                else if (stormyRoll < 80) newWeather = Weather.Rainy; 
                else if (stormyRoll < 90) newWeather = Weather.Snowy; 
                else newWeather = Weather.Cloudy;                     
                break;

            case Weather.Foggy:
                int foggyRoll = random.Next(100);
                if (foggyRoll < 40) newWeather = Weather.Foggy;      
                else if (foggyRoll < 70) newWeather = Weather.Cloudy; 
                else if (foggyRoll < 90) newWeather = Weather.Rainy;  
                else if (foggyRoll < 95) newWeather = Weather.Stormy;
                else if (foggyRoll < 100) newWeather = Weather.Snowy;
                else newWeather = Weather.Sunny;                     
                break;

            case Weather.Snowy:
                int snowyRoll = random.Next(100);
                if (snowyRoll < 50) newWeather = Weather.Snowy;     
                else if (snowyRoll < 80) newWeather = Weather.Cloudy;
                else if (snowyRoll < 90) newWeather = Weather.Rainy;  
                else if (snowyRoll < 95) newWeather = Weather.Foggy;
                else newWeather = Weather.Sunny;                      
                break;
        }

        if (newWeather != currentWeather)
        {
            currentWeather = newWeather;
            lastWeatherChange = DateTime.Now;
        }
    }

    public static void ForceWeatherChange()
    {
        ChangeWeatherRandomly();
    }
}
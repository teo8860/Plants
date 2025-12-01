using System;

namespace Plants
{
    public enum DayPhase
    {
        Night,      // 0:00 - 5:59
        Dawn,       // 6:00 - 7:59
        Morning,    // 8:00 - 11:59
        Afternoon,  // 12:00 - 17:59
        Dusk,       // 18:00 - 19:59
        Evening     // 20:00 - 23:59
    }

    public enum Weather
    {
        Sunny,      // Soleggiato
        Cloudy,     // Nuvoloso
        Rainy,      // Pioggia
        Stormy,     // Tempesta
        Foggy,      // Nebbioso
        Snowy       // Neve
    }

    public static class FaseGiorno
    {
        public static DayPhase GetCurrentPhase()
        {
            return GetPhaseFromTime(DateTime.Now);
        }

        public static DayPhase GetPhaseFromTime(DateTime time)
        {
            int hour = time.Hour;
            if (hour >= 0 && hour < 6)
                return DayPhase.Night;
            else if (hour >= 6 && hour < 8)
                return DayPhase.Dawn;
            else if (hour >= 8 && hour < 12)
                return DayPhase.Morning;
            else if (hour >= 12 && hour < 18)
                return DayPhase.Afternoon;
            else if (hour >= 18 && hour < 20)
                return DayPhase.Dusk;
            else
                return DayPhase.Evening;
        }

        public static DayPhase GetPhaseFromHour(int hour)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentException("L'ora deve essere tra 0 e 23");
            if (hour >= 0 && hour < 6)
                return DayPhase.Night;
            else if (hour >= 6 && hour < 8)
                return DayPhase.Dawn;
            else if (hour >= 8 && hour < 12)
                return DayPhase.Morning;
            else if (hour >= 12 && hour < 18)
                return DayPhase.Afternoon;
            else if (hour >= 18 && hour < 20)
                return DayPhase.Dusk;
            else
                return DayPhase.Evening;
        }

        public static DayPhase ChangeDayPhase()
        {
            DayPhase newPhase = Game.Phase;

            switch (newPhase)
            {
                case DayPhase.Night:
                    return DayPhase.Dawn;
                    
                case DayPhase.Dawn:
                    return DayPhase.Morning;
                    
                case DayPhase.Morning:
                    return DayPhase.Afternoon;
                    
                case DayPhase.Afternoon:
                    return DayPhase.Dusk;
                    
                case DayPhase.Dusk:
                    return DayPhase.Evening;

                case DayPhase.Evening:
                    return DayPhase.Night;
            }        
            return DayPhase.Evening;
        }

    }

    public static class MeteoManager
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
                    else if (sunnyRoll < 90) newWeather = Weather.Cloudy;
                    else if (sunnyRoll < 95) newWeather = Weather.Rainy;
                    else if (sunnyRoll < 100) newWeather = Weather.Snowy;
                    else newWeather = Weather.Foggy;                      
                    break;

                case Weather.Cloudy:
                    int cloudyRoll = random.Next(100);
                    if (cloudyRoll < 40) newWeather = Weather.Sunny;    
                    else if (cloudyRoll < 60) newWeather = Weather.Cloudy;
                    else if (cloudyRoll < 85) newWeather = Weather.Rainy; 
                    else if (cloudyRoll < 95) newWeather = Weather.Stormy; 
                    else if (cloudyRoll < 100) newWeather = Weather.Snowy;
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
                    else if (snowyRoll < 100) newWeather = Weather.Sunny;
                    else newWeather = Weather.Foggy;                      
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

        public static string GetWeatherDescription()
        {
            switch (currentWeather)
            {
                case Weather.Sunny:
                    return "Soleggiato";
                case Weather.Cloudy:
                    return "Nuvoloso";
                case Weather.Rainy:
                    return "Pioggia";
                case Weather.Stormy:
                    return "Tempesta";
                case Weather.Foggy:
                    return "Nebbioso";
                case Weather.Snowy:
                    return "Neve";
                default:
                    return "Sconosciuto";
            }
        }
    }
}
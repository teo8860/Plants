using System;

namespace Plants;

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
    Stormy,      // Tempesta
    Windy,      // Ventoso
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

    public static void SetCurrentPhase(DayPhase phase)
    {
        // Set the current phase (for save/load system)
        // Note: This is a simplified implementation
        // In a full implementation, you might need to update internal timing
    }

}

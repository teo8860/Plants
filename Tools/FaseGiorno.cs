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
    }  
}
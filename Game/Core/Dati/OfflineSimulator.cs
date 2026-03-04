using System;

namespace Plants;

public static class OfflineSimulator
{
    private const int TICK_MINUTES = 10;
    private const int SUB_TICKS_PER_TICK = TICK_MINUTES * 60; // 600 (1 per second)
    private const int WEATHER_DURATION_MINUTES = 30;

    public static OfflineSimulationResult Simulate(
        DateTime closeTime,
        DateTime openTime,
        Weather startWeather,
        DateTime lastWeatherChange)
    {
        var result = new OfflineSimulationResult();
        TimeSpan offlineTime = openTime - closeTime;

        if (offlineTime.TotalMinutes < 1 || Game.pianta == null)
        {
            result.TimeSimulated = offlineTime;
            return result;
        }

        var plant = Game.pianta;
        var logic = plant.proprieta;

        // Snapshot before
        result.TimeSimulated = offlineTime;
        result.HealthBefore = plant.Stats.Salute;
        result.HydrationBefore = plant.Stats.Idratazione;
        result.HeightBefore = plant.Stats.Altezza;
        result.LeavesBefore = plant.Stats.FoglieAttuali;
        result.WaterBefore = WaterSystem.Current;

        int totalTicks = (int)(offlineTime.TotalMinutes / TICK_MINUTES);
        if (totalTicks < 1) totalTicks = 1;

        // Weather simulation state
        Weather simWeather = startWeather;
        DateTime nextWeatherChange = lastWeatherChange.AddMinutes(WEATHER_DURATION_MINUTES);
        Random weatherRng = new Random(closeTime.GetHashCode());
        int weatherChanges = 0;

        // Enter offline simulation mode
        Game.IsOfflineSimulation = true;

        try
        {
            bool plantDied = false;

            for (int tick = 0; tick < totalTicks && !plantDied; tick++)
            {
                DateTime tickTime = closeTime.AddMinutes((tick + 1) * TICK_MINUTES);

                // === Advance weather ===
                while (tickTime >= nextWeatherChange)
                {
                    Weather newWeather = WeatherManager.GetNextWeather(simWeather, weatherRng);
                    if (newWeather != simWeather)
                    {
                        simWeather = newWeather;
                        weatherChanges++;
                    }
                    nextWeatherChange = nextWeatherChange.AddMinutes(WEATHER_DURATION_MINUTES);
                }

                // === Resolve environment ===
                DayPhase phase = FaseGiorno.GetPhaseFromTime(tickTime);
                WorldModifier worldMod = WorldManager.GetCurrentModifiers();

                // Set overrides so ALL internal code sees simulated values
                FaseGiorno.SetSimulationOverride(phase);
                WeatherManager.SetSimulationOverride(simWeather);
                WorldManager.SetSimulationOverride(worldMod);

                // === Run sub-ticks (plant simulation at 1/sec rate) ===
                for (int sub = 0; sub < SUB_TICKS_PER_TICK; sub++)
                {
                    logic.AggiornaTutto(phase, simWeather, worldMod);

                    // Check death
                    if (plant.Stats.Salute <= 0)
                    {
                        plantDied = true;
                        break;
                    }
                }

                result.TicksSimulated = tick + 1;
            }

            result.PlantDied = plantDied;
        }
        finally
        {
            // ALWAYS clear simulation mode
            Game.IsOfflineSimulation = false;
            FaseGiorno.SetSimulationOverride(null);
            WeatherManager.SetSimulationOverride(null);
            WorldManager.SetSimulationOverride(null);

            // Update live weather state to where simulation ended
            WeatherManager.SetWeatherDirect(simWeather);
            WeatherManager.SetLastWeatherChange(
                nextWeatherChange.AddMinutes(-WEATHER_DURATION_MINUTES));
        }

        // Snapshot after
        result.HealthAfter = plant.Stats.Salute;
        result.HydrationAfter = plant.Stats.Idratazione;
        result.HeightAfter = plant.Stats.Altezza;
        result.LeavesAfter = plant.Stats.FoglieAttuali;

        // Water: passive recharge for entire offline period
        WaterSystem.AddOfflineRecharge(offlineTime.TotalSeconds);
        result.WaterAfter = WaterSystem.Current;

        result.WeatherChanges = weatherChanges;

        return result;
    }
}

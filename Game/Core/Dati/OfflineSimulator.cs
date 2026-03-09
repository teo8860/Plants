using System;

namespace Plants;

public static class OfflineSimulator
{
    private const int TICK_MINUTES = 10;
    private const int SUB_TICKS_PER_TICK = TICK_MINUTES * 60; // 600 (1 per second)
    private const int WEATHER_DURATION_MINUTES = 30;

    // Auto-watering: when hydration drops below this, use watering can
    private const float AUTO_WATER_THRESHOLD = 0.5f;
    // Auto-watering: refill hydration to this level
    private const float AUTO_WATER_TARGET = 0.95f;
    // In-game: 0.01 hydration/frame @ 60fps = 0.6 hydration/sec, costs 10 water/sec
    // So 1.0 hydration = 16.67 water units
    private const float WATER_PER_HYDRATION = 16.67f;
    // Watering can passive recharge: 10 per 5 minutes
    private const float WATER_RECHARGE_RATE = 10f / (5f * 60f);

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

        // Local water state (don't modify WaterSystem until the end)
        float waterCurrent = WaterSystem.Current;
        float waterMax = WaterSystem.Max;

        // Health must not change during offline simulation
        float originalHealth = plant.Stats.Salute;

        Game.IsOfflineSimulation = true;

        try
        {
            for (int tick = 0; tick < totalTicks; tick++)
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

                FaseGiorno.SetSimulationOverride(phase);
                WeatherManager.SetSimulationOverride(simWeather);
                WorldManager.SetSimulationOverride(worldMod);

                // === Run sub-ticks ===
                for (int sub = 0; sub < SUB_TICKS_PER_TICK; sub++)
                {
                    // 1. Water consumption
                    float consumo = logic.CalcolaConsumoAcqua(worldMod);
                    plant.Stats.Idratazione = Math.Max(0, plant.Stats.Idratazione - consumo);

                    // 2. Rain hydration
                    if (worldMod.IsMeteoOn && (simWeather == Weather.Rainy || simWeather == Weather.Stormy))
                    {
                        float rain = 0.003f * Math.Max(0, worldMod.HydrationFromRain);
                        plant.Stats.Idratazione = Math.Min(1.0f, plant.Stats.Idratazione + rain);
                    }

                    // 3. Watering can passive recharge
                    waterCurrent = Math.Min(waterMax, waterCurrent + WATER_RECHARGE_RATE);

                    // 4. Auto-water from can when hydration gets low
                    if (plant.Stats.Idratazione < AUTO_WATER_THRESHOLD && waterCurrent > 0)
                    {
                        float deficit = AUTO_WATER_TARGET - plant.Stats.Idratazione;
                        float waterNeeded = deficit * WATER_PER_HYDRATION;
                        float waterUsed = Math.Min(waterNeeded, waterCurrent);
                        float hydrationGained = waterUsed / WATER_PER_HYDRATION;
                        plant.Stats.Idratazione += hydrationGained;
                        waterCurrent -= waterUsed;
                    }

                    // 5. Growth (ControlloCrescita checks hydration internally)
                    plant.ControlloCrescita();

                    // 6. Ensure health never changes
                    plant.Stats.Salute = originalHealth;
                }

                result.TicksSimulated = tick + 1;
            }
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

            // Apply final water state
            WaterSystem.Current = waterCurrent;
        }

        // Snapshot after
        result.HealthAfter = plant.Stats.Salute;
        result.HydrationAfter = plant.Stats.Idratazione;
        result.HeightAfter = plant.Stats.Altezza;
        result.LeavesAfter = plant.Stats.FoglieAttuali;
        result.WaterAfter = WaterSystem.Current;
        result.WeatherChanges = weatherChanges;

        return result;
    }
}

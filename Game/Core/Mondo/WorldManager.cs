using Raylib_CSharp.Colors;
using System;
using System.Collections.Generic;

namespace Plants;

public static class WorldManager
{
    private static WorldType currentWorld = WorldType.Terra;
    private static int currentStage = 1;
    private static WorldType nextWorld = WorldType.Terra;

    private static WorldModifier? simulationOverride = null;

    public static void SetSimulationOverride(WorldModifier? mod)
    {
        simulationOverride = mod;
    }

    public static void SetCurrentWorld(WorldType world)
    {
        currentWorld = world;
        UpdateGroundColors();
        GameSave.get().Save();
    }

    private static void UpdateGroundColors()
    {
        if (WorldDefinitions.WorldColors.TryGetValue(currentWorld, out var colors))
        {
            if(Game.ground != null)
            Game.ground.SetGroundWorld(colors.ground1, colors.ground2, colors.nextGround1, colors.nextGround2);
        }
    }

    public static WorldType GetCurrentWorld() => currentWorld;
    public static int GetCurrentStage() => currentStage;
    public static void SetCurrentStage(int stage) { currentStage = Math.Max(1, stage); }
    public static float GetDifficultyMultiplier(int stage) => 1.0f + (stage / 10) * 0.25f;
    public static WorldType GetRandomWorld() { var w = new[] { WorldType.Terra, WorldType.Luna, WorldType.Marte, WorldType.Europa, WorldType.Venere, WorldType.Titano, WorldType.ReameMistico, WorldType.GiardinoMistico, WorldType.Origine }; return w[RandomHelper.Int(0, w.Length)]; }
    public static WorldType GetNextWorld() => nextWorld;
    public static void PrepareNextWorld() { nextWorld = GetRandomWorld(); }

    public static WorldModifier GetCurrentModifiers()
    {
        if (simulationOverride.HasValue) return simulationOverride.Value;
        var m = WorldDefinitions.GetModifiers(currentWorld);
        float d = GetDifficultyMultiplier(currentStage);
        return new WorldModifier { SolarMultiplier = m.SolarMultiplier / d, GravityMultiplier = m.GravityMultiplier, OxygenLevel = m.OxygenLevel / d, TemperatureModifier = m.TemperatureModifier, IsMeteoOn = m.IsMeteoOn, LimitMultiplier = m.LimitMultiplier / d, GrowthRateMultiplier = m.GrowthRateMultiplier / d, WaterConsumption = m.WaterConsumption * d, OxygenConsumption = m.OxygenConsumption * d, EnergyDrain = m.EnergyDrain * d, ParasiteChance = m.ParasiteChance * d, ParasiteDamage = m.ParasiteDamage * d, StormChance = m.StormChance * d, StormDamage = m.StormDamage * d, TemperatureDamage = m.TemperatureDamage * d, LeafDropRate = m.LeafDropRate * d, HealthRegen = m.HealthRegen / d, HydrationFromRain = m.HydrationFromRain, RequiresOxygenTank = m.RequiresOxygenTank, Difficulty = m.Difficulty }; }

    public static WorldModifier GetModifiers(WorldType world) =>
        WorldDefinitions.GetModifiers(world);

    // Missing methods needed for save/load system
    private static WorldDifficulty currentDifficulty = WorldDifficulty.Normal;
    private static Dictionary<WorldType, WorldDifficulty> difficultyOverrides = new();

    public static WorldDifficulty GetCurrentWorldDifficulty() => currentDifficulty;

    public static void SetWorldDifficulty(WorldType world, WorldDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        difficultyOverrides[world] = difficulty;
        GameSave.get().Save();
    }

    public static void SetNextWorld() {
        currentStage++;
        SetCurrentWorld(nextWorld);
        LeafHarvestSystem.HarvestAndShow($"Stage {currentStage - 1} Completato!");
    }

    public static void SetPreviousWorld() {
        SetCurrentWorld(WorldDefinitions.GetPreviousWorld(currentWorld));
        LeafHarvestSystem.HarvestAndShow("Cambio Mondo");
    }
}

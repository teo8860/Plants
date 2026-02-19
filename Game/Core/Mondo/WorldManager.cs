using Raylib_CSharp.Colors;
using System;
using System.Collections.Generic;

namespace Plants;

public enum WorldType
{
    Terra,
    Luna,
    Marte,
    Europa,
    Venere,
    Titano,
    ReameMistico,
    GiardinoMistico,
    Origine,
    Serra
}

public enum WorldDifficulty
{
    Tutorial,
    Easy,
    Normal,
    Medium,
    MediumHard,
    Hard,
    VeryHard,
    Extreme,
    Nightmare,
    Impossible
}

public struct WorldModifier
{
    public float SolarMultiplier;
    public float GravityMultiplier;
    public float OxygenLevel;
    public float TemperatureModifier;
    public bool IsMeteoOn;
    public float LimitMultiplier;
    public float GrowthRateMultiplier;
    public float WaterConsumption;
    public float OxygenConsumption;
    public float EnergyDrain;
    public float ParasiteChance;
    public float ParasiteDamage;
    public float StormChance;
    public float StormDamage;
    public float TemperatureDamage;
    public float LeafDropRate;
    public float HealthRegen;
    public float HydrationFromRain;
    public bool RequiresOxygenTank;
    public WorldDifficulty Difficulty;

    public WorldModifier()
    {
        SolarMultiplier = 1.0f;
        GravityMultiplier = 1.0f;
        OxygenLevel = 1.0f;
        TemperatureModifier = 0.0f;
        IsMeteoOn = true;
        LimitMultiplier = 1.0f;
        GrowthRateMultiplier = 1.0f;
        WaterConsumption = 1.0f;
        OxygenConsumption = 1.0f;
        EnergyDrain = 1.0f;
        ParasiteChance = 1.0f;
        ParasiteDamage = 1.0f;
        StormChance = 1.0f;
        StormDamage = 1.0f;
        TemperatureDamage = 1.0f;
        LeafDropRate = 1.0f;
        HealthRegen = 1.0f;
        HydrationFromRain = 1.0f;
        RequiresOxygenTank = false;
        Difficulty = WorldDifficulty.Easy;
    }
}

public static class WorldManager
{
    private static WorldType currentWorld = WorldType.Terra;

    private static readonly Dictionary<WorldType, WorldModifier> modifiers = new()
    {
        {
            WorldType.Serra,
            new WorldModifier
            {
                SolarMultiplier = 1.3f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 2.0f,
                IsMeteoOn = false,
                LimitMultiplier = 0.15f,
                GrowthRateMultiplier = 2.0f,
                WaterConsumption = 0.3f,
                OxygenConsumption = 0.0f,
                EnergyDrain = 0.5f,
                ParasiteChance = 0.0f,
                ParasiteDamage = 0.0f,
                StormChance = 0.0f,
                StormDamage = 0.0f,
                TemperatureDamage = 0.0f,
                LeafDropRate = 0.2f,
                HealthRegen = 3.0f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Tutorial
            }
        },
        {
            WorldType.Terra,
            new WorldModifier
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 0.0f,
                IsMeteoOn = true,
                LimitMultiplier = 1.0f,
                GrowthRateMultiplier = 1.0f,
                WaterConsumption = 1.0f,
                OxygenConsumption = 1.0f,
                EnergyDrain = 1.0f,
                ParasiteChance = 1.0f,
                ParasiteDamage = 1.0f,
                StormChance = 1.0f,
                StormDamage = 1.0f,
                TemperatureDamage = 1.0f,
                LeafDropRate = 1.0f,
                HealthRegen = 1.0f,
                HydrationFromRain = 1.0f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Easy
            }
        },
        {
            WorldType.Luna,
            new WorldModifier
            {
                SolarMultiplier = 0.6f,
                GravityMultiplier = 0.16f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -20.0f,
                IsMeteoOn = false,
                LimitMultiplier = 2.0f,
                GrowthRateMultiplier = 0.7f,
                WaterConsumption = 0.8f,
                OxygenConsumption = 1.5f,
                EnergyDrain = 1.2f,
                ParasiteChance = 0.2f,
                ParasiteDamage = 1.0f,
                StormChance = 0.0f,
                StormDamage = 0.0f,
                TemperatureDamage = 1.2f,
                LeafDropRate = 1.3f,
                HealthRegen = 0.8f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Normal
            }
        },
        {
            WorldType.Marte,
            new WorldModifier
            {
                SolarMultiplier = 0.45f,
                GravityMultiplier = 0.38f,
                OxygenLevel = 0.02f,
                TemperatureModifier = -35.0f,
                IsMeteoOn = true,
                LimitMultiplier = 3.0f,
                GrowthRateMultiplier = 0.5f,
                WaterConsumption = 1.2f,
                OxygenConsumption = 1.8f,
                EnergyDrain = 1.4f,
                ParasiteChance = 0.3f,
                ParasiteDamage = 1.2f,
                StormChance = 2.0f,
                StormDamage = 2.0f,
                TemperatureDamage = 1.5f,
                LeafDropRate = 1.8f,
                HealthRegen = 0.6f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Medium
            }
        },
        {
            WorldType.Europa,
            new WorldModifier
            {
                SolarMultiplier = 0.12f,
                GravityMultiplier = 0.13f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -55.0f,
                IsMeteoOn = false,
                LimitMultiplier = 5.0f,
                GrowthRateMultiplier = 0.35f,
                WaterConsumption = 0.6f,
                OxygenConsumption = 2.0f,
                EnergyDrain = 1.8f,
                ParasiteChance = 0.1f,
                ParasiteDamage = 0.8f,
                StormChance = 0.0f,
                StormDamage = 0.0f,
                TemperatureDamage = 2.5f,
                LeafDropRate = 2.5f,
                HealthRegen = 0.4f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.MediumHard
            }
        },
        {
            WorldType.Venere,
            new WorldModifier
            {
                SolarMultiplier = 0.2f,
                GravityMultiplier = 0.9f,
                OxygenLevel = 0.0f,
                TemperatureModifier = 55.0f,
                IsMeteoOn = true,
                LimitMultiplier = 4.0f,
                GrowthRateMultiplier = 0.3f,
                WaterConsumption = 3.5f,
                OxygenConsumption = 2.2f,
                EnergyDrain = 2.0f,
                ParasiteChance = 0.0f,
                ParasiteDamage = 0.0f,
                StormChance = 2.5f,
                StormDamage = 3.0f,
                TemperatureDamage = 3.0f,
                LeafDropRate = 3.0f,
                HealthRegen = 0.3f,
                HydrationFromRain = -0.5f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Hard
            }
        },
        {
            WorldType.Titano,
            new WorldModifier
            {
                SolarMultiplier = 0.08f,
                GravityMultiplier = 0.14f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -65.0f,
                IsMeteoOn = true,
                LimitMultiplier = 8.0f,
                GrowthRateMultiplier = 0.25f,
                WaterConsumption = 0.4f,
                OxygenConsumption = 2.5f,
                EnergyDrain = 2.5f,
                ParasiteChance = 0.5f,
                ParasiteDamage = 2.0f,
                StormChance = 1.5f,
                StormDamage = 2.0f,
                TemperatureDamage = 3.5f,
                LeafDropRate = 3.5f,
                HealthRegen = 0.25f,
                HydrationFromRain = 0.3f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.VeryHard
            }
        },
        {
            WorldType.ReameMistico,
            new WorldModifier
            {
                SolarMultiplier = 0.4f,
                GravityMultiplier = 0.6f,
                OxygenLevel = 0.5f,
                TemperatureModifier = -15.0f,
                IsMeteoOn = true,
                LimitMultiplier = 15.0f,
                GrowthRateMultiplier = 0.35f,
                WaterConsumption = 2.0f,
                OxygenConsumption = 1.5f,
                EnergyDrain = 2.8f,
                ParasiteChance = 3.0f,
                ParasiteDamage = 2.5f,
                StormChance = 2.0f,
                StormDamage = 2.5f,
                TemperatureDamage = 2.0f,
                LeafDropRate = 2.0f,
                HealthRegen = 0.4f,
                HydrationFromRain = 0.8f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Extreme
            }
        },
        {
            WorldType.GiardinoMistico,
            new WorldModifier
            {
                SolarMultiplier = 1.5f,
                GravityMultiplier = 0.8f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 5.0f,
                IsMeteoOn = true,
                LimitMultiplier = 30.0f,
                GrowthRateMultiplier = 0.8f,
                WaterConsumption = 4.0f,
                OxygenConsumption = 1.0f,
                EnergyDrain = 3.5f,
                ParasiteChance = 5.0f,
                ParasiteDamage = 3.0f,
                StormChance = 1.0f,
                StormDamage = 1.5f,
                TemperatureDamage = 1.5f,
                LeafDropRate = 4.0f,
                HealthRegen = 0.5f,
                HydrationFromRain = 1.2f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Nightmare
            }
        },
        {
            WorldType.Origine,
            new WorldModifier
            {
                SolarMultiplier = 0.25f,
                GravityMultiplier = 1.5f,
                OxygenLevel = 0.2f,
                TemperatureModifier = 25.0f,
                IsMeteoOn = true,
                LimitMultiplier = 100.0f,
                GrowthRateMultiplier = 0.2f,
                WaterConsumption = 5.0f,
                OxygenConsumption = 3.0f,
                EnergyDrain = 4.0f,
                ParasiteChance = 4.0f,
                ParasiteDamage = 4.0f,
                StormChance = 3.0f,
                StormDamage = 4.0f,
                TemperatureDamage = 3.0f,
                LeafDropRate = 5.0f,
                HealthRegen = 0.15f,
                HydrationFromRain = 0.5f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Impossible
            }
        }
    };

    private static readonly Dictionary<WorldType, (Color ground1, Color ground2, Color nextGround1, Color nextGround2)> worldColors = new()
    {
        { WorldType.Serra, (new Color(80, 60, 40, 255), new Color(60, 45, 30, 255), Color.DarkGreen, Color.Brown) },
        { WorldType.Terra, (Color.DarkGreen, Color.Brown, new Color(180, 180, 190, 255), new Color(140, 140, 150, 255)) },
        { WorldType.Luna, (new Color(180, 180, 190, 255), new Color(140, 140, 150, 255), new Color(180, 100, 80, 255), new Color(140, 70, 50, 255)) },
        { WorldType.Marte, (new Color(180, 100, 80, 255), new Color(140, 70, 50, 255), new Color(200, 220, 240, 255), new Color(160, 190, 220, 255)) },
        { WorldType.Europa, (new Color(200, 220, 240, 255), new Color(160, 190, 220, 255), new Color(220, 180, 140, 255), new Color(180, 140, 100, 255)) },
        { WorldType.Venere, (new Color(220, 180, 140, 255), new Color(180, 140, 100, 255), new Color(120, 100, 80, 255), new Color(90, 75, 60, 255)) },
        { WorldType.Titano, (new Color(120, 100, 80, 255), new Color(90, 75, 60, 255), new Color(100, 80, 140, 255), new Color(70, 50, 100, 255)) },
        { WorldType.ReameMistico, (new Color(100, 80, 140, 255), new Color(70, 50, 100, 255), new Color(80, 180, 120, 255), new Color(50, 140, 90, 255)) },
        { WorldType.GiardinoMistico, (new Color(80, 180, 120, 255), new Color(50, 140, 90, 255), new Color(60, 50, 70, 255), new Color(40, 30, 50, 255)) },
        { WorldType.Origine, (new Color(60, 50, 70, 255), new Color(40, 30, 50, 255), Color.Blank, Color.Blank) }
    };

    public static void SetCurrentWorld(WorldType world)
    {
        currentWorld = world;
        UpdateGroundColors();
        GameSave.get().Save();
    }

    private static void UpdateGroundColors()
    {
        if (worldColors.TryGetValue(currentWorld, out var colors))
        {
            if(Game.ground != null)
            Game.ground.SetGroundWorld(colors.ground1, colors.ground2, colors.nextGround1, colors.nextGround2);
        }
    }

    public static WorldType GetCurrentWorld() => currentWorld;
    public static WorldModifier GetCurrentModifiers() => GetModifiers(currentWorld);

    public static WorldModifier GetModifiers(WorldType world) =>
        modifiers.TryGetValue(world, out var mod) ? mod : new WorldModifier();

    public static WorldType GetNextWorld(WorldType current) => current switch
    {
        WorldType.Serra => WorldType.Terra,
        WorldType.Terra => WorldType.Luna,
        WorldType.Luna => WorldType.Marte,
        WorldType.Marte => WorldType.Europa,
        WorldType.Europa => WorldType.Venere,
        WorldType.Venere => WorldType.Titano,
        WorldType.Titano => WorldType.ReameMistico,
        WorldType.ReameMistico => WorldType.GiardinoMistico,
        WorldType.GiardinoMistico => WorldType.Origine,
        WorldType.Origine => WorldType.Terra,
        _ => WorldType.Terra
    };

    public static WorldType GetPreviousWorld(WorldType current) => current switch
    {
        WorldType.Terra => WorldType.Serra,
        WorldType.Luna => WorldType.Terra,
        WorldType.Marte => WorldType.Luna,
        WorldType.Europa => WorldType.Marte,
        WorldType.Venere => WorldType.Europa,
        WorldType.Titano => WorldType.Venere,
        WorldType.ReameMistico => WorldType.Titano,
        WorldType.GiardinoMistico => WorldType.ReameMistico,
        WorldType.Origine => WorldType.GiardinoMistico,
        _ => WorldType.Terra
    };

    public static string GetWorldName(WorldType world) => world switch
    {
        WorldType.Terra => "Terra",
        WorldType.Luna => "Luna",
        WorldType.Marte => "Marte",
        WorldType.Europa => "Europa",
        WorldType.Venere => "Venere",
        WorldType.Titano => "Titano",
        WorldType.ReameMistico => "Reame Mistico",
        WorldType.GiardinoMistico => "Giardino Mistico",
        WorldType.Origine => "L'Origine",
        WorldType.Serra => "Serra",
        _ => "Sconosciuto"
    };

    public static string GetWorldDescription(WorldType world) => world switch
    {
        WorldType.Serra => "Ambiente protetto per imparare. Impossibile fallire.",
        WorldType.Terra => "Il nostro pianeta. Condizioni ideali per iniziare.",
        WorldType.Luna => "Primo passo verso le stelle. Porta ossigeno!",
        WorldType.Marte => "Il pianeta rosso. Tempeste di sabbia e freddo.",
        WorldType.Europa => "Luna ghiacciata. Il freddo è il tuo nemico.",
        WorldType.Venere => "Inferno. Caldo estremo e piogge acide.",
        WorldType.Titano => "Mondo alieno. Quasi tutto è contro di te.",
        WorldType.ReameMistico => "Regno magico. Creature ovunque.",
        WorldType.GiardinoMistico => "Sembra un paradiso... non lo è.",
        WorldType.Origine => "Dove tutto ha inizio. Sfida finale.",
        _ => "???"
    };

    public static string GetDifficultyName(WorldDifficulty diff) => diff switch
    {
        WorldDifficulty.Tutorial => "Tutorial",
        WorldDifficulty.Easy => "Facile",
        WorldDifficulty.Normal => "Normale",
        WorldDifficulty.Medium => "Medio",
        WorldDifficulty.MediumHard => "Medio-Difficile",
        WorldDifficulty.Hard => "Difficile",
        WorldDifficulty.VeryHard => "Molto Difficile",
        WorldDifficulty.Extreme => "Estremo",
        WorldDifficulty.Nightmare => "Incubo",
        WorldDifficulty.Impossible => "Impossibile",
        _ => "???"
    };

    public static WorldDifficulty GetDifficultyFromName(string name) => name switch
    {
        "Tutorial" => WorldDifficulty.Tutorial,
        "Facile" => WorldDifficulty.Easy,
        "Normale" => WorldDifficulty.Normal,
        "Medio" => WorldDifficulty.Medium,
        "Medio-Difficile" => WorldDifficulty.MediumHard,
        "Difficile" => WorldDifficulty.Hard,
        "Molto Difficile" => WorldDifficulty.VeryHard,
        "Estremo" => WorldDifficulty.Extreme,
        "Incubo" => WorldDifficulty.Nightmare,
        "Impossibile" => WorldDifficulty.Impossible,
        _ => WorldDifficulty.Normal
    };

    private static Dictionary<WorldType, WorldModifier> worldModifiers = new()
    {
        {
            WorldType.Serra,
            new WorldModifier
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 0.0f,
                IsMeteoOn = true,
                LimitMultiplier = 1.0f,
                GrowthRateMultiplier = 1.0f,
                WaterConsumption = 1.0f,
                OxygenConsumption = 1.0f,
                EnergyDrain = 1.0f,
                ParasiteChance = 1.0f,
                ParasiteDamage = 1.0f,
                StormChance = 1.0f,
                StormDamage = 1.0f,
                TemperatureDamage = 1.0f,
                LeafDropRate = 1.0f,
                HealthRegen = 1.0f,
                HydrationFromRain = 1.0f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Normal
            }
        },
        {
            WorldType.Terra,
            new WorldModifier
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 0.0f,
                IsMeteoOn = true,
                LimitMultiplier = 1.0f,
                GrowthRateMultiplier = 1.0f,
                WaterConsumption = 1.0f,
                OxygenConsumption = 1.0f,
                EnergyDrain = 1.0f,
                ParasiteChance = 1.0f,
                ParasiteDamage = 1.0f,
                StormChance = 1.0f,
                StormDamage = 1.0f,
                TemperatureDamage = 1.0f,
                LeafDropRate = 1.0f,
                HealthRegen = 1.0f,
                HydrationFromRain = 1.0f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Normal
            }
        },
        {
            WorldType.Luna,
            new WorldModifier
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 0.16f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -40.0f,
                IsMeteoOn = false,
                LimitMultiplier = 2.5f,
                GrowthRateMultiplier = 0.5f,
                WaterConsumption = 2.0f,
                OxygenConsumption = 1.5f,
                EnergyDrain = 1.5f,
                ParasiteChance = 0.5f,
                ParasiteDamage = 0.5f,
                StormChance = 0.0f,
                StormDamage = 0.0f,
                TemperatureDamage = 2.0f,
                LeafDropRate = 1.5f,
                HealthRegen = 0.5f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Medium
            }
        },
        {
            WorldType.Marte,
            new WorldModifier
            {
                SolarMultiplier = 0.4f,
                GravityMultiplier = 0.38f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -20.0f,
                IsMeteoOn = true,
                LimitMultiplier = 3.0f,
                GrowthRateMultiplier = 0.4f,
                WaterConsumption = 2.5f,
                OxygenConsumption = 1.8f,
                EnergyDrain = 1.8f,
                ParasiteChance = 0.3f,
                ParasiteDamage = 0.7f,
                StormChance = 1.5f,
                StormDamage = 1.5f,
                TemperatureDamage = 2.0f,
                LeafDropRate = 2.0f,
                HealthRegen = 0.4f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Hard
            }
        },
        {
            WorldType.Europa,
            new WorldModifier
            {
                SolarMultiplier = 0.05f,
                GravityMultiplier = 0.13f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -80.0f,
                IsMeteoOn = true,
                LimitMultiplier = 3.5f,
                GrowthRateMultiplier = 0.35f,
                WaterConsumption = 2.5f,
                OxygenConsumption = 2.0f,
                EnergyDrain = 2.0f,
                ParasiteChance = 0.2f,
                ParasiteDamage = 0.6f,
                StormChance = 0.5f,
                StormDamage = 1.0f,
                TemperatureDamage = 2.5f,
                LeafDropRate = 2.0f,
                HealthRegen = 0.35f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Hard
            }
        },
        {
            WorldType.Venere,
            new WorldModifier
            {
                SolarMultiplier = 0.2f,
                GravityMultiplier = 0.9f,
                OxygenLevel = 0.0f,
                TemperatureModifier = 55.0f,
                IsMeteoOn = true,
                LimitMultiplier = 4.0f,
                GrowthRateMultiplier = 0.3f,
                WaterConsumption = 3.5f,
                OxygenConsumption = 2.2f,
                EnergyDrain = 2.0f,
                ParasiteChance = 0.0f,
                ParasiteDamage = 0.0f,
                StormChance = 2.5f,
                StormDamage = 3.0f,
                TemperatureDamage = 3.0f,
                LeafDropRate = 3.0f,
                HealthRegen = 0.3f,
                HydrationFromRain = -0.5f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.Hard
            }
        },
        {
            WorldType.Titano,
            new WorldModifier
            {
                SolarMultiplier = 0.08f,
                GravityMultiplier = 0.14f,
                OxygenLevel = 0.0f,
                TemperatureModifier = -100.0f,
                IsMeteoOn = true,
                LimitMultiplier = 4.5f,
                GrowthRateMultiplier = 0.25f,
                WaterConsumption = 3.0f,
                OxygenConsumption = 2.5f,
                EnergyDrain = 2.5f,
                ParasiteChance = 0.1f,
                ParasiteDamage = 0.4f,
                StormChance = 1.0f,
                StormDamage = 1.5f,
                TemperatureDamage = 3.5f,
                LeafDropRate = 2.5f,
                HealthRegen = 0.25f,
                HydrationFromRain = 0.0f,
                RequiresOxygenTank = true,
                Difficulty = WorldDifficulty.VeryHard
            }
        },
        {
            WorldType.ReameMistico,
            new WorldModifier
            {
                SolarMultiplier = 0.15f,
                GravityMultiplier = 0.8f,
                OxygenLevel = 0.3f,
                TemperatureModifier = 10.0f,
                IsMeteoOn = true,
                LimitMultiplier = 5.0f,
                GrowthRateMultiplier = 0.4f,
                WaterConsumption = 2.0f,
                OxygenConsumption = 1.5f,
                EnergyDrain = 1.5f,
                ParasiteChance = 2.0f,
                ParasiteDamage = 2.0f,
                StormChance = 3.0f,
                StormDamage = 2.5f,
                TemperatureDamage = 1.5f,
                LeafDropRate = 4.0f,
                HealthRegen = 0.4f,
                HydrationFromRain = 1.0f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Extreme
            }
        },
        {
            WorldType.GiardinoMistico,
            new WorldModifier
            {
                SolarMultiplier = 0.1f,
                GravityMultiplier = 0.6f,
                OxygenLevel = 0.5f,
                TemperatureModifier = -5.0f,
                IsMeteoOn = true,
                LimitMultiplier = 6.0f,
                GrowthRateMultiplier = 0.35f,
                WaterConsumption = 2.5f,
                OxygenConsumption = 1.8f,
                EnergyDrain = 2.0f,
                ParasiteChance = 3.0f,
                ParasiteDamage = 3.0f,
                StormChance = 4.0f,
                StormDamage = 3.5f,
                TemperatureDamage = 2.0f,
                LeafDropRate = 5.0f,
                HealthRegen = 0.35f,
                HydrationFromRain = 1.5f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Nightmare
            }
        },
        {
            WorldType.Origine,
            new WorldModifier
            {
                SolarMultiplier = 0.25f,
                GravityMultiplier = 1.5f,
                OxygenLevel = 0.2f,
                TemperatureModifier = 25.0f,
                IsMeteoOn = true,
                LimitMultiplier = 100.0f,
                GrowthRateMultiplier = 0.2f,
                WaterConsumption = 5.0f,
                OxygenConsumption = 3.0f,
                EnergyDrain = 4.0f,
                ParasiteChance = 4.0f,
                ParasiteDamage = 4.0f,
                StormChance = 3.0f,
                StormDamage = 4.0f,
                TemperatureDamage = 3.0f,
                LeafDropRate = 5.0f,
                HealthRegen = 0.15f,
                HydrationFromRain = 0.5f,
                RequiresOxygenTank = false,
                Difficulty = WorldDifficulty.Impossible
            }
        }
    };

    // Missing methods needed for save/load system
    private static WorldDifficulty currentDifficulty = WorldDifficulty.Normal;

    public static WorldDifficulty GetCurrentWorldDifficulty() => currentDifficulty;

    public static void SetWorldDifficulty(WorldType world, WorldDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        // Update current world difficulty
        if (worldModifiers.ContainsKey(world))
        {
            worldModifiers[world] = worldModifiers[world] with { Difficulty = difficulty };
        }

        
        GameSave.get().Save();
    }

    public static Color GetDifficultyColor(WorldDifficulty diff) => diff switch
    {
        WorldDifficulty.Tutorial => new Color(150, 200, 150, 255),
        WorldDifficulty.Easy => new Color(100, 200, 100, 255),
        WorldDifficulty.Normal => new Color(200, 200, 100, 255),
        WorldDifficulty.Medium => new Color(220, 180, 80, 255),
        WorldDifficulty.MediumHard => new Color(240, 150, 60, 255),
        WorldDifficulty.Hard => new Color(255, 120, 50, 255),
        WorldDifficulty.VeryHard => new Color(255, 80, 80, 255),
        WorldDifficulty.Extreme => new Color(200, 50, 50, 255),
        WorldDifficulty.Nightmare => new Color(150, 30, 100, 255),
        WorldDifficulty.Impossible => new Color(100, 0, 100, 255),
        _ => Color.White
    };

    public static int GetWorldIndex(WorldType world) => world switch
    {
        WorldType.Serra => 0,
        WorldType.Terra => 1,
        WorldType.Luna => 2,
        WorldType.Marte => 3,
        WorldType.Europa => 4,
        WorldType.Venere => 5,
        WorldType.Titano => 6,
        WorldType.ReameMistico => 7,
        WorldType.GiardinoMistico => 8,
        WorldType.Origine => 9,
        _ => 0
    };

    public static int GetTotalWorlds() => 10;
    public static void SetNextWorld() { 
        SetCurrentWorld(GetNextWorld(currentWorld)); 
        LeafHarvestSystem.HarvestAndShow("Cambio Mondo"); 
    }
    public static void SetPreviousWorld() {
        SetCurrentWorld(GetPreviousWorld(currentWorld)); 
        LeafHarvestSystem.HarvestAndShow("Cambio Mondo"); 
    }
}
using System;
using System.Collections.Generic;

namespace Plants;

public enum WorldType
{
    Terra,
    Luna
    // man mano che pensiamo ad i mondi li aggiungiamo qui
}


public struct WorldModifier
{
    public float SolarMultiplier = 1.0f;
    public float GravityMultiplier = 1.0f;
    public float OxygenLevel = 1.0f;
    public float TemperatureModifier = 0.0f;  
    public bool IsMeteoOn = true;

    public float LimitMultiplier = 1.0f;
    public float GrowthRateMultiplier = 1.0f;
    public float WaterConsumption = 1.0f;

    public WorldModifier()
    {

    }
}

public class WorldManager
{
    private static WorldType currentWorld;

    private static Dictionary<WorldType, WorldModifier> modifiers = new()
    {
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
                    WaterConsumption = 1.0f
                }
        },
        {
            WorldType.Luna,
                new WorldModifier
                {
                    SolarMultiplier = 0.6f,
                    GravityMultiplier = 0.165f,
                    OxygenLevel = 0.0f,
                    TemperatureModifier = -25.0f, 
                    IsMeteoOn = false,
                    LimitMultiplier = 1.5f,
                    GrowthRateMultiplier = 0.8f,
                    WaterConsumption = 0.7f
                }
        }
    };


    public static void SetCurrentWorld(WorldType world)
    {
        currentWorld = world;
    }
    public static WorldType GetCurrentWorld()
    {
        return currentWorld;
    }

    public static WorldModifier GetCurrentModifiers()
    {
        return GetModifiers(currentWorld);
    }

    public static WorldModifier GetModifiers(WorldType world)
    {
        return modifiers.GetValueOrDefault(world);
    }
}
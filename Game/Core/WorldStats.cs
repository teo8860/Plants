using System;

namespace Plants;

public enum WorldType 
{
    Terra,
    Luna
    // man mano che pensiamo ad i mondi li aggiungiamo qui
}

public class WorldModifiers
{

    public float SolarMultiplier = 1.0f;      
    public float GravityMultiplier = 1.0f;   
    public float OxygenLevel = 1.0f;
    public float TemperatureModifier = 0.0f;
    public bool IsMeteoOn = true;
           
    public float LimitMultiplier = 1.0f;
    public float GrowthRateMultiplier = 1.0f;
    public float WaterConsumption = 1.0f;

    public static WorldModifiers GetModifiers(WorldType world)
    {
        return world switch
        {
            WorldType.Terra => new WorldModifiers
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                IsMeteoOn = true,
                LimitMultiplier = 1.0f,

                GrowthRateMultiplier = 1.0f,
                WaterConsumption = 1.0f
            },

            WorldType.Luna => new WorldModifiers
            {
                SolarMultiplier = 0.6f,    
                GravityMultiplier = 0.165f, 
                OxygenLevel = 0.0f,
                IsMeteoOn = false,
                LimitMultiplier = 1.5f,

                GrowthRateMultiplier = 0.8f, 
                WaterConsumption = 0.7f    
            },

            _ => new WorldModifiers() 
        };
    }
}
using Raylib_CSharp.Colors;
using System;
using System.Collections;
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
    Serra  //Per il tutorial
}


public struct WorldModifier
{
    public float SolarMultiplier = 1.0f;        // Luce solare disponibile
    public float GravityMultiplier = 1.0f;      // Gravità (influenza crescita verticale)
    public float OxygenLevel = 1.0f;            // Livello ossigeno (0 = serve tank)
    public float TemperatureModifier = 0.0f;    // Modifica temperatura base
    public bool IsMeteoOn = true;               // Sistema meteo attivo

    public float LimitMultiplier = 1.0f;        // Moltiplicatore altezza massima
    public float GrowthRateMultiplier = 1.0f;   // Velocità crescita
    public float WaterConsumption = 1.0f;       // Consumo acqua

    public WorldModifier() { }
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
                WaterConsumption = 1.0f,
            }
        },
        
        {
            WorldType.Luna,
            new WorldModifier
            {
                SolarMultiplier = 0.6f,        
                GravityMultiplier = 0.16f,      
                OxygenLevel = 0.0f,             
                TemperatureModifier = -25.0f,   
                IsMeteoOn = false,             
                LimitMultiplier = 2.5f,         
                GrowthRateMultiplier = 0.7f,    
                WaterConsumption = 0.5f,        
            }
        },
        
        {
            WorldType.Marte,
            new WorldModifier
            {
                SolarMultiplier = 0.43f,        
                GravityMultiplier = 0.38f,      
                OxygenLevel = 0.01f,            
                TemperatureModifier = -60.0f,   
                IsMeteoOn = true,               
                LimitMultiplier = 5f,        
                GrowthRateMultiplier = 0.5f,    
                WaterConsumption = 0.4f,        
            }
        },

        {
            WorldType.Europa,
            new WorldModifier
            {
                SolarMultiplier = 0.04f,        
                GravityMultiplier = 0.13f,     
                OxygenLevel = 0.0f,            
                TemperatureModifier = -170.0f,  
                IsMeteoOn = false,               
                LimitMultiplier = 10f,         
                GrowthRateMultiplier = 0.3f,    
                WaterConsumption = 0.2f,        
            }
        },
        
        {
            WorldType.Venere,
            new WorldModifier
            {
                SolarMultiplier = 0.1f,         
                GravityMultiplier = 0.9f,       
                OxygenLevel = 0.0f,             
                TemperatureModifier = 445.0f,   
                IsMeteoOn = true,               
                LimitMultiplier = 20f,         
                GrowthRateMultiplier = 0.2f,    
                WaterConsumption = 5.0f,        
            }
        },

        {
            WorldType.Titano,
            new WorldModifier
            {
                SolarMultiplier = 0.01f,        
                GravityMultiplier = 0.14f,      
                OxygenLevel = 0.0f,             
                TemperatureModifier = -179.0f,  
                IsMeteoOn = true,               
                LimitMultiplier = 50f,         
                GrowthRateMultiplier = 0.25f,  
                WaterConsumption = 0.1f,       
            }
        },
        
        {
            WorldType.ReameMistico,
            new WorldModifier
            {
                SolarMultiplier = 0.5f,         
                GravityMultiplier = 0.7f,       
                OxygenLevel = 0.6f,             
                TemperatureModifier = -10.0f,   
                IsMeteoOn = true,               
                LimitMultiplier = 100f,
                GrowthRateMultiplier = 0.4f,    
                WaterConsumption = 1.5f,        
            }
        },

        {
            WorldType.GiardinoMistico,
            new WorldModifier
            {
                SolarMultiplier = 1.5f,             
                GravityMultiplier = 1.0f,       
                OxygenLevel = 1.2f,             
                TemperatureModifier = 5.0f,     
                IsMeteoOn = true,               
                LimitMultiplier = 500f,         
                GrowthRateMultiplier = 1.5f,    
                WaterConsumption = 2.5f,        
            }
        },

        {
            WorldType.Origine,
            new WorldModifier
            {
                SolarMultiplier = 0.3f,         
                GravityMultiplier = 1.5f,       
                OxygenLevel = 0.3f,             
                TemperatureModifier = 30.0f,   
                IsMeteoOn = true,               
                LimitMultiplier = 0f,     //infinito    
                GrowthRateMultiplier = 0.3f,    
                WaterConsumption = 2.0f,        
            }
        },

        {
            WorldType.Serra,
            new WorldModifier
            {
                SolarMultiplier = 1.0f,
                GravityMultiplier = 1.0f,
                OxygenLevel = 1.0f,
                TemperatureModifier = 0.0f,
                IsMeteoOn = false,
                LimitMultiplier = 0.5f,
                GrowthRateMultiplier = 1.25f,
                WaterConsumption = 0.75f,
            }
        }
    };


    public static void SetCurrentWorld(WorldType world)
    {
        currentWorld = world;
        UpdateGroundColors();
    }

    private static void UpdateGroundColors()
    {
        switch (currentWorld)
        {
            case WorldType.Terra:
                Game.ground.SetGroundWorld(
                    Color.DarkGreen, Color.Brown,
                    Color.DarkGray, Color.LightGray
                );
                break;
            case WorldType.Luna:
                Game.ground.SetGroundWorld(
                    Color.DarkGray, Color.LightGray,
                    new Color(139, 69, 19, 255), Color.Orange
                );
                break;
            case WorldType.Marte:
                Game.ground.SetGroundWorld(
                    new Color(139, 69, 19, 255), new Color(205, 92, 0, 255),
                    new Color(200, 200, 220, 255), Color.White
                );
                break;
            case WorldType.Europa:
                Game.ground.SetGroundWorld(
                    new Color(200, 220, 255, 255), new Color(150, 180, 220, 255),
                    new Color(255, 200, 100, 255), new Color(255, 150, 50, 255)
                );
                break;
            case WorldType.Venere:
                Game.ground.SetGroundWorld(
                    new Color(255, 140, 0, 255), new Color(139, 69, 19, 255),
                    new Color(100, 80, 60, 255), new Color(80, 60, 40, 255)
                );
                break;
            case WorldType.Titano:
                Game.ground.SetGroundWorld(
                    new Color(100, 80, 60, 255), new Color(139, 90, 43, 255),
                    new Color(100, 50, 150, 255), new Color(150, 80, 200, 255)
                );
                break;
            case WorldType.ReameMistico:
                Game.ground.SetGroundWorld(
                    new Color(100, 50, 150, 255), new Color(80, 30, 120, 255),
                    new Color(50, 150, 100, 255), new Color(80, 200, 120, 255)
                );
                break;
            case WorldType.GiardinoMistico:
                Game.ground.SetGroundWorld(
                    new Color(50, 180, 80, 255), new Color(80, 60, 40, 255),
                    new Color(20, 20, 30, 255), new Color(50, 30, 60, 255)
                );
                break;
            case WorldType.Origine:
                Game.ground.SetGroundWorld(
                    new Color(30, 20, 40, 255), new Color(60, 40, 20, 255),
                    Color.Blank, Color.Blank
                );
                break;
        }
    }

    public static WorldType GetCurrentWorld() => currentWorld;

    public static WorldModifier GetCurrentModifiers() => GetModifiers(currentWorld);

    public static WorldModifier GetModifiers(WorldType world) =>
        modifiers.GetValueOrDefault(world);

    public static WorldType GetNextWorld(WorldType current)
    {
        return current switch
        {
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
    }

    public static void SetNextWorld()
        {
        SetCurrentWorld(GetNextWorld(currentWorld));
    }
}
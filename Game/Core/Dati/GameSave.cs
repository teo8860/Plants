using Engine.Tools;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;


public class GameSaveData
{
    public WorldType CurrentWorld   { get; set; }
    public WorldDifficulty CurrentDifficulty   { get; set; }
    public Weather CurrentWeather   { get; set; }
    public DayPhase CurrentPhase   { get; set; }
    public DateTime SaveTime   { get; set; }
    public string Version { get; set; }
    public int essence { get; set; }
    public PlantStats PlantStats { get; set; }
    public SeedType PlantSeedType { get; set; }
    public SeedStats PlantSeedBonus { get; set; }

    public GameSaveData()
    {
        Version = "1.0.0";
        PlantStats = new PlantStats();
        PlantSeedBonus = new SeedStats();
    }
}

public class GameSave
{
    private const string SaveFileName = "savegame.json";
    public  GameSaveData data = new();

    
    private static GameSave instance = null;

    public static GameSave get()
    {
        if (GameSave.instance == null)
            GameSave.instance = new GameSave();

        return GameSave.instance;
    }


    private GameSave()
    {
        data = new();
    }

     public void Save()
    {
        if (Game.pianta != null)
        {
            data.PlantStats = Game.pianta.Stats;
            data.PlantSeedType = Game.pianta.TipoSeme;
            data.PlantSeedBonus = Game.pianta.seedBonus;
        }

        data.SaveTime = DateTime.Now;

        SaveHelper.Save(SaveFileName, data);
    }

    public void Load()
    {
        data = SaveHelper.Load<GameSaveData>(SaveFileName);

        if (data == null)
            data = new();

         var saveData = GameSave.get().data;

		WorldManager.SetCurrentWorld(saveData.CurrentWorld);
        WorldManager.SetWorldDifficulty(saveData.CurrentWorld, saveData.CurrentDifficulty);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        FaseGiorno.SetCurrentPhase(saveData.CurrentPhase);
        SeedUpgradeSystem.SetEssence(saveData.essence);

        Game.pianta.Stats = saveData.PlantStats;
        Game.pianta.TipoSeme = saveData.PlantSeedType;
        Game.pianta.seedBonus = saveData.PlantSeedBonus;

        CalculateOfflineGrowth();
    }

    private void CalculateOfflineGrowth()
    {
        if (data.SaveTime == default(DateTime))
        {
            data.SaveTime = DateTime.Now;
            return;
        }

        TimeSpan timeOffline = DateTime.Now - data.SaveTime;

        if (timeOffline.TotalMinutes < 1)
            return;
        SimulateOfflineGrowth(timeOffline);
    }

    private void SimulateOfflineGrowth(TimeSpan timeOffline)
    {
        if (Game.pianta == null || data.PlantStats == null)
            return;

        int growthTicks = (int)timeOffline.TotalSeconds;

        for (int i = 0; i < growthTicks; i++)
        {
            if (Game.pianta.Stats.Altezza >= Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)
                break;

            Game.pianta.proprieta.AggiornaTutto(
                FaseGiorno.GetCurrentPhase(),
                WeatherManager.GetCurrentWeather(),
                WorldManager.GetCurrentModifiers()
            );
        }
    }
    public static void DeleteSaveFile()
    {
        try
        {
            SaveHelper.Delete(SaveFileName);
            Console.WriteLine("Save file deleted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting save file: {ex.Message}");
        }
    }

    public static DateTime? GetLastSaveTime()
    {
        try
        {
            var saveData = SaveHelper.Load<GameSaveData>(SaveFileName);
            return saveData?.SaveTime;
        }
        catch
        {
            return null;
        }
    }
}
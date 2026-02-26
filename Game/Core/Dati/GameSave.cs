using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class RamoSaveData
{
    public List<Vector2> Punti { get; set; } = new();
}

public class RadiceSaveData
{
    public List<Vector2> Start { get; set; } = new();
    public List<Vector2> End { get; set; } = new();
    public List<RadiceSaveData> Rami { get; set; } = new();
    public int Generazione { get; set; }
}

public class EderaSaveData
{
    public List<Vector2> Punti { get; set; } = new();
}

public class PlantSaveData
{
    public List<Vector2> PuntiSpline { get; set; } = new();
    public List<RamoSaveData> Rami { get; set; } = new();
    public List<RadiceSaveData> Radici { get; set; } = new();
    public List<EderaSaveData> Edera { get; set; } = new();
    public bool EderaCreata { get; set; }
    public int ContatorePuntiPerRamo { get; set; }
    public int ContatorePuntiPerRadice { get; set; }
}

public class GameSaveData
{
    public WorldType CurrentWorld   { get; set; }
    public WorldDifficulty CurrentDifficulty   { get; set; }
    public Weather CurrentWeather   { get; set; }
    public DayPhase CurrentPhase   { get; set; }
    public DateTime SaveTime   { get; set; }
    public string Version { get; set; }
    public int essence { get; set; }
    public int CurrentStage { get; set; } = 1;
    public PlantStats PlantStats { get; set; }
    public SeedType PlantSeedType { get; set; }
    public SeedStats PlantSeedBonus { get; set; }
    public int randomSeed {get; set; }

    public float WaterCurrent { get; set; } = 100f;
    public float WaterMax { get; set; } = 100f;

    public PlantSaveData Plant { get; set; }

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
            data.randomSeed = Game.pianta.rseed;
            data.Plant = Game.pianta.ToSaveData();
		}

        data.WaterCurrent = WaterSystem.Current;
        data.WaterMax = WaterSystem.Max;

        data.SaveTime = DateTime.Now;
        data.CurrentStage = WorldManager.GetCurrentStage();

        SaveHelper.Save(SaveFileName, data);
    }

    public void Load()
    {
        data = SaveHelper.Load<GameSaveData>(SaveFileName);

        if (data == null)
        {
            Console.WriteLine("No save data found, starting new game.");
			data = new();
            WorldManager.SetCurrentWorld(WorldType.Terra);
            return;
        }

        var saveData = GameSave.get().data;

        Game.pianta.Stats = saveData.PlantStats;
        Game.pianta.TipoSeme = saveData.PlantSeedType;
        Game.pianta.seedBonus = saveData.PlantSeedBonus;
        Game.pianta.rseed = saveData.randomSeed;

        if (saveData.Plant != null && saveData.Plant.PuntiSpline.Count > 0)
        {
            Game.pianta.RestoreFromSaveData(saveData.Plant);
            int ramiMassimi = Math.Max(1, Game.pianta.proprieta.FoglieMassime / 5);
            Console.WriteLine($"Loaded plant: {saveData.Plant.PuntiSpline.Count} spline points, {saveData.Plant.Rami.Count}/{ramiMassimi} branches, {saveData.Plant.Radici.Count} roots, foglie {Game.pianta.Stats.FoglieAttuali}/{Game.pianta.proprieta.FoglieMassime}");
        }

		WorldManager.SetCurrentWorld(saveData.CurrentWorld);
        WorldManager.SetWorldDifficulty(saveData.CurrentWorld, saveData.CurrentDifficulty);
        int loadedStage = saveData.CurrentStage > 0 ? saveData.CurrentStage : 1;
        WorldManager.SetCurrentStage(loadedStage);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        FaseGiorno.SetCurrentPhase(saveData.CurrentPhase);
        SeedUpgradeSystem.SetEssence(saveData.essence);

        WaterSystem.Current = saveData.WaterCurrent;
        WaterSystem.Max = saveData.WaterMax;

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

        WaterSystem.AddOfflineRecharge(timeOffline.TotalSeconds);

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
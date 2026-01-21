using System;
using System.Collections.Generic;
using Engine.Tools;

namespace Plants;


public class GameSaveData
{
    public WorldType CurrentWorld   { get; set; }
    public WorldDifficulty CurrentDifficulty   { get; set; }
    public Weather CurrentWeather   { get; set; }
    public DayPhase CurrentPhase   { get; set; }
    public CompostSaveData CompostData   { get; set; }
    public DateTime SaveTime   { get; set; }
    public string Version { get; set; }

    public GameSaveData()
    {
        Version = "1.0.0";
    }
}

public class CompostSaveData
{
    public int CollectedLeaves { get; set; }
    public List<SeedPackageRarity> AvailablePackages { get; set; }
    public float CompostEfficiency { get; set; }

    public CompostSaveData()
    {
        AvailablePackages = new();
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
        SaveHelper.Save(SaveFileName, data);
    }

    public void Load()
    {
        data = SaveHelper.Load<GameSaveData>(SaveFileName);

        if (data == null)
            data = new();

         var saveData = GameSave.get().data;

		// Restore game state
		WorldManager.SetCurrentWorld(saveData.CurrentWorld);
        WorldManager.SetWorldDifficulty(saveData.CurrentWorld, saveData.CurrentDifficulty);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        FaseGiorno.SetCurrentPhase(saveData.CurrentPhase);

        if (saveData.CompostData != null)
        {
            CompostSystem.LoadFromData(saveData.CompostData);
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
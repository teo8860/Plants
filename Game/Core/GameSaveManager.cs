using System;
using System.Collections.Generic;
using Engine.Tools;

namespace Plants
{

public class GameSaveData
{
    public WorldType CurrentWorld;
    public WorldDifficulty CurrentDifficulty;
    public Weather CurrentWeather;
    public DayPhase CurrentPhase;
    public CompostSaveData CompostData;
    public InventorySaveData InventoryData;
    public DateTime SaveTime;
    public string Version = "1.0.0";
}

public class CompostSaveData
{
    public int CollectedLeaves;
    public List<SeedPackageRarity> AvailablePackages = new();
    public float CompostEfficiency;
}

public static class GameSaveManager
{
    private const string SaveFileName = "savegame.json";
    private static GameSaveData _pendingLoadData;

    public static void SaveGame(WorldType currentWorld, WorldDifficulty currentDifficulty,
        Weather currentWeather, DayPhase currentPhase)
    {
        try
        {
            var compostData = CompostSystem.GetSaveData();
            var inventorySeeds = Inventario.get().GetAllSeeds();

            var saveData = new GameSaveData
            {
                CurrentWorld = currentWorld,
                CurrentDifficulty = currentDifficulty,
                CurrentWeather = currentWeather,
                CurrentPhase = currentPhase,
                CompostData = compostData,
                InventoryData = new InventorySaveData
                {
                    seedsData = inventorySeeds
                },
                SaveTime = DateTime.Now
            };

            SaveHelper.Save(SaveFileName, saveData);
            Console.WriteLine($"Game saved successfully using SaveHelper at {saveData.SaveTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
        }
    }

    public static GameSaveData LoadGame()
    {
        try
        {
            // Usa SaveHelper dalla cartella tools invece di duplicare la logica
            var saveData = SaveHelper.Load<GameSaveData>(SaveFileName);

            if (saveData == null)
            {
                Console.WriteLine("No save file found or failed to load save data, starting new game");
                return null;
            }

            // Store for later restoration
            _pendingLoadData = saveData;

            Console.WriteLine($"Game loaded successfully from save at {saveData.SaveTime}");
            return saveData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game: {ex.Message}");
            return null;
        }
    }

    public static GameSaveData GetPendingLoadData()
    {
        var data = _pendingLoadData;
        _pendingLoadData = null; // Clear after use
        return data;
    }

    public static void RestoreGameState(GameSaveData saveData)
    {
        // Restore game state
        WorldManager.SetCurrentWorld(saveData.CurrentWorld);
        WorldManager.SetWorldDifficulty(saveData.CurrentWorld, saveData.CurrentDifficulty);
        WeatherManager.SetCurrentWeather(saveData.CurrentWeather);
        FaseGiorno.SetCurrentPhase(saveData.CurrentPhase);

        if (saveData.CompostData != null)
        {
            CompostSystem.LoadFromData(saveData.CompostData);
        }

        if (saveData.InventoryData != null)
        {
            Inventario.get().loadFromData(saveData.InventoryData);
        }
    }

    public static bool HasSaveFile() => SaveHelper.Exists(SaveFileName);

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
}
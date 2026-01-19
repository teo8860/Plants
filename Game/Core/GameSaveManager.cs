using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

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
    private static readonly string SaveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Plants",
        "savegame.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private static GameSaveData _pendingLoadData;

    public static void SaveGame(WorldType currentWorld, WorldDifficulty currentDifficulty,
        Weather currentWeather, DayPhase currentPhase)
    {
        try
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var saveData = new GameSaveData
            {
                CurrentWorld = currentWorld,
                CurrentDifficulty = currentDifficulty,
                CurrentWeather = currentWeather,
                CurrentPhase = currentPhase,
                CompostData = CompostSystem.GetSaveData(),
                InventoryData = new InventorySaveData
                {
                    Seeds = Inventario.get().GetAllSeeds()
                },
                SaveTime = DateTime.Now
            };

            string jsonString = JsonSerializer.Serialize(saveData, JsonOptions);
            File.WriteAllText(SaveFilePath, jsonString);

            Console.WriteLine($"Game saved successfully at {saveData.SaveTime}");
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
            if (!File.Exists(SaveFilePath))
            {
                Console.WriteLine("No save file found, starting new game");
                return null;
            }

            string jsonString = File.ReadAllText(SaveFilePath);
            var saveData = JsonSerializer.Deserialize<GameSaveData>(jsonString, JsonOptions);

            if (saveData == null)
            {
                Console.WriteLine("Failed to load save data");
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
            Inventario.get().LoadFromData(saveData.InventoryData);
        }
    }

    public static bool HasSaveFile() => File.Exists(SaveFilePath);

    public static void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Console.WriteLine("Save file deleted");
            }
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
            if (!File.Exists(SaveFilePath)) return null;

            string jsonString = File.ReadAllText(SaveFilePath);
            var saveData = JsonSerializer.Deserialize<GameSaveData>(jsonString, JsonOptions);
            return saveData?.SaveTime;
        }
        catch
        {
            return null;
        }
    }
}
}
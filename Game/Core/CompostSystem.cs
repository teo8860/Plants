using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public enum SeedPackageRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class SeedPackage
{
    public SeedPackageRarity Rarity;
    public int LeavesRequired;
    public List<SeedType> PossibleSeeds;

    public SeedPackage(SeedPackageRarity rarity)
    {
        Rarity = rarity;
        LeavesRequired = rarity switch
        {
            SeedPackageRarity.Common => 10,
            SeedPackageRarity.Uncommon => 25,
            SeedPackageRarity.Rare => 50,
            SeedPackageRarity.Epic => 100,
            SeedPackageRarity.Legendary => 200,
            _ => 10
        };

        // Define possible seeds for each rarity
        PossibleSeeds = rarity switch
        {
            SeedPackageRarity.Common => new List<SeedType> { SeedType.Normale },
            SeedPackageRarity.Uncommon => new List<SeedType> { SeedType.Poderoso, SeedType.Fluviale, SeedType.Florido },
            SeedPackageRarity.Rare => new List<SeedType> { SeedType.Glaciale, SeedType.Magmatico, SeedType.Rapido },
            SeedPackageRarity.Epic => new List<SeedType> { SeedType.Puro, SeedType.Antico },
            SeedPackageRarity.Legendary => new List<SeedType> { SeedType.Cosmico },
            _ => new List<SeedType> { SeedType.Normale }
        };
    }

    public SeedType Open()
    {
        if (PossibleSeeds.Count == 0) return SeedType.Normale;
        return PossibleSeeds[Random.Shared.Next(PossibleSeeds.Count)];
    }
}

public static class CompostSystem
{
    private static int _collectedLeaves = 0;
    private static List<SeedPackage> _availablePackages = new();

    // Climate affects composting efficiency
    private static float _currentCompostEfficiency = 1.0f;

    public static void AddLeaves(int leafCount, Weather currentWeather)
    {
        // Weather affects how many leaves actually compost successfully
        _currentCompostEfficiency = GetWeatherCompostEfficiency(currentWeather);
        int effectiveLeaves = (int)(leafCount * _currentCompostEfficiency);

        _collectedLeaves += effectiveLeaves;

        // Check if we can create new seed packages
        GenerateAvailablePackages();
    }

    public static int GetCollectedLeaves() => _collectedLeaves;

    public static List<SeedPackage> GetAvailablePackages() => new(_availablePackages);

    public static SeedType? OpenPackage(SeedPackage package)
    {
        if (!_availablePackages.Contains(package)) return null;

        _availablePackages.Remove(package);
        return package.Open();
    }

    public static bool CanCreatePackage(SeedPackageRarity rarity)
    {
        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        return _collectedLeaves >= leavesNeeded;
    }

    public static SeedPackage? CreatePackage(SeedPackageRarity rarity)
    {
        if (!CanCreatePackage(rarity)) return null;

        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        _collectedLeaves -= leavesNeeded;

        var package = new SeedPackage(rarity);
        _availablePackages.Add(package);
        return package;
    }

    private static void GenerateAvailablePackages()
    {
        // Auto-generate common packages when enough leaves are collected
        while (CanCreatePackage(SeedPackageRarity.Common))
        {
            CreatePackage(SeedPackageRarity.Common);
        }

        // Random chance to generate higher rarity packages
        if (_collectedLeaves >= 25 && Random.Shared.NextDouble() < 0.3f) // 30% chance
        {
            CreatePackage(SeedPackageRarity.Uncommon);
        }

        if (_collectedLeaves >= 50 && Random.Shared.NextDouble() < 0.1f) // 10% chance
        {
            CreatePackage(SeedPackageRarity.Rare);
        }
    }

    private static float GetWeatherCompostEfficiency(Weather weather) => weather switch
    {
        Weather.Sunny => 1.0f, // Ideal conditions
        Weather.Cloudy => 0.9f,
        Weather.Rainy => 1.2f, // Rain helps decomposition
        Weather.Stormy => 0.7f, // Too much water washes away nutrients
        Weather.Windy => 0.8f, // Wind can dry out compost
        _ => 1.0f
    };

    // Save/Load support
    public static CompostSaveData GetSaveData() => new()
    {
        CollectedLeaves = _collectedLeaves,
        AvailablePackages = _availablePackages.Select(p => p.Rarity).ToList(),
        CompostEfficiency = _currentCompostEfficiency
    };

    public static void LoadFromData(CompostSaveData data)
    {
        _collectedLeaves = data.CollectedLeaves;
        _currentCompostEfficiency = data.CompostEfficiency;

        _availablePackages.Clear();
        foreach (var rarity in data.AvailablePackages)
        {
            _availablePackages.Add(new SeedPackage(rarity));
        }
    }
}


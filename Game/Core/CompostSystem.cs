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
    private static List<SeedPackage> _availablePackages = new();

 
    public static List<SeedPackage> GetAvailablePackages() => new(_availablePackages);

    public static SeedType OpenPackage(SeedPackage package)
    {
        if (!_availablePackages.Contains(package)) 
            return SeedType.Normale;

        _availablePackages.Remove(package);
        return package.Open();
    }

    public static bool CanCreatePackage(SeedPackageRarity rarity)
    {
        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        return Game.pianta.Stats.FoglieAttuali >= leavesNeeded;
    }

    public static SeedPackage? CreatePackage(SeedPackageRarity rarity)
    {
        if (!CanCreatePackage(rarity)) return null;

        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        Game.pianta.Stats.FoglieAttuali -= leavesNeeded;

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
        if (Game.pianta.Stats.FoglieAttuali >= 25 && Random.Shared.NextDouble() < 0.3f) // 30% chance
        {
            CreatePackage(SeedPackageRarity.Uncommon);
        }

        if (Game.pianta.Stats.FoglieAttuali >= 50 && Random.Shared.NextDouble() < 0.1f) // 10% chance
        {
            CreatePackage(SeedPackageRarity.Rare);
        }
    }

}


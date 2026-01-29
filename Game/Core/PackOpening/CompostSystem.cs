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

public class PackageInProgress
{
    public SeedPackageRarity Rarity;
    public float TimeRequired;
    public float TimeElapsed;
    public bool IsComplete => TimeElapsed >= TimeRequired;
    public float Progress => Math.Clamp(TimeElapsed / TimeRequired, 0f, 1f);

    public PackageInProgress(SeedPackageRarity rarity)
    {
        Rarity = rarity;
        TimeElapsed = 0f;

        // Tempi in secondi basati sulla rarità
        TimeRequired = rarity switch
        {
            SeedPackageRarity.Common => 1f,      // 5 secondi
            SeedPackageRarity.Uncommon => 1f,   // 10 secondi
            SeedPackageRarity.Rare => 1f,       // 20 secondi
            SeedPackageRarity.Epic => 1f,       // 40 secondi
            SeedPackageRarity.Legendary => 1f,  // 60 secondi
            _ => 5f
        };
    }

    public void Update(float deltaTime)
    {
        if (!IsComplete)
        {
            TimeElapsed += deltaTime;
        }
    }
}

public static class CompostSystem
{
    private static List<SeedPackage> _availablePackages = new();
    private static List<PackageInProgress> _packagesInProgress = new();
    private static OpeningSystem _openingSystem = new(); 

    private const int MAX_PACKAGES = 4;

    public static List<SeedPackage> GetAvailablePackages() => new(_availablePackages);
    public static List<PackageInProgress> GetPackagesInProgress() => new(_packagesInProgress);
    public static int GetTotalPackageCount() => _availablePackages.Count + _packagesInProgress.Count;

    public static void Update(float deltaTime)
    {
        for (int i = _packagesInProgress.Count - 1; i >= 0; i--)
        {
            _packagesInProgress[i].Update(deltaTime);

            if (_packagesInProgress[i].IsComplete)
            {
                var completed = _packagesInProgress[i];
                _packagesInProgress.RemoveAt(i);
                _availablePackages.Add(new SeedPackage(completed.Rarity));
                Console.WriteLine($"Pacchetto {completed.Rarity} pronto per l'apertura!");
            }
        }
    }

    public static Seed OpenPackage(SeedPackage package)
    {
        if (!_availablePackages.Contains(package))
            return new Seed(SeedType.Normale);

        _availablePackages.Remove(package);

        Seed resultSeed = _openingSystem.RollSeedFromPackage(package.Rarity);

        Console.WriteLine($"Aperto pacchetto {package.Rarity}: Trovato {resultSeed.name} ({resultSeed.rarity})");
        return resultSeed;
    }

    public static bool CanCreatePackage(SeedPackageRarity rarity)
    {
        if (GetTotalPackageCount() >= MAX_PACKAGES)
            return false;

        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        return Game.pianta.Stats.FoglieAttuali >= leavesNeeded;
    }

    public static bool StartPackageCreation(SeedPackageRarity rarity)
    {
        if (!CanCreatePackage(rarity))
            return false;

        int leavesNeeded = new SeedPackage(rarity).LeavesRequired;
        Game.pianta.Stats.FoglieAttuali -= leavesNeeded;

        var packageInProgress = new PackageInProgress(rarity);
        _packagesInProgress.Add(packageInProgress);
        return true;
    }
}
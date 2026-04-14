using System;

namespace Plants;

public static class StarterSeedSystem
{
    public const int STARTER_SEED_ID = 1;
    public const int STARTER_SEED_ESSENCE_VALUE = 1;

    private const string StarterFlagFile = "starter.json";

    private class StarterFlag { public bool granted { get; set; } }

    public static Seed CreateStarterSeed()
    {
        Seed seed = new Seed(SeedType.Normale);
        seed.id = STARTER_SEED_ID;
        seed.stats = new SeedStats();
        seed.name = "Seme Iniziale";
        return seed;
    }

    private static bool HasBeenGranted()
    {
        var flag = SaveHelper.Load<StarterFlag>(StarterFlagFile);
        return flag != null && flag.granted;
    }

    private static void MarkGranted()
    {
        SaveHelper.Save(StarterFlagFile, new StarterFlag { granted = true });
    }

    public static bool ShouldGrantStarter()
    {
        if (HasBeenGranted())
            return false;

        if (Inventario.get().seeds.Count > 0)
            return false;

        if (CompostSystem.GetTotalPackageCount() > 0)
            return false;

        if (CompostSystem.CanCreatePackage(SeedPackageRarity.Common))
            return false;

        return true;
    }

    public static bool GrantIfNeeded()
    {
        if (!ShouldGrantStarter())
            return false;

        Inventario.get().AddSeed(CreateStarterSeed());
        MarkGranted();
        return true;
    }
}

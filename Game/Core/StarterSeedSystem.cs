using System;

namespace Plants;

public static class StarterSeedSystem
{
    public const int STARTER_SEED_ID = 1;
    public const int STARTER_SEED_ESSENCE_VALUE = 1;

    public static Seed CreateStarterSeed()
    {
        Seed seed = new Seed(SeedType.Normale);
        seed.id = STARTER_SEED_ID;
        seed.stats = new SeedStats();
        seed.name = "Seme Iniziale";
        return seed;
    }

    public static bool ShouldGrantStarter()
    {
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
        return true;
    }
}

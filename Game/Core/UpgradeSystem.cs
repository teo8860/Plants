using System;

namespace Plants;

public enum UpgradeType
{
    Innaffiatoio,
    Inventario,
    SpazioPacchetti
}

public static class UpgradeSystem
{
    public const int MaxLevel = 5;
    private static int[] levels = new int[3]; // uno per UpgradeType

    private static readonly int[] Costs = { 50, 150, 400, 800, 1500 };

    private static readonly string[] Names = { "Innaffiatoio", "Inventario", "Spazio Pacchetti" };

    public static int GetLevel(UpgradeType type) => levels[(int)type];

    public static void SetLevel(UpgradeType type, int level)
    {
        levels[(int)type] = Math.Clamp(level, 0, MaxLevel);
    }

    public static string GetName(UpgradeType type) => Names[(int)type];

    public static int GetCost(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level >= MaxLevel) return -1;
        return Costs[level];
    }

    public static bool CanUpgrade(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level >= MaxLevel) return false;
        int cost = Costs[level];
        return Game.pianta.Stats.FoglieAccumulate >= cost;
    }

    public static bool TryUpgrade(UpgradeType type)
    {
        if (!CanUpgrade(type)) return false;

        int cost = Costs[GetLevel(type)];
        Game.pianta.Stats.FoglieAccumulate -= cost;
        levels[(int)type]++;

        if (type == UpgradeType.Innaffiatoio)
        {
            WaterSystem.Max = GetWaterMax();
            if (WaterSystem.Current > WaterSystem.Max)
                WaterSystem.Current = WaterSystem.Max;
        }

        return true;
    }

    public static float GetWaterMax()
    {
        return 100f + GetLevel(UpgradeType.Innaffiatoio) * 50f;
    }

    public static int GetMaxSeeds()
    {
        return 50 + GetLevel(UpgradeType.Inventario) * 25;
    }

    public static int GetMaxPackages()
    {
        return 2 + GetLevel(UpgradeType.SpazioPacchetti);
    }
}

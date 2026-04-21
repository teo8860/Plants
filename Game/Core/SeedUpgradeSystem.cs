using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public static class SeedUpgradeSystem
{

    // Essenza disponibile per i miglioramenti
    public static int Essence { get; private set; } = 0;


    // Costi base per miglioramenti (scala con livello attuale)
    private const int BASE_UPGRADE_COST = 50;
    private const float COST_MULTIPLIER = 1.5f;

    // Limiti di miglioramento basati sulla rarità
    private static readonly Dictionary<SeedRarity, int> MaxUpgradeLevels = new()
    {
        { SeedRarity.Comune, 5 },
        { SeedRarity.NonComune, 7 },
        { SeedRarity.Raro, 9 },
        { SeedRarity.Epico, 11 },
        { SeedRarity.Leggendario, 13 }
    };

    // Range di essenza base per rarità (min-max)
    private static readonly Dictionary<SeedRarity, (int min, int max)> SacrificeRanges = new()
    {
        { SeedRarity.Comune, (8, 12) },
        { SeedRarity.NonComune, (20, 30) },
        { SeedRarity.Raro, (50, 70) },
        { SeedRarity.Epico, (130, 170) },
        { SeedRarity.Leggendario, (350, 450) }
    };

    private static readonly Dictionary<int, float> BonusPerTier = new()
{
    { 0, 10f },  
    { 1, 8f },  
    { 2, 7f },  
    { 3, 6f },  
    { 4, 5f }   
};

    // Bonus per stat per livello
    private const float STAT_BONUS_PER_LEVEL = 5f; // +5% per livello

    public static int SacrificeSeed(Seed seed)
    {
        if (seed == null) return 0;

        int essenceGained;

        if (seed.id == StarterSeedSystem.STARTER_SEED_ID)
        {
            essenceGained = StarterSeedSystem.STARTER_SEED_ESSENCE_VALUE;
        }
        else
        {
            essenceGained = CalculateSeedEssenceValue(seed);

            // Bonus essenza basato sui livelli di upgrade del seme
            if (seed.upgradeLevel > 0)
            {
                int totalLevels = seed.upgradeLevel;
                essenceGained += totalLevels * 25; // +25 essenza per livello
            }
        }

        SetEssence(Essence+essenceGained);

        // Rimuovi dall'inventario
        Inventario.get().RemoveSeed(seed);
        Inventario.get().Save();

		return essenceGained;
    }

    private static int CalculateSeedEssenceValue(Seed seed)
    {
        if (!SacrificeRanges.TryGetValue(seed.rarity, out var range))
        {
            range = (8, 12); // Default per rarità sconosciute
        }

        // Calcola un punteggio di qualità basato sulle statistiche del seme
        float qualityScore = CalculateSeedQuality(seed);

        int essenceValue = range.min + (int)((range.max - range.min) * qualityScore);

        return essenceValue;
    }

    private static float CalculateSeedQuality(Seed seed)
    {
        var stats = seed.stats;

        // Scala 0-99: primarie baseline 10, range tipico 5-25 (fusioni possono portare piu' in alto).
        // Score normalizzato sulla porzione di barra oltre il baseline.
        float vitalitaScore = Math.Clamp((stats.vitalita - SeedStatScaling.PrimaryNeutralReference) / 15f, 0f, 1f);
        float vegetazioneScore = Math.Clamp((stats.vegetazione - SeedStatScaling.PrimaryNeutralReference) / 15f, 0f, 1f);
        float metabolismoScore = Math.Clamp((stats.metabolismo - SeedStatScaling.PrimaryNeutralReference) / 15f, 0f, 1f);

        // Idratazione: piu' basso = meglio (meno consumo acqua).
        float idratazioneScore = Math.Clamp(1f - ((stats.idratazione - 3f) / 22f), 0f, 1f);

        // Resistenze: 0 = neutro, 99 = max. Score lineare.
        float resistenzaFreddoScore = Math.Clamp(stats.resistenzaFreddo / SeedStatScaling.StatMax, 0f, 1f);
        float resistenzaCaldoScore = Math.Clamp(stats.resistenzaCaldo / SeedStatScaling.StatMax, 0f, 1f);
        float resistenzaParassitiScore = Math.Clamp(stats.resistenzaParassiti / SeedStatScaling.StatMax, 0f, 1f);
        float resistenzaVuotoScore = Math.Clamp(stats.resistenzaVuoto / SeedStatScaling.StatMax, 0f, 1f);

        // Peso diverso per ogni statistica (totale = 1.0)
        float weightedScore =
            vitalitaScore * 0.20f +           // 20% - Salute è importante
            idratazioneScore * 0.15f +        // 15% - Efficienza acqua
            vegetazioneScore * 0.15f +        // 15% - Foglie
            metabolismoScore * 0.15f +        // 15% - Velocità crescita
            resistenzaFreddoScore * 0.10f +   // 10% - Resistenza freddo
            resistenzaCaldoScore * 0.10f +    // 10% - Resistenza caldo
            resistenzaParassitiScore * 0.10f + // 10% - Resistenza parassiti
            resistenzaVuotoScore * 0.05f;     // 5% - Resistenza vuoto

        // Aggiungi un piccolo bonus basato sul numero di fusioni
        float fusionBonus = Math.Clamp(stats.fusionCount * 0.05f, 0f, 0.15f); // Max +15%

        float totalScore = Math.Clamp(weightedScore + fusionBonus, 0f, 1f);

        return totalScore;
    }

    public static int PreviewSacrificeValue(Seed seed)
    {
        if (seed == null) return 0;

        if (seed.id == StarterSeedSystem.STARTER_SEED_ID)
            return StarterSeedSystem.STARTER_SEED_ESSENCE_VALUE;

        int baseValue = CalculateSeedEssenceValue(seed);

        // Aggiungi bonus da livelli di upgrade se presenti
        if (seed.upgradeLevel > 0)
        {
            int totalLevels = seed.upgradeLevel;
            baseValue += totalLevels * 5;
        }

        return baseValue;
    }

    public static int GetUpgradeCost(Seed seed)
    {
        if (seed.upgradeLevel == 0)
        {
            // Primo upgrade
            return BASE_UPGRADE_COST;
        }

        int currentLevel = seed.upgradeLevel;
        return (int)(BASE_UPGRADE_COST * Math.Pow(COST_MULTIPLIER, currentLevel));
    }

    public static bool CanUpgrade(Seed seed)
    {
        if (seed == null) return false;

        int currentLevel = seed.upgradeLevel;
        int maxLevel = GetMaxUpgradeLevel(seed);
        int cost = GetUpgradeCost(seed);

        return currentLevel < maxLevel && Essence >= cost;
    }

    public static bool UpgradeStat(Seed seed, SeedStatType stats)
    {
        if (!CanUpgrade(seed))
            return false;

        int cost = GetUpgradeCost(seed);
        SetEssence(Essence-cost);

        
        seed.upgradeLevel += 1;
        seed.upgradedStats.Add(stats);

        ApplyStatBonus(seed, stats);

        return true;
    }

    public static void ApplyStatBonus(Seed seed, SeedStatType stat)
    {
        int level = GetStatLevel(seed, stat);
        float percentage = (STAT_BONUS_PER_LEVEL + GetBonusForLevel(level)) / 100;
        float multiplier = level * percentage;
   
		switch (stat)
        {
            case SeedStatType.Vitalita:
                seed.stats.vitalita = SeedStatScaling.ClampPrimary(seed.stats.vitalita + seed.stats.vitalita * multiplier, SeedStatScaling.VitalitaMin);
                break;

            case SeedStatType.Idratazione:
                seed.stats.idratazione = SeedStatScaling.ClampPrimary(seed.stats.idratazione + seed.stats.idratazione * multiplier, SeedStatScaling.IdratazioneMin);
                break;

            case SeedStatType.ResistenzaParassiti:
                seed.stats.resistenzaParassiti = SeedStatScaling.ClampResistance(seed.stats.resistenzaParassiti + seed.stats.resistenzaParassiti * multiplier);
                break;

            case SeedStatType.Vegetazione:
                seed.stats.vegetazione = SeedStatScaling.ClampPrimary(seed.stats.vegetazione + seed.stats.vegetazione * multiplier, SeedStatScaling.VegetazioneMin);
                break;

            case SeedStatType.Metabolismo:
                seed.stats.metabolismo = SeedStatScaling.ClampPrimary(seed.stats.metabolismo + seed.stats.metabolismo * multiplier, SeedStatScaling.MetabolismoMin);
                break;
        }
    }

    private static int GetCurrentTier(int level)
    {
        if (level < 5) return 0;
        if (level < 7) return 1;
        if (level < 9) return 2;
        if (level < 11) return 3;
        return 4;
    }

    private static float GetBonusForLevel(int level)
    {
        int tier = GetCurrentTier(level);
        return BonusPerTier.GetValueOrDefault(tier, 0.03f);
    }


    public static int GetMaxUpgradeLevel(Seed seed)
    {
        return MaxUpgradeLevels.GetValueOrDefault(seed.rarity, 5);
    }

    public static int GetStatLevel(Seed seed, SeedStatType stats)
    {
        if (seed.upgradedStats.Count(x => x == stats) == 0)
        {
            return 0;
        }

        return seed.upgradedStats.Count(x => x == stats);
    }

	internal static void SetEssence(int essence)
	{
		SeedUpgradeSystem.Essence = essence;
        GameSave.get().data.essence = essence;
	}
}


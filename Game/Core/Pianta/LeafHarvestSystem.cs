using System;
using System.Collections.Generic;

namespace Plants;

/// <summary>
/// Rappresenta il risultato della raccolta di una singola foglia
/// </summary>
public class LeafResult
{
    public bool IsIntact { get; set; }       // foglia integra (raccoglibile)
    public float Quality { get; set; }        // qualità 0-1
    public int EssenceValue { get; set; }     // essence che vale
    public string DamageReason { get; set; } // motivo del danno se rotta
}

/// <summary>
/// Risultato complessivo della raccolta
/// </summary>
public class HarvestResult
{
    public int TotalLeaves { get; set; }
    public int IntactLeaves { get; set; }
    public int BrokenLeaves { get; set; }
    public int EssenceGained { get; set; }
    public List<LeafResult> Leaves { get; set; } = new();
    public string TriggerReason { get; set; } // "Cambio Mondo" o "Pianta morta"
}

/// <summary>
/// Sistema per raccogliere le foglie quando la pianta muore o si cambia mondo
/// </summary>
public static class LeafHarvestSystem
{
    // Essence base per foglia integra (scala con rarità del seme)
    private const int BASE_ESSENCE_PER_LEAF = 3;
    private const int MAX_ESSENCE_PER_LEAF = 15;

    /// <summary>
    /// Calcola la probabilità che una foglia sia integra in base alle condizioni attuali
    /// </summary>
    public static float CalculateLeafIntactChance(PlantStats stats, WorldModifier worldMod, Weather weather, DayPhase phase)
    {
        float chance = 0.85f; // base 85%

        // --- PARASSITI ---
        if (stats.Infestata)
        {
            // i parassiti rovinano le foglie pesantemente
            chance -= stats.IntensitaInfestazione * 0.5f;
        }

        // --- METEO ---
        if (worldMod.IsMeteoOn)
        {
            switch (weather)
            {
                case Weather.Stormy:
                    chance -= 0.35f; // tempesta strappa e rompe le foglie
                    break;
                case Weather.Snowy:
                    chance -= 0.20f; // neve le congela e le rompe
                    break;
                case Weather.Rainy:
                    chance -= 0.05f; // pioggia leggero impatto
                    break;
                case Weather.Foggy:
                    chance -= 0.08f; // umidità rovina le foglie
                    break;
                case Weather.Sunny:
                    chance += 0.05f; // sole ideale
                    break;
                case Weather.Cloudy:
                    // neutro
                    break;
            }
        }

        // --- TEMPERATURA ---
        float temp = stats.Temperatura;
        if (temp <= GameLogicPianta.TEMPERATURA_GELIDA)
        {
            chance -= 0.40f; // gelo critico, foglie fragili e rotte
        }
        else if (temp < GameLogicPianta.TEMPERATURA_FREDDA)
        {
            chance -= 0.20f;
        }
        else if (temp >= GameLogicPianta.TEMPERATURA_TORRIDA)
        {
            chance -= 0.35f; // calore estremo le brucia
        }
        else if (temp > GameLogicPianta.TEMPERATURA_CALDA)
        {
            chance -= 0.15f;
        }
        else if (temp >= GameLogicPianta.TEMPERATURA_IDEALE_MIN && temp <= GameLogicPianta.TEMPERATURA_IDEALE_MAX)
        {
            chance += 0.05f; // temperatura perfetta
        }

        // --- IDRATAZIONE ---
        if (stats.Idratazione < 0.2f)
        {
            chance -= 0.25f; // disidratazione grave rende le foglie fragili
        }
        else if (stats.Idratazione < 0.4f)
        {
            chance -= 0.10f;
        }
        else if (stats.Idratazione > 0.7f)
        {
            chance += 0.03f;
        }

        // --- SALUTE GENERALE ---
        if (stats.Salute < 0.2f)
        {
            chance -= 0.20f; // pianta quasi morta, foglie pessime
        }
        else if (stats.Salute < 0.5f)
        {
            chance -= 0.10f;
        }

        // --- OSSIGENO ---
        if (stats.Ossigeno < 0.2f)
        {
            chance -= 0.10f;
        }

        // --- MODIFICATORI MONDO ---
        // Alcuni mondi rendono più difficile avere foglie integre
        chance -= (worldMod.LeafDropRate - 1f) * 0.15f;
        chance -= (worldMod.ParasiteDamage - 1f) * 0.05f;

        return Math.Clamp(chance, 0.05f, 0.95f); // minimo 5%, massimo 95%
    }

    /// <summary>
    /// Calcola la ragione principale del danno per una foglia rotta (per UI)
    /// </summary>
    private static string GetDamageReason(PlantStats stats, WorldModifier worldMod, Weather weather)
    {
        // Trova la ragione più grave
        if (stats.Infestata && stats.IntensitaInfestazione > 0.5f)
            return "Parassiti";

        if (worldMod.IsMeteoOn && weather == Weather.Stormy)
            return "Tempesta";

        float temp = stats.Temperatura;
        if (temp <= GameLogicPianta.TEMPERATURA_GELIDA)
            return "Gelo";
        if (temp >= GameLogicPianta.TEMPERATURA_TORRIDA)
            return "Calore estremo";

        if (stats.Idratazione < 0.2f)
            return "Disidratazione";

        if (stats.Salute < 0.2f)
            return "Pianta malata";

        if (worldMod.IsMeteoOn && weather == Weather.Snowy)
            return "Neve";

        return "Condizioni avverse";
    }

    /// <summary>
    /// Calcola il valore in essence di una foglia integra
    /// </summary>
    private static int CalculateLeafEssenceValue(float quality, SeedType seedType)
    {
        // Rarità del seme influenza il valore base
        SeedRarity rarity = SeedDataType.GetRarity(seedType);
        float rarityMultiplier = rarity switch
        {
            SeedRarity.Comune => 1.0f,
            SeedRarity.NonComune => 1.3f,
            SeedRarity.Raro => 1.7f,
            SeedRarity.Epico => 2.5f,
            SeedRarity.Leggendario => 4.0f,
            _ => 1.0f
        };

        float essenceF = BASE_ESSENCE_PER_LEAF + (MAX_ESSENCE_PER_LEAF - BASE_ESSENCE_PER_LEAF) * quality;
        essenceF *= rarityMultiplier;

        return Math.Max(1, (int)essenceF);
    }

    /// <summary>
    /// Esegue la raccolta di tutte le foglie della pianta.
    /// Chiamato quando si cambia mondo o la pianta muore.
    /// </summary>
    public static HarvestResult Harvest(string triggerReason = "Cambio Mondo")
    {
        var pianta = Game.pianta;
        if (pianta == null)
            return new HarvestResult { TriggerReason = triggerReason };

        var stats = pianta.Stats;
        var worldMod = WorldManager.GetCurrentModifiers();
        var weather = WeatherManager.GetCurrentWeather();
        var phase = Game.Phase;

        int totalLeaves = stats.FoglieAttuali;
        var result = new HarvestResult
        {
            TotalLeaves = totalLeaves,
            TriggerReason = triggerReason
        };

        if (totalLeaves <= 0)
            return result;

        float intactChance = CalculateLeafIntactChance(stats, worldMod, weather, phase);
        string damageReason = GetDamageReason(stats, worldMod, weather);

        for (int i = 0; i < totalLeaves; i++)
        {
            bool isIntact = RandomHelper.Chance(intactChance);

            // qualità influenzata dall'integrità e da un po' di random
            float quality = isIntact
                ? RandomHelper.Float(0.5f, 1.0f) * intactChance
                : 0f;

            quality = Math.Clamp(quality, 0f, 1f);

            int essence = isIntact ? CalculateLeafEssenceValue(quality, pianta.TipoSeme) : 0;

            var leafResult = new LeafResult
            {
                IsIntact = isIntact,
                Quality = quality,
                EssenceValue = essence,
                DamageReason = isIntact ? "" : damageReason
            };

            result.Leaves.Add(leafResult);

            if (isIntact)
            {
                result.IntactLeaves++;
                result.EssenceGained += essence;
            }
            else
            {
                result.BrokenLeaves++;
            }
        }

        // Applica l'essence guadagnata
        if (result.EssenceGained > 0)
        {
            SeedUpgradeSystem.SetEssence(SeedUpgradeSystem.Essence + result.EssenceGained);
        }

        return result;
    }

    /// <summary>
    /// Scorciatoia: raccoglie e mostra il popup
    /// </summary>
    public static void HarvestAndShow(string triggerReason = "Cambio Mondo")
    {
        if (Game.pianta == null || Game.pianta.Stats.FoglieAttuali <= 0)
            return;

        var result = Harvest(triggerReason);

        if (Game.leafHarvestPopup != null)
        {
            Game.leafHarvestPopup.Show(result);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Plants;

public class SeedFusionManager
{
    private static SeedFusionManager instance = null;

    public Seed SelectedSeed1 { get; private set; } = null;
    public Seed SelectedSeed2 { get; private set; } = null;
    public int SelectedIndex1 { get; private set; } = -1;
    public int SelectedIndex2 { get; private set; } = -1;

    public bool IsFusionMode { get; private set; } = false;
    public bool CanFuse => SelectedSeed1 != null && SelectedSeed2 != null &&
                          SelectedSeed1.CanBeFused && SelectedSeed2.CanBeFused;

    public static SeedFusionManager Get()
    {
        if (instance == null)
            instance = new SeedFusionManager();
        return instance;
    }

    private SeedFusionManager() { }

    public void StartFusionMode()
    {
        IsFusionMode = true;
        ClearSelection();
    }

    public void StopFusionMode()
    {
        IsFusionMode = false;
        ClearSelection();
    }

    public void ToggleSeedSelection(Seed seed, int index)
    {
        if (!IsFusionMode) return;

        // Verifica se il seme può essere fuso
        if (!seed.CanBeFused)
        {
            Console.WriteLine($"Questo seme ha raggiunto il limite di fusioni ({Seed.MAX_FUSIONS})!");
            return;
        }

        // Se è già selezionato, deseleziona
        if (SelectedIndex1 == index)
        {
            SelectedSeed1 = null;
            SelectedIndex1 = -1;
            return;
        }

        if (SelectedIndex2 == index)
        {
            SelectedSeed2 = null;
            SelectedIndex2 = -1;
            return;
        }

        // Seleziona in uno slot libero
        if (SelectedSeed1 == null)
        {
            SelectedSeed1 = seed;
            SelectedIndex1 = index;
        }
        else if (SelectedSeed2 == null)
        {
            SelectedSeed2 = seed;
            SelectedIndex2 = index;
        }
        else
        {
            // Entrambi gli slot sono occupati, sostituisci il primo
            SelectedSeed1 = seed;
            SelectedIndex1 = index;
        }
    }

    public bool IsSeedSelected(int index)
    {
        return index == SelectedIndex1 || index == SelectedIndex2;
    }

    public Seed PerformFusion()
    {
        if (!CanFuse)
        {
            Console.WriteLine("Impossibile fondere: uno o entrambi i semi hanno raggiunto il limite di fusioni!");
            return null;
        }

        // Crea il nuovo seme fuso
        Seed fusedSeed = new Seed(SelectedSeed1, SelectedSeed2);

        // Debug: mostra informazioni sulla fusione
        int maxFusionCount = Math.Max(SelectedSeed1.stats.fusionCount, SelectedSeed2.stats.fusionCount);
        Console.WriteLine($"Fusione completata!");
        Console.WriteLine($"Seme 1: {SelectedSeed1.rarity} (Fusioni: {SelectedSeed1.stats.fusionCount}/{Seed.MAX_FUSIONS})");
        Console.WriteLine($"Seme 2: {SelectedSeed2.rarity} (Fusioni: {SelectedSeed2.stats.fusionCount}/{Seed.MAX_FUSIONS})");
        Console.WriteLine($"Risultato: {fusedSeed.rarity} (Fusioni: {fusedSeed.stats.fusionCount}/{Seed.MAX_FUSIONS})");

        // Rimuovi i semi originali dall'inventario
        var inventory = Inventario.get();
        inventory.RemoveSeed(SelectedSeed1);
        inventory.RemoveSeed(SelectedSeed2);

        // Aggiungi il nuovo seme
        inventory.AddSeed(fusedSeed);

        // Reset della selezione
        ClearSelection();
        StopFusionMode();

        return fusedSeed;
    }

    public void ClearSelection()
    {
        SelectedSeed1 = null;
        SelectedSeed2 = null;
        SelectedIndex1 = -1;
        SelectedIndex2 = -1;
    }

    public string GetFusionPreview()
    {
        if (!CanFuse)
        {
            if (SelectedSeed1 != null && !SelectedSeed1.CanBeFused)
                return "Seme 1: Max fusioni raggiunto!";
            if (SelectedSeed2 != null && !SelectedSeed2.CanBeFused)
                return "Seme 2: Max fusioni raggiunto!";
            return "";
        }

        // Anteprima della rarità risultante
        int rarity1 = (int)SelectedSeed1.rarity;
        int rarity2 = (int)SelectedSeed2.rarity;
        int avgRarity = (rarity1 + rarity2) / 2;

        SeedRarity resultRarity = (SeedRarity)Math.Clamp(avgRarity, 0, (int)SeedRarity.Leggendario);

        int resultFusionCount = Math.Max(SelectedSeed1.stats.fusionCount, SelectedSeed2.stats.fusionCount) + 1;

        return $"Fusione: {GetRarityName(SelectedSeed1.rarity)} + {GetRarityName(SelectedSeed2.rarity)}\n" +
               $"Risultato: ~{GetRarityName(resultRarity)}\n" +
               $"Fusioni risultanti: {resultFusionCount}/{Seed.MAX_FUSIONS}";
    }

    private string GetRarityName(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune => "Comune",
        SeedRarity.NonComune => "Non Comune",
        SeedRarity.Raro => "Raro",
        SeedRarity.Epico => "Epico",
        SeedRarity.Leggendario => "Leggendario",
        _ => "???"
    };
}
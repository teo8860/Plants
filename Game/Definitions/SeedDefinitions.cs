using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_CSharp.Colors;

namespace Plants;

/// <summary>
/// Dati di definizione per semi: tipi, rarità, bonus statistiche,
/// colori, nomi, descrizioni. Tutta la "tabella dei dati" dei semi
/// in un unico posto, senza logica di gioco.
/// </summary>

// ─── Enums ───────────────────────────────────────────────────

public enum SeedType
{
    Normale,
    Poderoso,
    Fluviale,
    Glaciale,
    Magmatico,
    Puro,
    Florido,
    Rapido,
    Antico,
    Cosmico
}

public enum SeedRarity
{
    Comune = 1,
    NonComune = 2,
    Raro = 3,
    Esotico = 100,
    Epico = 4,
    Leggendario = 5,
    Mitico = 101
}

public enum SeedStatType
{
    Vitalita,
    Idratazione,
    ResistenzaFreddo,
    ResistenzaCaldo,
    ResistenzaParassiti,
    Vegetazione,
    Metabolismo,
    ResistenzaVuoto
}

// ─── Definizioni ─────────────────────────────────────────────

public static class SeedDefinitions
{
    // ╔════════════════════════════════════════════════════════╗
    // ║  RARITÀ                                               ║
    // ╚════════════════════════════════════════════════════════╝

    /// <summary>
    /// Ordinamento canonico delle rarità (dal meno raro al più raro).
    /// I valori int dell'enum NON sono sequenziali (Esotico=100, Mitico=101),
    /// quindi questo array è la fonte di verità.
    /// </summary>
    public static readonly SeedRarity[] RarityOrder = new[]
    {
        SeedRarity.Comune,       // rank 0
        SeedRarity.NonComune,    // rank 1
        SeedRarity.Raro,         // rank 2
        SeedRarity.Esotico,      // rank 3
        SeedRarity.Epico,        // rank 4
        SeedRarity.Leggendario,  // rank 5
        SeedRarity.Mitico,       // rank 6
    };

    /// <summary>Rank massimo raggiungibile tramite fusione (Mitico escluso).</summary>
    public const int MAX_BREEDING_RANK = 5; // Leggendario

    public static int GetRarityRank(SeedRarity rarity)
    {
        for (int i = 0; i < RarityOrder.Length; i++)
            if (RarityOrder[i] == rarity) return i;
        return 0;
    }

    public static SeedRarity GetRarityAtRank(int rank)
    {
        rank = Math.Clamp(rank, 0, RarityOrder.Length - 1);
        return RarityOrder[rank];
    }

    public static string GetRarityName(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune      => "Comune",
        SeedRarity.NonComune   => "Non Comune",
        SeedRarity.Raro        => "Raro",
        SeedRarity.Esotico     => "Esotico",
        SeedRarity.Epico       => "Epico",
        SeedRarity.Leggendario => "Leggendario",
        SeedRarity.Mitico      => "Mitico",
        _                      => "???"
    };

    public static Color GetRarityColor(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune      => new Color(200, 200, 200, 255),
        SeedRarity.NonComune   => new Color(80,  200, 80,  255),
        SeedRarity.Raro        => new Color(80,  150, 255, 255),
        SeedRarity.Esotico     => new Color(0,   220, 200, 255),
        SeedRarity.Epico       => new Color(180, 80,  255, 255),
        SeedRarity.Leggendario => new Color(255, 180, 50,  255),
        SeedRarity.Mitico      => new Color(255, 80,  150, 255),
        _                      => Color.White
    };

    /// <summary>Moltiplicatore stats base per rarità (applicato alla generazione).</summary>
    public static float GetRarityMultiplier(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune      => 1.0f,
        SeedRarity.NonComune   => 1.1f,
        SeedRarity.Raro        => 1.25f,
        SeedRarity.Epico       => 1.5f,
        SeedRarity.Leggendario => 2.0f,
        _                      => 1.0f
    };

    // ╔════════════════════════════════════════════════════════╗
    // ║  TIPO SEME → dati fissi                               ║
    // ╚════════════════════════════════════════════════════════╝

    /// <summary>Rarità base associata ad ogni tipo di seme.</summary>
    public static SeedRarity GetRarityForType(SeedType type) => type switch
    {
        SeedType.Normale   => SeedRarity.Comune,
        SeedType.Poderoso  => SeedRarity.NonComune,
        SeedType.Fluviale  => SeedRarity.NonComune,
        SeedType.Glaciale  => SeedRarity.Raro,
        SeedType.Magmatico => SeedRarity.Raro,
        SeedType.Puro      => SeedRarity.Epico,
        SeedType.Florido   => SeedRarity.NonComune,
        SeedType.Rapido    => SeedRarity.Raro,
        SeedType.Antico    => SeedRarity.Epico,
        SeedType.Cosmico   => SeedRarity.Leggendario,
        _                  => SeedRarity.Comune
    };

    public static string GetSeedName(SeedType type) => type switch
    {
        SeedType.Normale   => "Seme Normale",
        SeedType.Poderoso  => "Seme Poderoso",
        SeedType.Fluviale  => "Seme Fluviale",
        SeedType.Glaciale  => "Seme Glaciale",
        SeedType.Magmatico => "Seme Magmatico",
        SeedType.Puro      => "Seme Puro",
        SeedType.Florido   => "Seme Florido",
        SeedType.Rapido    => "Seme Rapido",
        SeedType.Antico    => "Seme Antico",
        SeedType.Cosmico   => "Seme Cosmico",
        _                  => "Seme Sconosciuto"
    };

    public static string GetSeedDescription(SeedType type) => type switch
    {
        SeedType.Normale   => "Un seme comune senza particolari proprietà.",
        SeedType.Poderoso  => "Piante robuste ma lente. +50% vita.",
        SeedType.Fluviale  => "Consuma metà acqua. Soffre il caldo.",
        SeedType.Glaciale  => "Resiste al gelo. Vulnerabile al caldo.",
        SeedType.Magmatico => "Prospera nel caldo. Assetato.",
        SeedType.Puro      => "Immune ai parassiti. Fragile.",
        SeedType.Florido   => "Fogliame rigoglioso. Richiede cure.",
        SeedType.Rapido    => "Crescita veloce. Molto fragile.",
        SeedType.Antico    => "Equilibrato. Bonus a tutto.",
        SeedType.Cosmico   => "Adatto allo spazio. Lento sulla Terra.",
        _                  => "???"
    };

    /// <summary>Colore visuale del seme (usato dallo shader Obj_Seed).</summary>
    public static Vector3 GetSeedColor(SeedType type) => type switch
    {
        SeedType.Normale   => new Vector3(0.6f, 0.6f, 0.6f),
        SeedType.Poderoso  => new Vector3(0.9f, 0.2f, 0.2f),
        SeedType.Fluviale  => new Vector3(0.2f, 0.5f, 0.9f),
        SeedType.Florido   => new Vector3(0.2f, 0.8f, 0.3f),
        SeedType.Glaciale  => new Vector3(0.7f, 0.9f, 1.0f),
        SeedType.Magmatico => new Vector3(1.0f, 0.4f, 0.0f),
        SeedType.Rapido    => new Vector3(1.0f, 1.0f, 0.2f),
        SeedType.Puro      => new Vector3(1.0f, 1.0f, 1.0f),
        SeedType.Antico    => new Vector3(0.5f, 0.4f, 0.2f),
        SeedType.Cosmico   => new Vector3(0.6f, 0.2f, 0.8f),
        _                  => new Vector3(0.6f, 0.6f, 0.6f)
    };

    // ╔════════════════════════════════════════════════════════╗
    // ║  BONUS STATISTICHE per tipo (valori fissi)            ║
    // ╚════════════════════════════════════════════════════════╝

    /// <summary>
    /// Bonus fissi applicati quando un seme viene piantato (seedBonus).
    /// Rappresentano l'identità meccanica del tipo. Usati come moltiplicatori
    /// sulle stats base della pianta.
    /// </summary>
    // Scala 0-99. Primarie baseline 10 (neutro), resistenze baseline 0 (nessun bonus).
    // Nessun valore negativo: le specializzazioni danno bonus forti su 1-2 stat e
    // zero sulle altre (opportunita' mancata, non penalita').
    private static readonly Dictionary<SeedType, SeedStats> _typeBonuses = new()
    {
        { SeedType.Normale, new SeedStats() },

        { SeedType.Poderoso, new SeedStats {
            vitalita = 15f, idratazione = 11f, resistenzaFreddo = 10f,
            resistenzaCaldo = 10f, resistenzaParassiti = 10f,
            vegetazione = 9f, metabolismo = 8f, resistenzaVuoto = 10f
        }},

        { SeedType.Fluviale, new SeedStats {
            vitalita = 10f, idratazione = 5f, resistenzaFreddo = 10f,
            resistenzaCaldo = 0f, resistenzaParassiti = 0f,
            vegetazione = 11f, metabolismo = 10f, resistenzaVuoto = 0f
        }},

        { SeedType.Glaciale, new SeedStats {
            vitalita = 11f, idratazione = 8.5f, resistenzaFreddo = 55f,
            resistenzaCaldo = 0f, resistenzaParassiti = 15f,
            vegetazione = 8.5f, metabolismo = 8.5f, resistenzaVuoto = 25f
        }},

        { SeedType.Magmatico, new SeedStats {
            vitalita = 11f, idratazione = 14f, resistenzaFreddo = 0f,
            resistenzaCaldo = 55f, resistenzaParassiti = 25f,
            vegetazione = 8f, metabolismo = 11f, resistenzaVuoto = 15f
        }},

        { SeedType.Puro, new SeedStats {
            vitalita = 9f, idratazione = 10f, resistenzaFreddo = 0f,
            resistenzaCaldo = 0f, resistenzaParassiti = 75f,
            vegetazione = 11f, metabolismo = 10f, resistenzaVuoto = 0f
        }},

        { SeedType.Florido, new SeedStats {
            vitalita = 9.5f, idratazione = 12.5f, resistenzaFreddo = 0f,
            resistenzaCaldo = 0f, resistenzaParassiti = 0f,
            vegetazione = 15f, metabolismo = 10f, resistenzaVuoto = 0f
        }},

        { SeedType.Rapido, new SeedStats {
            vitalita = 7.5f, idratazione = 15f, resistenzaFreddo = 0f,
            resistenzaCaldo = 0f, resistenzaParassiti = 0f,
            vegetazione = 10f, metabolismo = 15f, resistenzaVuoto = 0f
        }},

        { SeedType.Antico, new SeedStats {
            vitalita = 11.5f, idratazione = 9f, resistenzaFreddo = 20f,
            resistenzaCaldo = 20f, resistenzaParassiti = 20f,
            vegetazione = 11f, metabolismo = 9f, resistenzaVuoto = 15f
        }},

        { SeedType.Cosmico, new SeedStats {
            vitalita = 10f, idratazione = 6f, resistenzaFreddo = 40f,
            resistenzaCaldo = 25f, resistenzaParassiti = 30f,
            vegetazione = 7f, metabolismo = 8f, resistenzaVuoto = 65f
        }}
    };

    public static SeedStats GetTypeBonus(SeedType type) =>
        _typeBonuses.TryGetValue(type, out var bonus) ? bonus : new SeedStats();

    // ╔════════════════════════════════════════════════════════╗
    // ║  BONUS GENERAZIONE (random ranges per GenStats)       ║
    // ╚════════════════════════════════════════════════════════╝

    /// <summary>
    /// Bonus con variazione casuale, usati solo nella generazione di nuovi semi
    /// (Seed.GenStats). Ogni valore è un range [min, max] da cui pescare con
    /// RandomHelper.Float.
    /// </summary>
    // Scala 0-99: primarie ×10 (baseline 10), resistenze ×100 (baseline 0).
    // Le resistenze NON possono essere negative. Le primarie possono avere
    // contributi negativi (semantici: Fluviale idratazione=5 significa basso
    // consumo d'acqua, quindi "efficiente"), mai sotto i min di SeedStatScaling.
    public static SeedStats GetTypeGenerationBonus(SeedType type) => type switch
    {
        SeedType.Normale => new SeedStats()
        {
            vitalita = RandomHelper.Float(-0.5f, 0.5f),
            idratazione = RandomHelper.Float(-0.5f, 0.5f),
            metabolismo = RandomHelper.Float(-0.5f, 0.5f),
            vegetazione = RandomHelper.Float(-0.5f, 0.5f),
        },

        SeedType.Poderoso => new SeedStats()
        {
            vitalita = RandomHelper.Float(4f, 6f),
            idratazione = RandomHelper.Float(0.5f, 1.5f),
            metabolismo = RandomHelper.Float(-2.5f, -1.5f),
            resistenzaParassiti = RandomHelper.Float(5f, 15f),
            vegetazione = RandomHelper.Float(-1.5f, -0.5f),
        },

        SeedType.Fluviale => new SeedStats()
        {
            idratazione = RandomHelper.Float(-6f, -4f),
            vegetazione = RandomHelper.Float(1f, 2f),
            resistenzaFreddo = RandomHelper.Float(5f, 15f),
        },

        SeedType.Florido => new SeedStats()
        {
            vegetazione = RandomHelper.Float(5f, 7f),
            idratazione = RandomHelper.Float(2f, 3f),
            vitalita = RandomHelper.Float(-1f, 0f),
        },

        SeedType.Glaciale => new SeedStats()
        {
            resistenzaFreddo = RandomHelper.Float(50f, 60f),
            vitalita = RandomHelper.Float(0.5f, 1.5f),
            idratazione = RandomHelper.Float(-2f, -1f),
            metabolismo = RandomHelper.Float(-2f, -1f),
            resistenzaVuoto = RandomHelper.Float(20f, 30f),
        },

        SeedType.Magmatico => new SeedStats()
        {
            resistenzaCaldo = RandomHelper.Float(50f, 60f),
            vitalita = RandomHelper.Float(0.5f, 1.5f),
            idratazione = RandomHelper.Float(3f, 5f),
            metabolismo = RandomHelper.Float(0.5f, 1.5f),
            resistenzaParassiti = RandomHelper.Float(20f, 30f),
        },

        SeedType.Rapido => new SeedStats()
        {
            metabolismo = RandomHelper.Float(4f, 6f),
            vitalita = RandomHelper.Float(-3f, -2f),
            idratazione = RandomHelper.Float(4f, 6f),
        },

        SeedType.Puro => new SeedStats()
        {
            resistenzaParassiti = RandomHelper.Float(65f, 75f),
            vitalita = RandomHelper.Float(-1.5f, -0.5f),
            vegetazione = RandomHelper.Float(0.5f, 1.5f),
        },

        SeedType.Antico => new SeedStats()
        {
            vitalita = RandomHelper.Float(1f, 2f),
            idratazione = RandomHelper.Float(-1.5f, -0.5f),
            metabolismo = RandomHelper.Float(-1.5f, -0.5f),
            resistenzaFreddo = RandomHelper.Float(15f, 25f),
            resistenzaCaldo = RandomHelper.Float(15f, 25f),
            resistenzaParassiti = RandomHelper.Float(15f, 25f),
            vegetazione = RandomHelper.Float(0.5f, 1.5f),
            resistenzaVuoto = RandomHelper.Float(10f, 20f),
        },

        SeedType.Cosmico => new SeedStats()
        {
            resistenzaVuoto = RandomHelper.Float(55f, 65f),
            resistenzaFreddo = RandomHelper.Float(30f, 40f),
            resistenzaCaldo = RandomHelper.Float(15f, 25f),
            resistenzaParassiti = RandomHelper.Float(20f, 30f),
            idratazione = RandomHelper.Float(-4.5f, -3.5f),
            metabolismo = RandomHelper.Float(-2.5f, -1.5f),
            vegetazione = RandomHelper.Float(-3.5f, -2.5f),
        },

        _ => new SeedStats()
    };

    // ╔════════════════════════════════════════════════════════╗
    // ║  FUSIONE                                              ║
    // ╚════════════════════════════════════════════════════════╝

    /// <summary>Coppie di tipi complementari (bonus compatibilità in fusione).</summary>
    public static readonly (SeedType, SeedType)[] ComplementaryPairs = new[]
    {
        (SeedType.Glaciale, SeedType.Magmatico),
        (SeedType.Fluviale, SeedType.Florido),
        (SeedType.Rapido,   SeedType.Poderoso),
        (SeedType.Puro,     SeedType.Antico),
    };

    public static bool AreTypesComplementary(SeedType type1, SeedType type2)
    {
        foreach (var (a, b) in ComplementaryPairs)
        {
            if ((type1 == a && type2 == b) || (type1 == b && type2 == a))
                return true;
        }
        return false;
    }
}

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
    private static readonly Dictionary<SeedType, SeedStats> _typeBonuses = new()
    {
        { SeedType.Normale, new SeedStats() },

        { SeedType.Poderoso, new SeedStats {
            vitalita = 1.5f, idratazione = 1.1f, resistenzaFreddo = 0.1f,
            resistenzaCaldo = 0.1f, resistenzaParassiti = 0.1f,
            vegetazione = 0.9f, metabolismo = 0.8f, resistenzaVuoto = 0.1f
        }},

        { SeedType.Fluviale, new SeedStats {
            vitalita = 1.0f, idratazione = 0.5f, resistenzaFreddo = 0.1f,
            resistenzaCaldo = -0.2f, resistenzaParassiti = 0.0f,
            vegetazione = 1.2f, metabolismo = 1.0f, resistenzaVuoto = 0.0f
        }},

        { SeedType.Glaciale, new SeedStats {
            vitalita = 1.1f, idratazione = 0.85f, resistenzaFreddo = 0.65f,
            resistenzaCaldo = -0.3f, resistenzaParassiti = 0.15f,
            vegetazione = 0.85f, metabolismo = 0.85f, resistenzaVuoto = 0.25f
        }},

        { SeedType.Magmatico, new SeedStats {
            vitalita = 1.1f, idratazione = 1.4f, resistenzaFreddo = -0.3f,
            resistenzaCaldo = 0.65f, resistenzaParassiti = 0.25f,
            vegetazione = 0.8f, metabolismo = 1.1f, resistenzaVuoto = 0.15f
        }},

        { SeedType.Puro, new SeedStats {
            vitalita = 0.9f, idratazione = 1.0f, resistenzaFreddo = 0.0f,
            resistenzaCaldo = 0.0f, resistenzaParassiti = 0.8f,
            vegetazione = 1.1f, metabolismo = 1.0f, resistenzaVuoto = 0.0f
        }},

        { SeedType.Florido, new SeedStats {
            vitalita = 0.95f, idratazione = 1.25f, resistenzaFreddo = -0.1f,
            resistenzaCaldo = -0.1f, resistenzaParassiti = -0.15f,
            vegetazione = 1.7f, metabolismo = 1.0f, resistenzaVuoto = 0.0f
        }},

        { SeedType.Rapido, new SeedStats {
            vitalita = 0.75f, idratazione = 1.5f, resistenzaFreddo = -0.1f,
            resistenzaCaldo = -0.1f, resistenzaParassiti = -0.1f,
            vegetazione = 1.0f, metabolismo = 1.6f, resistenzaVuoto = 0.0f
        }},

        { SeedType.Antico, new SeedStats {
            vitalita = 1.15f, idratazione = 0.9f, resistenzaFreddo = 0.2f,
            resistenzaCaldo = 0.2f, resistenzaParassiti = 0.2f,
            vegetazione = 1.1f, metabolismo = 0.9f, resistenzaVuoto = 0.15f
        }},

        { SeedType.Cosmico, new SeedStats {
            vitalita = 1.0f, idratazione = 0.6f, resistenzaFreddo = 0.45f,
            resistenzaCaldo = 0.3f, resistenzaParassiti = 0.35f,
            vegetazione = 0.7f, metabolismo = 0.8f, resistenzaVuoto = 0.7f
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
    public static SeedStats GetTypeGenerationBonus(SeedType type) => type switch
    {
        SeedType.Normale => new SeedStats()
        {
            vitalita = RandomHelper.Float(-0.05f, 0.05f),
            idratazione = RandomHelper.Float(-0.05f, 0.05f),
            metabolismo = RandomHelper.Float(-0.05f, 0.05f),
            vegetazione = RandomHelper.Float(-0.05f, 0.05f),
        },

        SeedType.Poderoso => new SeedStats()
        {
            vitalita = RandomHelper.Float(0.4f, 0.6f),
            idratazione = RandomHelper.Float(0.05f, 0.15f),
            metabolismo = RandomHelper.Float(-0.25f, -0.15f),
            resistenzaParassiti = RandomHelper.Float(0.05f, 0.15f),
            vegetazione = RandomHelper.Float(-0.15f, -0.05f),
        },

        SeedType.Fluviale => new SeedStats()
        {
            idratazione = RandomHelper.Float(-0.6f, -0.4f),
            vegetazione = RandomHelper.Float(0.15f, 0.25f),
            resistenzaCaldo = RandomHelper.Float(-0.25f, -0.15f),
            resistenzaFreddo = RandomHelper.Float(0.05f, 0.15f),
        },

        SeedType.Florido => new SeedStats()
        {
            vegetazione = RandomHelper.Float(0.6f, 0.8f),
            idratazione = RandomHelper.Float(0.2f, 0.3f),
            vitalita = RandomHelper.Float(-0.1f, 0.0f),
            resistenzaParassiti = RandomHelper.Float(-0.2f, -0.1f),
        },

        SeedType.Glaciale => new SeedStats()
        {
            resistenzaFreddo = RandomHelper.Float(0.6f, 0.7f),
            resistenzaCaldo = RandomHelper.Float(-0.35f, -0.25f),
            vitalita = RandomHelper.Float(0.05f, 0.15f),
            idratazione = RandomHelper.Float(-0.2f, -0.1f),
            metabolismo = RandomHelper.Float(-0.2f, -0.1f),
            resistenzaVuoto = RandomHelper.Float(0.2f, 0.3f),
        },

        SeedType.Magmatico => new SeedStats()
        {
            resistenzaCaldo = RandomHelper.Float(0.6f, 0.7f),
            resistenzaFreddo = RandomHelper.Float(-0.35f, -0.25f),
            vitalita = RandomHelper.Float(0.05f, 0.15f),
            idratazione = RandomHelper.Float(0.3f, 0.5f),
            metabolismo = RandomHelper.Float(0.05f, 0.15f),
            resistenzaParassiti = RandomHelper.Float(0.2f, 0.3f),
        },

        SeedType.Rapido => new SeedStats()
        {
            metabolismo = RandomHelper.Float(0.5f, 0.7f),
            vitalita = RandomHelper.Float(-0.3f, -0.2f),
            idratazione = RandomHelper.Float(0.4f, 0.6f),
            resistenzaParassiti = RandomHelper.Float(-0.15f, -0.05f),
        },

        SeedType.Puro => new SeedStats()
        {
            resistenzaParassiti = RandomHelper.Float(0.75f, 0.85f),
            vitalita = RandomHelper.Float(-0.15f, -0.05f),
            vegetazione = RandomHelper.Float(0.05f, 0.15f),
        },

        SeedType.Antico => new SeedStats()
        {
            vitalita = RandomHelper.Float(0.1f, 0.2f),
            idratazione = RandomHelper.Float(-0.15f, -0.05f),
            metabolismo = RandomHelper.Float(-0.15f, -0.05f),
            resistenzaFreddo = RandomHelper.Float(0.15f, 0.25f),
            resistenzaCaldo = RandomHelper.Float(0.15f, 0.25f),
            resistenzaParassiti = RandomHelper.Float(0.15f, 0.25f),
            vegetazione = RandomHelper.Float(0.05f, 0.15f),
            resistenzaVuoto = RandomHelper.Float(0.1f, 0.2f),
        },

        SeedType.Cosmico => new SeedStats()
        {
            resistenzaVuoto = RandomHelper.Float(0.65f, 0.75f),
            resistenzaFreddo = RandomHelper.Float(0.4f, 0.5f),
            resistenzaCaldo = RandomHelper.Float(0.25f, 0.35f),
            resistenzaParassiti = RandomHelper.Float(0.3f, 0.4f),
            idratazione = RandomHelper.Float(-0.45f, -0.35f),
            metabolismo = RandomHelper.Float(-0.25f, -0.15f),
            vegetazione = RandomHelper.Float(-0.35f, -0.25f),
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

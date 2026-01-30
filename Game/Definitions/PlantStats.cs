using System;
using System.Collections.Generic;

namespace Plants;

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
    Comune,
    NonComune,
    Raro,
    Esotico,
    Epico,
    Leggendario,
    Mitico
}

public struct SeedStats
{
    public float Vitalita  { get; set; }
    public float Idratazione { get; set; }
    public float ResistenzaFreddo { get; set; }
    public float ResistenzaCaldo { get; set; }
    public float ResistenzaParassiti { get; set; }
    public float Vegetazione { get; set; }
    public float Metabolismo { get; set; }
    public float ResistenzaVuoto { get; set; }

    public SeedStats()
    {
        Vitalita = 1.0f;
        Idratazione = 1.0f;
        ResistenzaFreddo = 0.0f;
        ResistenzaCaldo = 0.0f;
        ResistenzaParassiti = 0.0f;
        Vegetazione = 1.0f;
        Metabolismo = 1.0f;
        ResistenzaVuoto = 0.0f;
    }
}

public static class SeedDataType
{
    private static readonly Dictionary<SeedType, SeedStats> _bonuses = new()
    {
        { SeedType.Normale, new SeedStats()},

        { SeedType.Poderoso, new SeedStats {
            Vitalita = 1.5f, Idratazione = 1.1f, ResistenzaFreddo = 0.1f,
            ResistenzaCaldo = 0.1f, ResistenzaParassiti = 0.1f,
            Vegetazione = 0.9f, Metabolismo = 0.8f, ResistenzaVuoto = 0.1f
        }},

        { SeedType.Fluviale, new SeedStats {
            Vitalita = 1.0f, Idratazione = 0.5f, ResistenzaFreddo = 0.1f,
            ResistenzaCaldo = -0.2f, ResistenzaParassiti = 0.0f,
            Vegetazione = 1.2f, Metabolismo = 1.0f, ResistenzaVuoto = 0.0f
        }},

        { SeedType.Glaciale, new SeedStats {
            Vitalita = 1.1f, Idratazione = 0.85f, ResistenzaFreddo = 0.65f,
            ResistenzaCaldo = -0.3f, ResistenzaParassiti = 0.15f,
            Vegetazione = 0.85f, Metabolismo = 0.85f, ResistenzaVuoto = 0.25f
        }},

        { SeedType.Magmatico, new SeedStats {
            Vitalita = 1.1f, Idratazione = 1.4f, ResistenzaFreddo = -0.3f,
            ResistenzaCaldo = 0.65f, ResistenzaParassiti = 0.25f,
            Vegetazione = 0.8f, Metabolismo = 1.1f, ResistenzaVuoto = 0.15f
        }},

        { SeedType.Puro, new SeedStats {
            Vitalita = 0.9f, Idratazione = 1.0f, ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = 0.0f, ResistenzaParassiti = 0.8f,
            Vegetazione = 1.1f, Metabolismo = 1.0f, ResistenzaVuoto = 0.0f
        }},

        { SeedType.Florido, new SeedStats {
            Vitalita = 0.95f, Idratazione = 1.25f, ResistenzaFreddo = -0.1f,
            ResistenzaCaldo = -0.1f, ResistenzaParassiti = -0.15f,
            Vegetazione = 1.7f, Metabolismo = 1.0f, ResistenzaVuoto = 0.0f
        }},

        { SeedType.Rapido, new SeedStats {
            Vitalita = 0.75f, Idratazione = 1.5f, ResistenzaFreddo = -0.1f,
            ResistenzaCaldo = -0.1f, ResistenzaParassiti = -0.1f,
            Vegetazione = 1.0f, Metabolismo = 1.6f, ResistenzaVuoto = 0.0f
        }},

        { SeedType.Antico, new SeedStats {
            Vitalita = 1.15f, Idratazione = 0.9f, ResistenzaFreddo = 0.2f,
            ResistenzaCaldo = 0.2f, ResistenzaParassiti = 0.2f,
            Vegetazione = 1.1f, Metabolismo = 0.9f, ResistenzaVuoto = 0.15f
        }},

        { SeedType.Cosmico, new SeedStats {
            Vitalita = 1.0f, Idratazione = 0.6f, ResistenzaFreddo = 0.45f,
            ResistenzaCaldo = 0.3f, ResistenzaParassiti = 0.35f,
            Vegetazione = 0.7f, Metabolismo = 0.8f, ResistenzaVuoto = 0.7f
        }}
    };

    private static readonly Dictionary<SeedType, SeedRarity> _rarities = new()
    {
        { SeedType.Normale, SeedRarity.Comune },
        { SeedType.Poderoso, SeedRarity.NonComune },
        { SeedType.Fluviale, SeedRarity.NonComune },
        { SeedType.Glaciale, SeedRarity.Raro },
        { SeedType.Magmatico, SeedRarity.Raro },
        { SeedType.Puro, SeedRarity.Epico },
        { SeedType.Florido, SeedRarity.NonComune },
        { SeedType.Rapido, SeedRarity.Raro },
        { SeedType.Antico, SeedRarity.Epico },
        { SeedType.Cosmico, SeedRarity.Leggendario }
    };

    public static SeedStats GetBonus(SeedType type) =>
        _bonuses.TryGetValue(type, out var bonus) ? bonus : new SeedStats();

    public static SeedRarity GetRarity(SeedType type) =>
        _rarities.TryGetValue(type, out var rarity) ? rarity : SeedRarity.Comune;

    public static string GetName(SeedType type) => type switch
    {
        SeedType.Normale => "Seme Normale",
        SeedType.Poderoso => "Seme Poderoso",
        SeedType.Fluviale => "Seme Fluviale",
        SeedType.Glaciale => "Seme Glaciale",
        SeedType.Magmatico => "Seme Magmatico",
        SeedType.Puro => "Seme Puro",
        SeedType.Florido => "Seme Florido",
        SeedType.Rapido => "Seme Rapido",
        SeedType.Antico => "Seme Antico",
        SeedType.Cosmico => "Seme Cosmico",
        _ => "Seme Sconosciuto"
    };

    public static string GetDescription(SeedType type) => type switch
    {
        SeedType.Normale => "Un seme comune senza particolari proprietà.",
        SeedType.Poderoso => "Piante robuste ma lente. +50% vita.",
        SeedType.Fluviale => "Consuma metà acqua. Soffre il caldo.",
        SeedType.Glaciale => "Resiste al gelo. Vulnerabile al caldo.",
        SeedType.Magmatico => "Prospera nel caldo. Assetato.",
        SeedType.Puro => "Immune ai parassiti. Fragile.",
        SeedType.Florido => "Fogliame rigoglioso. Richiede cure.",
        SeedType.Rapido => "Crescita veloce. Molto fragile.",
        SeedType.Antico => "Equilibrato. Bonus a tutto.",
        SeedType.Cosmico => "Adatto allo spazio. Lento sulla Terra.",
        _ => "???"
    };
}

public class PlantStats
{
    public float Salute = 1f;
    public float Idratazione = 0.4f;
    public float Ossigeno = 1.0f;
    public float Metabolismo = 0.8f;
    public float Temperatura = 20.0f;
    public float ResistenzaFreddo = 0.0f;
    public float ResistenzaCaldo = 0.0f;
    public float ResistenzaParassiti = 0.0f;
    public float ResistenzaVuoto = 0.0f;
    public int FoglieBase = 120;
    public int FoglieAttuali = 1000;
    public float DropRateFoglie = 0.003f;
    public float Altezza = 0.0f;
    public float AltezzaMassima = 5000.0f;
    public bool Infestata = false;
    public float IntensitaInfestazione = 0.0f;

    public float EffectiveMaxHeight => AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
   
    public const float SALUTE_MIN = 0f;
    public const float SALUTE_MAX = 1f;

    public void ClampAllValues()
    {
        Salute = Math.Clamp(Salute, SALUTE_MIN, SALUTE_MAX);
        Idratazione = Math.Clamp(Idratazione, 0f, 1f);
        Ossigeno = Math.Clamp(Ossigeno, 0f, 1f);
        Metabolismo = Math.Clamp(Metabolismo, 0f, 2f);
        FoglieAttuali = Math.Clamp(FoglieAttuali, 0, FoglieBase * 3);
        IntensitaInfestazione = Math.Clamp(IntensitaInfestazione, 0f, 1f);
    }
}
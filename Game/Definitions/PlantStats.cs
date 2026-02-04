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

public class SeedStats
{
    public float vitalita  { get; set; }
    public float idratazione { get; set; }
    public float resistenzaFreddo { get; set; }
    public float resistenzaCaldo { get; set; }
    public float resistenzaParassiti { get; set; }
    public float vegetazione { get; set; }
    public float metabolismo { get; set; }
    public float resistenzaVuoto { get; set; }
    public int fusionCount { get; set; }

    public SeedStats()
    {
        vitalita = 1.0f;
        idratazione = 1.0f;
        resistenzaFreddo = 0.0f;
        resistenzaCaldo = 0.0f;
        resistenzaParassiti = 0.0f;
        vegetazione = 1.0f;
        metabolismo = 1.0f;
        resistenzaVuoto = 0.0f;
        fusionCount = 0;
    }
}

public static class SeedDataType
{
    private static readonly Dictionary<SeedType, SeedStats> _bonuses = new()
    {
        { SeedType.Normale, new SeedStats()},

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
using Engine.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

public class Seed
{
    public SeedStats stats { get; set; }
    public String name { get; set; }
    public SeedRarity rarity { get; set; }
    private SeedType _type;
    public SeedType type
    {
        get
        {
            return _type;
        }
        set
        {
            this._type = value;
            this.color = GetColorFromType(value);
            this.name = GetNameFromType(value);
            this.rarity = GetRarityFromType(value);
        }
    }
    public Vector3 color { get; set; }


    public Seed()
    {
    }

    public Seed(SeedType type)
    {
        this.name = GetNameFromType(type);
        this.rarity = GetRarityFromType(type);
        this.type = type;
        this.color = GetColorFromType(type);
        this.stats = GenStats();
    }


    public Seed(Seed seed1, Seed seed2)
    {
		this.stats = GenStats(seed1, seed2);
		this.name = "Seme";
		this.type = SeedType.Normale;
		this.color = GetColorFromType(this.type);
    }

    private static string GetNameFromType(SeedType type) => type switch
    {
        SeedType.Normale => "Seme Normale",
        SeedType.Poderoso => "Seme Poderoso",
        SeedType.Fluviale => "Seme Fluviale",
        SeedType.Florido => "Seme Florido",
        SeedType.Glaciale => "Seme Glaciale",
        SeedType.Magmatico => "Seme Magmatico",
        SeedType.Rapido => "Seme Rapido",
        SeedType.Puro => "Seme Puro",
        SeedType.Antico => "Seme Antico",
        SeedType.Cosmico => "Seme Cosmico",
        _ => "Seme"
    };

    private static SeedRarity GetRarityFromType(SeedType type) => type switch
    {
        SeedType.Normale => SeedRarity.Comune,
        SeedType.Poderoso => SeedRarity.NonComune,
        SeedType.Fluviale => SeedRarity.NonComune,
        SeedType.Florido => SeedRarity.NonComune,
        SeedType.Glaciale => SeedRarity.Raro,
        SeedType.Magmatico => SeedRarity.Raro,
        SeedType.Rapido => SeedRarity.Raro,
        SeedType.Puro => SeedRarity.Epico,
        SeedType.Antico => SeedRarity.Epico,
        SeedType.Cosmico => SeedRarity.Leggendario,
        _ => SeedRarity.Comune
    };

    private static Vector3 GetColorFromType(SeedType type) => type switch
    {
        SeedType.Normale => new Vector3(0.6f, 0.6f, 0.6f), // grigio neutro
        SeedType.Poderoso => new Vector3(0.9f, 0.2f, 0.2f), // rosso intenso (forza)
        SeedType.Fluviale => new Vector3(0.2f, 0.5f, 0.9f), // blu acqua
        SeedType.Florido => new Vector3(0.2f, 0.8f, 0.3f), // verde vivo
        SeedType.Glaciale => new Vector3(0.7f, 0.9f, 1.0f), // azzurro ghiaccio
        SeedType.Magmatico => new Vector3(1.0f, 0.4f, 0.0f), // arancione lava
        SeedType.Rapido => new Vector3(1.0f, 1.0f, 0.2f), // giallo elettrico
        SeedType.Puro => new Vector3(1.0f, 1.0f, 1.0f), // bianco puro
        SeedType.Antico => new Vector3(0.5f, 0.4f, 0.2f), // marrone/oro spento
        SeedType.Cosmico => new Vector3(0.6f, 0.2f, 0.8f), // viola cosmico
        _ => new Vector3(0.6f, 0.6f, 0.6f)
    };

    private SeedStats GenStats()
    {
        var baseValues = new SeedStats();

        var allSeeds = Inventario.get().GetAllSeeds();
        if (allSeeds != null && allSeeds.Count > 0)
        {
            baseValues.idratazione          = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.idratazione).ToList());
            baseValues.metabolismo          = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.metabolismo).ToList());
            baseValues.resistenzaCaldo      = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.resistenzaCaldo).ToList());
            baseValues.resistenzaFreddo     = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.resistenzaFreddo).ToList());
            baseValues.resistenzaParassiti  = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.resistenzaParassiti).ToList());
            baseValues.resistenzaVuoto      = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.resistenzaVuoto).ToList());
            baseValues.vegetazione          = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.vegetazione).ToList());
            baseValues.vitalita             = MathHelper.CalcoloMediaValori(allSeeds.Select(o => o.stats.vitalita).ToList());
        }
        else
        {
            // Valori base per il primo seme mai generato
            baseValues.vitalita = 1.0f;
            baseValues.idratazione = 1.0f;
            baseValues.metabolismo = 1.0f;
            baseValues.vegetazione = 1.0f;
            baseValues.resistenzaFreddo = 0.0f;
            baseValues.resistenzaCaldo = 0.0f;
            baseValues.resistenzaParassiti = 0.0f;
            baseValues.resistenzaVuoto = 0.0f;
        }


        var randomVariation = new SeedStats()
        {
            vitalita = RandomHelper.Float(-0.1f, 0.1f),
            idratazione = RandomHelper.Float(-0.1f, 0.1f),
            metabolismo = RandomHelper.Float(-0.1f, 0.1f),
            resistenzaCaldo = RandomHelper.Float(-0.05f, 0.05f),
            resistenzaFreddo = RandomHelper.Float(-0.05f, 0.05f),
            resistenzaParassiti = RandomHelper.Float(-0.05f, 0.05f),
            resistenzaVuoto = RandomHelper.Float(-0.05f, 0.05f),
            vegetazione = RandomHelper.Float(-0.1f, 0.1f),
        };

        var typeBonus = GetTypeBonuses(this.type);

        float rarityMultiplier = GetRarityMultiplier(this.rarity);

        var finalStats = new SeedStats()
        {
            vitalita = Math.Max(0.5f, (baseValues.vitalita + randomVariation.vitalita + typeBonus.vitalita) * rarityMultiplier),
            idratazione = Math.Max(0.3f, (baseValues.idratazione + randomVariation.idratazione + typeBonus.idratazione) * rarityMultiplier),
            metabolismo = Math.Max(0.5f, (baseValues.metabolismo + randomVariation.metabolismo + typeBonus.metabolismo) * rarityMultiplier),
            vegetazione = Math.Max(0.5f, (baseValues.vegetazione + randomVariation.vegetazione + typeBonus.vegetazione) * rarityMultiplier),
            resistenzaFreddo = Math.Clamp((baseValues.resistenzaFreddo + randomVariation.resistenzaFreddo + typeBonus.resistenzaFreddo) * rarityMultiplier, -0.5f, 1.0f),
            resistenzaCaldo = Math.Clamp((baseValues.resistenzaCaldo + randomVariation.resistenzaCaldo + typeBonus.resistenzaCaldo) * rarityMultiplier, -0.5f, 1.0f),
            resistenzaParassiti = Math.Clamp((baseValues.resistenzaParassiti + randomVariation.resistenzaParassiti + typeBonus.resistenzaParassiti) * rarityMultiplier, -0.5f, 1.0f),
            resistenzaVuoto = Math.Clamp((baseValues.resistenzaVuoto + randomVariation.resistenzaVuoto + typeBonus.resistenzaVuoto) * rarityMultiplier, -0.3f, 1.0f),
        };

        return finalStats;
    }

    private SeedStats GetTypeBonuses(SeedType type)
    {
        return type switch
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
    }

    private float GetRarityMultiplier(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune => 1.0f,
        SeedRarity.NonComune => 1.1f,   // +10%
        SeedRarity.Raro => 1.25f,       // +25%
        SeedRarity.Epico => 1.5f,       // +50%
        SeedRarity.Leggendario => 2.0f, // +100%
        _ => 1.0f
    };

    private SeedStats GenStats(Seed seed1, Seed seed2)
    {

        var hybrid = new SeedStats();

        hybrid.vitalita = BreedStat(seed1.stats.vitalita, seed2.stats.vitalita, 0.05f);
        hybrid.idratazione = BreedStat(seed1.stats.idratazione, seed2.stats.idratazione, 0.05f);
        hybrid.metabolismo = BreedStat(seed1.stats.metabolismo, seed2.stats.metabolismo, 0.05f);
        hybrid.vegetazione = BreedStat(seed1.stats.vegetazione, seed2.stats.vegetazione, 0.05f);
        hybrid.resistenzaFreddo = BreedStat(seed1.stats.resistenzaFreddo, seed2.stats.resistenzaFreddo, 0.05f);
        hybrid.resistenzaCaldo = BreedStat(seed1.stats.resistenzaCaldo, seed2.stats.resistenzaCaldo, 0.05f);
        hybrid.resistenzaParassiti = BreedStat(seed1.stats.resistenzaParassiti, seed2.stats.resistenzaParassiti, 0.05f);
        hybrid.resistenzaVuoto = BreedStat(seed1.stats.resistenzaVuoto, seed2.stats.resistenzaVuoto, 0.05f);

        float compatibilityBonus = CalculateCompatibilityBonus(seed1, seed2);
        if (compatibilityBonus > 0)
        {
            hybrid.vitalita *= (1f + compatibilityBonus * 0.1f);
            hybrid.metabolismo *= (1f + compatibilityBonus * 0.1f);
        }

        hybrid.vitalita = Math.Max(0.5f, hybrid.vitalita);
        hybrid.idratazione = Math.Max(0.3f, hybrid.idratazione);
        hybrid.metabolismo = Math.Max(0.5f, hybrid.metabolismo);
        hybrid.vegetazione = Math.Max(0.5f, hybrid.vegetazione);
        hybrid.resistenzaFreddo = Math.Clamp(hybrid.resistenzaFreddo, -0.5f, 1.0f);
        hybrid.resistenzaCaldo = Math.Clamp(hybrid.resistenzaCaldo, -0.5f, 1.0f);
        hybrid.resistenzaParassiti = Math.Clamp(hybrid.resistenzaParassiti, -0.5f, 1.0f);
        hybrid.resistenzaVuoto = Math.Clamp(hybrid.resistenzaVuoto, -0.3f, 1.0f);

        return hybrid;
    }

    private float BreedStat(float stat1, float stat2, float mutationChance)
    {
        float better = Math.Max(stat1, stat2);
        float worse = Math.Min(stat1, stat2);

        float bred = better * 0.7f + worse * 0.3f;

        if (RandomHelper.Float(0, 1) < mutationChance)
        {
            float mutation = RandomHelper.Float(-0.15f, 0.15f);
            bred += bred * mutation;
        }

        return bred;
    }

    private float CalculateCompatibilityBonus(Seed seed1, Seed seed2)
    {
        float bonus = 0f;

        int rarityDiff = Math.Abs((int)seed1.rarity - (int)seed2.rarity);
        if (rarityDiff == 0)
            bonus += 0.3f;
        else if (rarityDiff == 1)
            bonus += 0.15f; 

        if (AreTypesComplementary(seed1.type, seed2.type))
            bonus += 0.2f;

        return bonus;
    }

    private bool AreTypesComplementary(SeedType type1, SeedType type2)
    {
        var complementaryPairs = new List<(SeedType, SeedType)>
        {
            (SeedType.Glaciale, SeedType.Magmatico),
            (SeedType.Fluviale, SeedType.Florido),
            (SeedType.Rapido, SeedType.Poderoso),
            (SeedType.Puro, SeedType.Antico),
        };

        foreach (var pair in complementaryPairs)
        {
            if ((type1 == pair.Item1 && type2 == pair.Item2) ||
                (type1 == pair.Item2 && type2 == pair.Item1))
            {
                return true;
            }
        }

        return false;
    }

    private SeedRarity CalculateBreedingRarity(SeedRarity rarity1, SeedRarity rarity2)
    {
        int avgRarity = ((int)rarity1 + (int)rarity2) / 2;

        int higherRarity = Math.Max((int)rarity1, (int)rarity2);
        if (RandomHelper.Float(0, 1) < 0.2f)
        {
            avgRarity = higherRarity;
        }

        if (RandomHelper.Float(0, 1) < 0.05f && avgRarity < (int)SeedRarity.Leggendario)
        {
            avgRarity++;
        }

        return (SeedRarity)Math.Clamp(avgRarity, 0, (int)SeedRarity.Leggendario);
    }

    private Vector3 BlendColors(Vector3 color1, Vector3 color2)
    {
        float r = (color1.X + color2.X) / 2f + RandomHelper.Float(-0.1f, 0.1f);
        float g = (color1.Y + color2.Y) / 2f + RandomHelper.Float(-0.1f, 0.1f);
        float b = (color1.Z + color2.Z) / 2f + RandomHelper.Float(-0.1f, 0.1f);

        return new Vector3(
            Math.Clamp(r, 0f, 1f),
            Math.Clamp(g, 0f, 1f),
            Math.Clamp(b, 0f, 1f)
        );
    }

}
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
    public int upgradeLevel { get; set; } = 0;
    public List<SeedStatType> upgradedStats { get; set; }

    public int id { get; set; } = 0;

    public const int MAX_ITEM_SLOTS = 3;
    public List<string> equippedItems { get; set; } = new() { null, null, null };

    public const int MAX_FUSIONS = 4;

    public bool CanBeFused => stats.fusionCount < MAX_FUSIONS;

    public int RemainingFusions => Math.Max(0, MAX_FUSIONS - stats.fusionCount);


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
        this.upgradeLevel = 0;
        this.upgradedStats = new List<SeedStatType>();
    }


    public Seed(Seed seed1, Seed seed2)
    {
        // Il setter di `type` sovrascrive name, rarity e color in base al tipo,
        // quindi impostiamo prima il tipo e poi sovrascriviamo i campi specifici
        // dell'ibrido. Senza questa ordine il seme fuso sarebbe sempre Comune.
        // Il tipo viene ereditato casualmente da uno dei due genitori (50/50).
        this.type = RandomHelper.Float(0, 1) < 0.5f ? seed1.type : seed2.type;

        this.stats = GenStats(seed1, seed2);
        this.rarity = CalculateBreedingRarity(seed1.rarity, seed2.rarity);
        this.color = BlendColors(seed1.color, seed2.color);
        this.upgradeLevel = 0;
        this.upgradedStats = new List<SeedStatType>();
    }

    private static string GetNameFromType(SeedType type) => SeedDefinitions.GetSeedName(type);

    public static SeedRarity GetRarityFromType(SeedType type) => SeedDefinitions.GetRarityForType(type);

    private static Vector3 GetColorFromType(SeedType type) => SeedDefinitions.GetSeedColor(type);

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
            // Primo seme generato: baseline 10 (primarie) / 0 (resistenze) su scala 0-99.
            baseValues.vitalita            = SeedStatScaling.PrimaryNeutralReference;
            baseValues.idratazione         = SeedStatScaling.PrimaryNeutralReference;
            baseValues.metabolismo         = SeedStatScaling.PrimaryNeutralReference;
            baseValues.vegetazione         = SeedStatScaling.PrimaryNeutralReference;
            baseValues.resistenzaFreddo    = 0f;
            baseValues.resistenzaCaldo     = 0f;
            baseValues.resistenzaParassiti = 0f;
            baseValues.resistenzaVuoto     = 0f;
        }


        var randomVariation = new SeedStats()
        {
            vitalita = RandomHelper.Float(-1f, 1f),
            idratazione = RandomHelper.Float(-1f, 1f),
            metabolismo = RandomHelper.Float(-1f, 1f),
            resistenzaCaldo = RandomHelper.Float(-2f, 2f),
            resistenzaFreddo = RandomHelper.Float(-2f, 2f),
            resistenzaParassiti = RandomHelper.Float(-2f, 2f),
            resistenzaVuoto = RandomHelper.Float(-2f, 2f),
            vegetazione = RandomHelper.Float(-1f, 1f),
        };

        var typeBonus = GetTypeBonuses(this.type);

        float rarityMultiplier = GetRarityMultiplier(this.rarity);

        var finalStats = new SeedStats()
        {
            vitalita     = SeedStatScaling.ClampPrimary((baseValues.vitalita     + randomVariation.vitalita     + typeBonus.vitalita)     * rarityMultiplier, SeedStatScaling.VitalitaMin),
            idratazione  = SeedStatScaling.ClampPrimary((baseValues.idratazione  + randomVariation.idratazione  + typeBonus.idratazione)  * rarityMultiplier, SeedStatScaling.IdratazioneMin),
            metabolismo  = SeedStatScaling.ClampPrimary((baseValues.metabolismo  + randomVariation.metabolismo  + typeBonus.metabolismo)  * rarityMultiplier, SeedStatScaling.MetabolismoMin),
            vegetazione  = SeedStatScaling.ClampPrimary((baseValues.vegetazione  + randomVariation.vegetazione  + typeBonus.vegetazione)  * rarityMultiplier, SeedStatScaling.VegetazioneMin),
            resistenzaFreddo    = SeedStatScaling.ClampResistance((baseValues.resistenzaFreddo    + randomVariation.resistenzaFreddo    + typeBonus.resistenzaFreddo)    * rarityMultiplier),
            resistenzaCaldo     = SeedStatScaling.ClampResistance((baseValues.resistenzaCaldo     + randomVariation.resistenzaCaldo     + typeBonus.resistenzaCaldo)     * rarityMultiplier),
            resistenzaParassiti = SeedStatScaling.ClampResistance((baseValues.resistenzaParassiti + randomVariation.resistenzaParassiti + typeBonus.resistenzaParassiti) * rarityMultiplier),
            resistenzaVuoto     = SeedStatScaling.ClampResistance((baseValues.resistenzaVuoto     + randomVariation.resistenzaVuoto     + typeBonus.resistenzaVuoto)     * rarityMultiplier),
            fusionCount = 0
        };

        return finalStats;
    }

    private SeedStats GetTypeBonuses(SeedType type) => SeedDefinitions.GetTypeGenerationBonus(type);

    private float GetRarityMultiplier(SeedRarity rarity) => SeedDefinitions.GetRarityMultiplier(rarity);

    private SeedStats GenStats(Seed seed1, Seed seed2)
    {

        var hybrid = new SeedStats();

        hybrid.vitalita = BreedStat(seed1.stats.vitalita, seed2.stats.vitalita, 0.5f);
        hybrid.idratazione = BreedStat(seed1.stats.idratazione, seed2.stats.idratazione, 0.5f);
        hybrid.metabolismo = BreedStat(seed1.stats.metabolismo, seed2.stats.metabolismo, 0.5f);
        hybrid.vegetazione = BreedStat(seed1.stats.vegetazione, seed2.stats.vegetazione, 0.5f);
        hybrid.resistenzaFreddo = BreedStat(seed1.stats.resistenzaFreddo, seed2.stats.resistenzaFreddo, 0.3f);
        hybrid.resistenzaCaldo = BreedStat(seed1.stats.resistenzaCaldo, seed2.stats.resistenzaCaldo, 0.3f);
        hybrid.resistenzaParassiti = BreedStat(seed1.stats.resistenzaParassiti, seed2.stats.resistenzaParassiti, 0.3f);
        hybrid.resistenzaVuoto = BreedStat(seed1.stats.resistenzaVuoto, seed2.stats.resistenzaVuoto, 0.1f);

        float compatibilityBonus = CalculateCompatibilityBonus(seed1, seed2);
        if (compatibilityBonus > 0)
        {
            hybrid.vitalita *= (1f + compatibilityBonus * 0.1f);
            hybrid.metabolismo *= (1f + compatibilityBonus * 0.1f);
        }

        hybrid.vitalita     = SeedStatScaling.ClampPrimary(hybrid.vitalita,     SeedStatScaling.VitalitaMin);
        hybrid.idratazione  = SeedStatScaling.ClampPrimary(hybrid.idratazione,  SeedStatScaling.IdratazioneMin);
        hybrid.metabolismo  = SeedStatScaling.ClampPrimary(hybrid.metabolismo,  SeedStatScaling.MetabolismoMin);
        hybrid.vegetazione  = SeedStatScaling.ClampPrimary(hybrid.vegetazione,  SeedStatScaling.VegetazioneMin);
        hybrid.resistenzaFreddo    = SeedStatScaling.ClampResistance(hybrid.resistenzaFreddo);
        hybrid.resistenzaCaldo     = SeedStatScaling.ClampResistance(hybrid.resistenzaCaldo);
        hybrid.resistenzaParassiti = SeedStatScaling.ClampResistance(hybrid.resistenzaParassiti);
        hybrid.resistenzaVuoto     = SeedStatScaling.ClampResistance(hybrid.resistenzaVuoto);
        hybrid.fusionCount = Math.Max(seed1.stats.fusionCount, seed2.stats.fusionCount) + 1;

        return hybrid;
    }

    // 70/30 pesato su migliore/peggiore: la media tende al genitore migliore
    // senza salti grandi. Mutazione ±15% lo rende un po' proporzionale alla scala
    // corrente del valore. Compatibilita' (vedi CalculateCompatibilityBonus)
    // aggiunge un piccolo bonus moltiplicativo a vitalita/metabolismo.
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

        // Usa il rank canonico, non l'int dell'enum (Esotico=100, Mitico=101
        // romperebbero il calcolo della differenza).
        int rarityDiff = Math.Abs(SeedDefinitions.GetRarityRank(seed1.rarity)
                                - SeedDefinitions.GetRarityRank(seed2.rarity));
        if (rarityDiff == 0)
            bonus += 0.3f;
        else if (rarityDiff == 1)
            bonus += 0.15f;

        if (AreTypesComplementary(seed1.type, seed2.type))
            bonus += 0.2f;

        return bonus;
    }

    private bool AreTypesComplementary(SeedType type1, SeedType type2) =>
        SeedDefinitions.AreTypesComplementary(type1, type2);

    private SeedRarity CalculateBreedingRarity(SeedRarity rarity1, SeedRarity rarity2)
    {
        // Usa il rank canonico (SeedDefinitions.RarityOrder) invece del valore int
        // dell'enum, che non e' sequenziale (Esotico=100, Mitico=101).
        int rank1 = SeedDefinitions.GetRarityRank(rarity1);
        int rank2 = SeedDefinitions.GetRarityRank(rarity2);
        int minRank = Math.Min(rank1, rank2);
        int maxRank = Math.Max(rank1, rank2);

        int resultRank;
        if (minRank == maxRank)
        {
            // Stessa rarità: risultato garantito della stessa rarità
            // (es. 2 Leggendari -> sempre Leggendario).
            resultRank = maxRank;
        }
        else
        {
            // Rarità diverse: il risultato tende verso quella più alta (70%),
            // ma con 30% puo' scendere a quella più bassa.
            resultRank = RandomHelper.Float(0, 1) < 0.3f ? minRank : maxRank;
        }

        // 5% di probabilità di upgrade al tier successivo, fino al massimo
        // ottenibile tramite fusione (Leggendario). Mitico non e' raggiungibile
        // tramite fusione: per 2 Leggendari il risultato sara' sempre Leggendario.
        if (resultRank < SeedDefinitions.MAX_BREEDING_RANK
            && RandomHelper.Float(0, 1) < 0.05f)
        {
            resultRank++;
        }

        return SeedDefinitions.GetRarityAtRank(resultRank);
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
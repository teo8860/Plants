using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public class OpeningSystem
{
    private static int luckEpic = 0;
    private static int luckLegendary = 0;

    private const int MAX_LUCK_EPIC = 60;
    private const int MAX_LUCK_LEGENDARY = 40;

    private readonly Dictionary<SeedRarity, int> baseWeights = new()
    {
        { SeedRarity.Comune, 750 },
        { SeedRarity.NonComune, 400 },
        { SeedRarity.Raro, 100 },
        { SeedRarity.Epico, 20 },
        { SeedRarity.Leggendario, 5 }
    };

    public Seed RollSeedFromPackage(SeedPackageRarity packageRarity)
    {

        var rollWeights = new Dictionary<SeedRarity, int>(baseWeights);

        switch (packageRarity)
        {
            case SeedPackageRarity.Uncommon:
                rollWeights[SeedRarity.NonComune] += 50;
                break;
            case SeedPackageRarity.Rare:
                rollWeights[SeedRarity.Raro] += 30;
                break;
            case SeedPackageRarity.Epic:
                rollWeights[SeedRarity.NonComune] -= 5;
                rollWeights[SeedRarity.Epico] += 10;
                break;
            case SeedPackageRarity.Legendary:
                rollWeights[SeedRarity.Comune] -= 15;
                rollWeights[SeedRarity.NonComune] -= 10;
                rollWeights[SeedRarity.Raro] += 20;
                rollWeights[SeedRarity.Epico] += 15;
                rollWeights[SeedRarity.Leggendario] += 5;
                break;
        }
        rollWeights[SeedRarity.Epico] = Math.Max(1, rollWeights[SeedRarity.Epico] + luckEpic);
        rollWeights[SeedRarity.Leggendario] = Math.Max(1, rollWeights[SeedRarity.Leggendario] + luckLegendary);

        int totalWeight = rollWeights.Values.Sum();
        int roll = Random.Shared.Next(1, totalWeight + 1);
        int cursor = 0;
        SeedRarity selectedRarity = SeedRarity.Comune;


		foreach (var item in rollWeights)
        {
            cursor += item.Value;
            if (roll <= cursor)
            {
                selectedRarity = item.Key;
                break;
            }
        }

		ApplyMomentum(selectedRarity);

        return CreateRandomSeedOfRarity(selectedRarity);
    }

    private void ApplyMomentum(SeedRarity rarity)
    {
        switch (rarity)
        {
            case SeedRarity.Comune:
                luckEpic = Math.Min(luckEpic + 3, MAX_LUCK_EPIC);
                luckLegendary = Math.Min(luckLegendary + 2, MAX_LUCK_LEGENDARY);
                break;
            case SeedRarity.NonComune:
                luckEpic = Math.Min(luckEpic + 2, MAX_LUCK_EPIC);
                luckLegendary = Math.Min(luckLegendary + 1, MAX_LUCK_LEGENDARY);
                break;
            case SeedRarity.Raro:
                luckEpic = Math.Max(0, luckEpic - 1);
                break;
            case SeedRarity.Epico:
                luckEpic = 0;
                luckLegendary = Math.Max(0, luckLegendary - 2);
                break;
            case SeedRarity.Leggendario:
                luckEpic = 0;
                luckLegendary = 0;
                break;
        }
    }

    private Seed CreateRandomSeedOfRarity(SeedRarity rarity)
    {
        var possibleTypes = Enum.GetValues<SeedType>()
            .Cast<SeedType>()
            .Where(t => new Seed(t).rarity == rarity)
            .ToList();

        if (possibleTypes.Count == 0) return new Seed(SeedType.Normale);

        SeedType chosenType = possibleTypes[Random.Shared.Next(possibleTypes.Count)];
        return new Seed(chosenType);
    }
}
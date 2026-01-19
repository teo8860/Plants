using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

public struct Seed
{
	public SeedStats stats  { get; set; }
	public String name  { get; set; }
	public SeedRarity rarity  { get; set; }
	public SeedType type  { get; set; }

	public Seed()
	{
		this.stats = new();
		this.name = "Seme";
		this.rarity = SeedRarity.Comune;
		this.type = SeedType.Normale;
	}

	public Seed(SeedType type)
	{
		this.stats = new();
		this.name = GetNameFromType(type);
		this.rarity = GetRarityFromType(type);
		this.type = type;
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

}

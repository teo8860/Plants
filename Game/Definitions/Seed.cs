using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

public struct Seed
{
	public SeedStats stats  { get; set; }
	public String name  { get; set; }
	public SeedRarity rarity  { get; set; }
	public SeedType type  { get; set; }
	public Vector3 color { get; set; }

    public Seed()
	{
		this.stats = new();
		this.name = "Seme";
		this.rarity = SeedRarity.Comune;
		this.type = SeedType.Normale;
		this.color = GetColorFromType(this.type);
    }

	public Seed(SeedType type)
	{
		this.stats = new();
		this.name = GetNameFromType(type);
		this.rarity = GetRarityFromType(type);
		this.type = type;
        this.color = GetColorFromType(type);
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

}

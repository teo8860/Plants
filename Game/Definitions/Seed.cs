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
	public SeedStats stats  { get; set; }
	public String name  { get; set; }
	public SeedRarity rarity  { get; set; }
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
		// Calcoliamo i valori di base da cui partire

		{
			idratazione			= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.idratazione).ToList()),
			metabolismo			= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.metabolismo).ToList()),
			resistenzaCaldo		= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.resistenzaCaldo).ToList()),
			resistenzaFreddo	= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.resistenzaFreddo).ToList()),
			resistenzaParassiti = MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.resistenzaParassiti).ToList()),
			resistenzaVuoto		= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.resistenzaVuoto).ToList()),
			vegetazione			= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.vegetazione).ToList()),
			vitalita			= MathHelper.CalcoloMediaValori(Inventario.get().GetAllSeeds().Select(o=> o.stats.vitalita).ToList()),
		};

		// Calcoliamo i punti da sommare, di base tutti neutrali
		var additionalValues = new SeedStats()
		{
			idratazione			= RandomHelper.Int(-10, 10),
			metabolismo			= RandomHelper.Int(-10, 10),
			resistenzaCaldo		= RandomHelper.Int(-10, 10),
			resistenzaFreddo	= RandomHelper.Int(-10, 10),
			resistenzaParassiti = RandomHelper.Int(-10, 10),
			resistenzaVuoto		= RandomHelper.Int(-10, 10),
			vegetazione			= RandomHelper.Int(-10, 10),
			vitalita			= RandomHelper.Int(-10, 10),
		};

		// In base al tipo di seme, ricalcoliamo i punti da sommare come bonus/malus
		if(this.type == SeedType.Glaciale)
		{
			additionalValues.resistenzaFreddo = RandomHelper.Int(10, 25);
			additionalValues.resistenzaCaldo = RandomHelper.Int(-15, 5);
		}
		if(this.type == SeedType.Magmatico)
		{
			additionalValues.resistenzaCaldo = RandomHelper.Int(10, 25);
			additionalValues.resistenzaFreddo = RandomHelper.Int(-15, 5);
		}
		if(this.type == SeedType.Rapido)
		{
			additionalValues.metabolismo = RandomHelper.Int(10, 25);
		}

		// TODO - Aggiungere gli altri tipi di seme


		// Calcola il moltiplicatore in base alla rarità
		float multiplier = 1.0f;

		if(this.rarity == SeedRarity.NonComune)
			multiplier = 1.1f;
		else if(this.rarity == SeedRarity.Raro)
			multiplier = 1.25f;
		else if(this.rarity == SeedRarity.Epico)
			multiplier = 1.5f;
		else if(this.rarity == SeedRarity.Leggendario)
			multiplier = 2.0f;

		// Applica il multiplier solo se i valori base sono maggiori di 0 (per non penalizzare ulteriormente i valori negativi)
		float ApplyMultiplier(float value, float multiplier)
		{
			return value > 0 ? value * multiplier : value;
		};

		// Calcola le stast finali facendo la somma dei valori massimi + (valori aggiuntivi moltiplicati per il moltiplicatore)
		var finalStats = new SeedStats()
		{
			vitalita			= (int)(maxValues.vitalita				+ ApplyMultiplier(additionalValues.vitalita, multiplier)),
			idratazione			= (int)(maxValues.idratazione			+ ApplyMultiplier(additionalValues.idratazione, multiplier)),
			resistenzaFreddo	= (int)(maxValues.resistenzaFreddo		+ ApplyMultiplier(additionalValues.resistenzaFreddo, multiplier)),
			resistenzaCaldo		= (int)(maxValues.resistenzaCaldo		+ ApplyMultiplier(additionalValues.resistenzaCaldo, multiplier)),	
			resistenzaParassiti = (int)(maxValues.resistenzaParassiti	+ ApplyMultiplier(additionalValues.resistenzaParassiti, multiplier)),	
			vegetazione			= (int)(maxValues.vegetazione			+ ApplyMultiplier(additionalValues.vegetazione, multiplier)),	
			metabolismo			= (int)(maxValues.metabolismo			+ ApplyMultiplier(additionalValues.metabolismo, multiplier)),	
			resistenzaVuoto		= (int)(maxValues.resistenzaVuoto		+ ApplyMultiplier(additionalValues.resistenzaVuoto, multiplier)),	
		};

		return finalStats;
	}

	private SeedStats GenStats(Seed seed1, Seed seed2)
	{
		return new SeedStats()
		{

		};
	}
}

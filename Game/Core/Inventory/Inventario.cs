using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public class InventorySaveData
{
    public Dictionary<SeedType, int> Seeds = new();
}

public class Inventario
{
	private Dictionary<SeedType, int> seedCounts = new();

	public static Inventario get()
	{
		if(Inventario.instance == null)
			Inventario.instance = new Inventario();

		return Inventario.instance;
	}

	private static Inventario instance = null;

	private Inventario()
	{
		seedCounts = new Dictionary<SeedType, int>();
	}

	public Dictionary<SeedType, int> GetAllSeeds()
	{
		return new Dictionary<SeedType, int>(seedCounts);
	}

	public void LoadFromData(InventorySaveData data)
	{
		seedCounts = new Dictionary<SeedType, int>(data.Seeds ?? new Dictionary<SeedType, int>());
	}

	public void AddSeed(SeedType type, int amount = 1)
	{
		if (seedCounts.ContainsKey(type))
		{
			seedCounts[type] += amount;
		}
		else
		{
			seedCounts[type] = amount;
		}
	}

	public bool RemoveSeed(SeedType type, int amount = 1)
	{
		if (seedCounts.ContainsKey(type) && seedCounts[type] >= amount)
		{
			seedCounts[type] -= amount;
			if (seedCounts[type] <= 0)
			{
				seedCounts.Remove(type);
			}
			return true;
		}
		return false;
	}

	// Legacy methods for compatibility
	public List<Seed> seeds => seedCounts.SelectMany(kvp => Enumerable.Repeat(new Seed(kvp.Key), kvp.Value)).ToList();

	public void Save()
	{
		// Add some default seeds for testing
		AddSeed(SeedType.Normale, 3);
        SaveHelper.Save("inventory.json", seedCounts);
	}

	public void Load()
	{
		var loaded = SaveHelper.Load<Dictionary<SeedType, int>>("inventory.json");
		if (loaded != null)
		{
			seedCounts = loaded;
		}
	}
}

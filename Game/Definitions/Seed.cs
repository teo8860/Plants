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

}

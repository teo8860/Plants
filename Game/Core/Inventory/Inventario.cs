using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public class Inventario
{
	public List<Seed> seeds = new();

	public static Inventario get()
	{
		if(Inventario.instance == null)
			Inventario.instance = new Inventario();

		return Inventario.instance;
	}

	private static Inventario instance = null;

	private Inventario()
	{ 
		seeds = new();
	}



	public void Save()
	{
		SaveHelper.Save("inventory.json", seeds);
	}

	public void Load()
	{ Console.WriteLine(SaveHelper.Load<List<Seed>>("inventory.json"));
		seeds = SaveHelper.Load<List<Seed>>("inventory.json");
	}


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Tools;

namespace Plants;



public class Inventario
{
    public List<Seed> seeds = new();
    public int maxSeeds = 100;

	public static Inventario get()
    {
        if (Inventario.instance == null)
            Inventario.instance = new Inventario();

        return Inventario.instance;
    }

    private static Inventario instance = null;

    private Inventario()
    {
        seeds = new();
    }


    public void AddSeed(Seed seed)
    {
        seeds.Add(seed);
        Save();
    }
    public void RemoveSeed(Seed seed)
    {
        seeds.Remove(seed);
    }

    public bool HasSeed(Seed seed)
    {
        return seeds.Contains(seed);
    }



    public List<Seed> GetAllSeeds()
    {
        return seeds;
    }

    public void Save()
    {
        SaveHelper.Save("inventory.json", seeds);
    }

    public void Load()
    {
        seeds = SaveHelper.Load<List<Seed>>("inventory.json");


        if (seeds == null)
            seeds = new();
        else
        {
            foreach (var item in seeds)
            {
                item.type = item.type;
            }
        }
    }


}

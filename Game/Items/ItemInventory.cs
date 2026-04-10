using System.Collections.Generic;

namespace Plants;

/// <summary>
/// Inventario degli oggetti posseduti dal giocatore (non equipaggiati).
/// Ogni stringa e' un ItemDefinition.Id.
/// </summary>
public class ItemInventory
{
    public List<string> items = new();

    private static ItemInventory instance = null;

    public static ItemInventory get()
    {
        if (instance == null)
            instance = new ItemInventory();
        return instance;
    }

    private ItemInventory() { }

    public void Add(string itemId)
    {
        items.Add(itemId);
        Save();
    }

    public void Remove(string itemId)
    {
        items.Remove(itemId);
        Save();
    }

    public bool Has(string itemId)
    {
        return items.Contains(itemId);
    }

    public List<string> GetAll() => items;

    public void Save()
    {
        SaveHelper.Save("items.json", items);
    }

    public void Load()
    {
        items = SaveHelper.Load<List<string>>("items.json");
        if (items == null)
            items = new();
    }
}

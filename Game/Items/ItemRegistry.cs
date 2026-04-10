using System.Collections.Generic;
using System.Linq;

namespace Plants;

/// <summary>
/// Registro statico di tutti i tipi di oggetto disponibili nel gioco.
/// Ogni ItemDefinition viene registrata qui all'avvio.
/// </summary>
public static class ItemRegistry
{
    private static readonly Dictionary<string, ItemDefinition> _items = new();

    public static void Register(ItemDefinition item)
    {
        _items[item.Id] = item;
    }

    public static ItemDefinition Get(string id)
    {
        return _items.TryGetValue(id, out var item) ? item : null;
    }

    public static IEnumerable<ItemDefinition> GetAll() => _items.Values;

    public static IEnumerable<ItemDefinition> GetByCategory(ItemCategory category)
    {
        return _items.Values.Where(i => i.Category == category);
    }

    public static void Init()
    {
        Register(new ItemFertilizzante());
        Register(new ItemScudoGelo());
        Register(new ItemAcceleratore());
        Register(new ItemParapioggia());
        Register(new ItemRadicatore());
        Register(new ItemFotosintesi());
    }
}

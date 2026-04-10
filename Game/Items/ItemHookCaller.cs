using System;
using System.Collections.Generic;

namespace Plants;

/// <summary>
/// Richiama gli hook degli oggetti equipaggiati sulla pianta corrente.
/// </summary>
public static class ItemHookCaller
{
    private static List<ItemDefinition> GetEquippedItems(Obj_Plant pianta)
    {
        var result = new List<ItemDefinition>();
        if (pianta.equippedItemIds == null) return result;

        foreach (var id in pianta.equippedItemIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            var def = ItemRegistry.Get(id);
            if (def != null)
                result.Add(def);
        }
        return result;
    }

    public static void CallOnStart(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnStart(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnStart error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnEnd(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnEnd(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnEnd error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnGrow(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnGrow(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnGrow error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnBranchNew(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnBranchNew(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnBranchNew error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnBranchGrow(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnBranchGrow(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnBranchGrow error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnLeafNew(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnLeafNew(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnLeafNew error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnLeafGrow(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnLeafGrow(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnLeafGrow error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnWeatherChange(Obj_Plant pianta, Weather newWeather)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnWeatherChange(pianta, newWeather); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnWeatherChange error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnWeatherRain(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnWeatherRain(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnWeatherRain error ({item.Id}): {ex.Message}"); }
        }
    }

    public static void CallOnWeatherSun(Obj_Plant pianta)
    {
        foreach (var item in GetEquippedItems(pianta))
        {
            try { item.OnWeatherSun(pianta); }
            catch (Exception ex) { Console.WriteLine($"[ItemHook] OnWeatherSun error ({item.Id}): {ex.Message}"); }
        }
    }
}

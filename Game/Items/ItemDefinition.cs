namespace Plants;

public enum ItemCategory
{
    Equipaggiabile,
    Cosmetico,
    Consumabile
}

/// <summary>
/// Classe base per tutti gli oggetti.
/// Ogni oggetto concreto eredita da questa e sovrascrive gli hook desiderati.
/// </summary>
public abstract class ItemDefinition
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual ItemCategory Category => ItemCategory.Equipaggiabile;

    // Hook richiamati durante la vita della pianta (solo Equipaggiabile)
    public virtual void OnStart(Obj_Plant pianta) { }
    public virtual void OnEnd(Obj_Plant pianta) { }
    public virtual void OnGrow(Obj_Plant pianta) { }
    public virtual void OnBranchNew(Obj_Plant pianta) { }
    public virtual void OnBranchGrow(Obj_Plant pianta) { }
    public virtual void OnLeafNew(Obj_Plant pianta) { }
    public virtual void OnLeafGrow(Obj_Plant pianta) { }
    public virtual void OnWeatherChange(Obj_Plant pianta, Weather newWeather) { }
    public virtual void OnWeatherRain(Obj_Plant pianta) { }
    public virtual void OnWeatherSun(Obj_Plant pianta) { }
}

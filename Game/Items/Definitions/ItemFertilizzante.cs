using System;

namespace Plants;

/// <summary>
/// Fertilizzante Magico: aumenta la crescita e rigenera salute ad ogni crescita.
/// </summary>
public class ItemFertilizzante : ItemDefinition
{
    public override string Id => "fertilizzante";
    public override string Name => "Fertilizzante Magico";
    public override string Description => "Un potente fertilizzante che accelera la crescita e rigenera un po' di salute ogni volta che la pianta cresce.";

    public override void OnGrow(Obj_Plant pianta)
    {
        pianta.Stats.Salute = Math.Min(1f, pianta.Stats.Salute + 0.002f);
    }

    public override void OnStart(Obj_Plant pianta)
    {
        pianta.Stats.Metabolismo = Math.Min(2f, pianta.Stats.Metabolismo + 0.1f);
        Console.WriteLine("[Item] Fertilizzante Magico attivato: metabolismo aumentato!");
    }
}

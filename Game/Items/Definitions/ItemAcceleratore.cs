using System;

namespace Plants;

/// <summary>
/// Acceleratore di Rami: i rami crescono piu' velocemente e ne spuntano di piu'.
/// </summary>
public class ItemAcceleratore : ItemDefinition
{
    public override string Id => "acceleratore_rami";
    public override string Name => "Acceleratore di Rami";
    public override string Description => "Un tonico speciale che stimola la crescita dei rami. Ogni volta che spunta un nuovo ramo, le foglie aumentano.";

    public override void OnBranchNew(Obj_Plant pianta)
    {
        pianta.Stats.FoglieAttuali = Math.Min(pianta.Stats.FoglieBase, pianta.Stats.FoglieAttuali + 5);
        Console.WriteLine("[Item] Acceleratore: nuovo ramo -> +5 foglie!");
    }

    public override void OnBranchGrow(Obj_Plant pianta)
    {
        if (RandomHelper.Float(0, 1) < 0.1f)
        {
            pianta.Stats.FoglieAttuali = Math.Min(pianta.Stats.FoglieBase, pianta.Stats.FoglieAttuali + 1);
        }
    }
}

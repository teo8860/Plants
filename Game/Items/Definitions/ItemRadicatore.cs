using System;

namespace Plants;

/// <summary>
/// Radicatore Profondo: migliora la stabilita' e la resistenza della pianta tramite le radici.
/// </summary>
public class ItemRadicatore : ItemDefinition
{
    public override string Id => "radicatore";
    public override string Name => "Radicatore Profondo";
    public override string Description => "Stimola la crescita radicale. All'inizio aumenta la vitalita' massima e durante la crescita riduce il consumo d'acqua.";

    public override void OnStart(Obj_Plant pianta)
    {
        // Scala 0-99: +2 vitalita (circa +20% del baseline 10).
        pianta.seedBonus.vitalita = Math.Min(SeedStatScaling.StatMax, pianta.seedBonus.vitalita + 2f);
        Console.WriteLine("[Item] Radicatore Profondo: vitalita' aumentata!");
    }

    public override void OnGrow(Obj_Plant pianta)
    {
        if (pianta.Stats.Idratazione < 0.3f)
        {
            pianta.Stats.Idratazione = Math.Min(1f, pianta.Stats.Idratazione + 0.005f);
        }
    }
}

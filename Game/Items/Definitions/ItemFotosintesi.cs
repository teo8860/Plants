using System;

namespace Plants;

/// <summary>
/// Amplificatore Fotosintesi: potenzia la fotosintesi durante il sole e aumenta le foglie.
/// </summary>
public class ItemFotosintesi : ItemDefinition
{
    public override string Id => "fotosintesi";
    public override string Name => "Amplificatore Fotosintesi";
    public override string Description => "Amplifica la fotosintesi. Con il sole la pianta produce piu' energia e le foglie crescono rigogliose.";

    public override void OnWeatherSun(Obj_Plant pianta)
    {
        pianta.Stats.Metabolismo = Math.Min(2f, pianta.Stats.Metabolismo + 0.005f);
    }

    public override void OnLeafNew(Obj_Plant pianta)
    {
        if (RandomHelper.Float(0, 1) < 0.2f)
        {
            pianta.Stats.FoglieAttuali = Math.Min(pianta.Stats.FoglieBase, pianta.Stats.FoglieAttuali + 1);
            Console.WriteLine("[Item] Fotosintesi: foglia bonus!");
        }
    }

    public override void OnLeafGrow(Obj_Plant pianta)
    {
        pianta.Stats.Ossigeno = Math.Min(1f, pianta.Stats.Ossigeno + 0.001f);
    }
}

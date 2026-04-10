using System;

namespace Plants;

/// <summary>
/// Scudo di Gelo: protegge dal freddo e dai danni da neve.
/// </summary>
public class ItemScudoGelo : ItemDefinition
{
    public override string Id => "scudo_gelo";
    public override string Name => "Scudo di Gelo";
    public override string Description => "Uno scudo magico che protegge la pianta dal freddo estremo. Durante la neve, rigenera salute invece di subire danni.";

    public override void OnStart(Obj_Plant pianta)
    {
        pianta.Stats.ResistenzaFreddo += 0.3f;
        Console.WriteLine("[Item] Scudo di Gelo attivato: resistenza freddo +0.3!");
    }

    public override void OnWeatherChange(Obj_Plant pianta, Weather newWeather)
    {
        if (newWeather == Weather.Snowy)
        {
            pianta.Stats.Salute = Math.Min(1f, pianta.Stats.Salute + 0.05f);
        }
    }
}

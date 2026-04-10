using System;

namespace Plants;

/// <summary>
/// Parapioggia Nutriente: la pioggia rigenera la pianta invece di essere neutra.
/// </summary>
public class ItemParapioggia : ItemDefinition
{
    public override string Id => "parapioggia";
    public override string Name => "Parapioggia Nutriente";
    public override string Description => "Trasforma la pioggia in nutrimento puro. Quando piove, la pianta si idrata piu' velocemente e rigenera salute.";

    public override void OnWeatherRain(Obj_Plant pianta)
    {
        pianta.Stats.Idratazione = Math.Min(1f, pianta.Stats.Idratazione + 0.01f);
        pianta.Stats.Salute = Math.Min(1f, pianta.Stats.Salute + 0.003f);
    }

    public override void OnWeatherChange(Obj_Plant pianta, Weather newWeather)
    {
        if (newWeather == Weather.Rainy || newWeather == Weather.Stormy)
        {
            Console.WriteLine("[Item] Parapioggia: pioggia in arrivo, nutrienti attivati!");
        }
    }
}

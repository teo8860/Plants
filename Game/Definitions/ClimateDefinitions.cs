using System.Collections.Generic;

namespace Plants;

/// <summary>
/// Enumerazioni e costanti numeriche legate al clima, alle fasi del giorno
/// e al meteo. Tutti i valori di bilanciamento che influenzano temperatura,
/// consumo risorse e soglie di danno vivono qui.
/// </summary>

// ─── Enums ───────────────────────────────────────────────────

public enum DayPhase
{
    Night,      // 0:00 - 5:59
    Dawn,       // 6:00 - 7:59
    Morning,    // 8:00 - 11:59
    Afternoon,  // 12:00 - 17:59
    Dusk,       // 18:00 - 19:59
    Evening     // 20:00 - 23:59
}

public enum Weather
{
    Sunny,
    Cloudy,
    Rainy,
    Stormy,
    Windy,
    Foggy,
    Snowy
}

// ─── Costanti e tabelle ──────────────────────────────────────

public static class ClimateDefinitions
{
    // === Soglie temperatura ===
    public const float TEMPERATURA_GELIDA     = -15.0f;
    public const float TEMPERATURA_FREDDA     =   2.0f;
    public const float TEMPERATURA_FRESCA     =  10.0f;
    public const float TEMPERATURA_IDEALE_MIN =  18.0f;
    public const float TEMPERATURA_IDEALE_MAX =  26.0f;
    public const float TEMPERATURA_CALDA      =  34.0f;
    public const float TEMPERATURA_TORRIDA    =  45.0f;

    // === Consumo risorse ===
    public const float CONSUMO_ACQUA_BASE         = 0.0012f;
    public const float CONSUMO_OSSIGENO_BASE       = 0.0006f;
    public const float CONSUMO_ENERGIA_BASE        = 0.0008f;
    public const float RIGENERAZIONE_SALUTE_BASE   = 0.0006f;

    // === Soglie stato pianta ===
    public const float SOGLIA_DISIDRATAZIONE   = 0.05f;
    public const float SOGLIA_SOFFOCAMENTO     = 0.15f;
    public const float SOGLIA_CRITICA_SALUTE   = 0.20f;
    public const float SOGLIA_FAME_ENERGIA     = 0.10f;

    // === Parassiti e foglie ===
    public const float PROBABILITA_PARASSITI_BASE = 0.0006f;
    public const float DANNO_PARASSITI_BASE       = 0.006f;
    public const float DROP_FOGLIE_BASE           = 0.004f;

    // === Temperatura base per fase del giorno (Terra) ===
    public static readonly Dictionary<DayPhase, float> TemperatureBaseFase = new()
    {
        { DayPhase.Night,     10f },
        { DayPhase.Dawn,      13f },
        { DayPhase.Morning,   18f },
        { DayPhase.Afternoon, 24f },
        { DayPhase.Dusk,      19f },
        { DayPhase.Evening,   14f }
    };

    // === Modifiche temperatura da meteo ===
    public static readonly Dictionary<Weather, float> WeatherTemperatureModifiers = new()
    {
        { Weather.Sunny,   +4f },
        { Weather.Cloudy,  -2f },
        { Weather.Rainy,   -5f },
        { Weather.Stormy,  -8f },
        { Weather.Windy,    0f },
        { Weather.Foggy,   -3f },
        { Weather.Snowy,  -15f }
    };

    // === Energia fotosintesi per fase ===
    public static readonly Dictionary<DayPhase, float> FotosinteisiEnergia = new()
    {
        { DayPhase.Night,      -0.0005f },
        { DayPhase.Dawn,        0.001f },
        { DayPhase.Morning,     0.002f },
        { DayPhase.Afternoon,   0.0025f },
        { DayPhase.Dusk,        0.001f },
        { DayPhase.Evening,    -0.0005f }
    };

    // === Moltiplicatori fotosintesi da meteo ===
    public static readonly Dictionary<Weather, float> WeatherFotosinteisiMult = new()
    {
        { Weather.Sunny,   1.0f },
        { Weather.Cloudy,  0.5f },
        { Weather.Rainy,   0.35f },
        { Weather.Stormy,  0.15f },
        { Weather.Windy,   1.0f },
        { Weather.Foggy,   0.3f },
        { Weather.Snowy,   0.4f }
    };

    // === Durata meteo (minuti) ===
    public const int WEATHER_DURATION_MINUTES = 30;
}

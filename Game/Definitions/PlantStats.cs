using System;

namespace Plants;

public class SeedStats
{
    public float vitalita  { get; set; }
    public float idratazione { get; set; }
    public float resistenzaFreddo { get; set; }
    public float resistenzaCaldo { get; set; }
    public float resistenzaParassiti { get; set; }
    public float vegetazione { get; set; }
    public float metabolismo { get; set; }
    public float resistenzaVuoto { get; set; }
    public int fusionCount { get; set; }

    public SeedStats()
    {
        vitalita = 1.0f;
        idratazione = 1.0f;
        resistenzaFreddo = 0.3f;
        resistenzaCaldo = 0.3f;
        resistenzaParassiti = 0.2f;
        vegetazione = 1.0f;
        metabolismo = 1.0f;
        resistenzaVuoto = 0.1f;
        fusionCount = 0;
    }
}

public class PlantStats
{
    public float Salute { get; set; } = 1f;
    public float Idratazione { get; set; } = 0.1f;
    public float Ossigeno { get; set; } = 1.0f;
    public float Metabolismo { get; set; } = 1.0f;
    public float Temperatura { get; set; } = 20.0f;
    public float ResistenzaFreddo { get; set; } = 0.0f;
    public float ResistenzaCaldo { get; set; } = 0.0f;
    public float ResistenzaParassiti { get; set; } = 0.0f;
    public float ResistenzaVuoto { get; set; } = 0.0f;
    public int FoglieBase { get; set; } = 1000;
    public int FoglieAttuali { get; set; } = 0;
    public int FoglieAccumulate { get; set; } = 0;
    public float DropRateFoglie { get; set; } = 0.003f;
    public float Altezza { get; set; } = 0.0f;
    public float AltezzaMassima { get; set; } = 5000.0f;
    public bool Infestata { get; set; } = false;
    public float IntensitaInfestazione { get; set; } = 0.0f;

    public float EffectiveMaxHeight => AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
   
    public const float SALUTE_MIN = 0f;
    public const float SALUTE_MAX = 1f;

    public void ClampAllValues()
    {
        Salute = Math.Clamp(Salute, SALUTE_MIN, SALUTE_MAX);
        Idratazione = Math.Clamp(Idratazione, 0f, 1f);
        Ossigeno = Math.Clamp(Ossigeno, 0f, 1f);
        Metabolismo = Math.Clamp(Metabolismo, 0f, 2f);
        FoglieAttuali = Math.Clamp(FoglieAttuali, 0, FoglieBase);
        IntensitaInfestazione = Math.Clamp(IntensitaInfestazione, 0f, 1f);
    }
}
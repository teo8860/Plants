using System;
using System.Collections.Generic;

namespace Plants;

public class GameLogicPianta
{
    private Obj_Plant pianta;
    public int contatoreSecondi = 0;
    public PlantEventSystem EventSystem { get; set; }

    public const float CONSUMO_ACQUA_BASE = 0.0012f;
    public const float CONSUMO_OSSIGENO_BASE = 0.0006f;
    public const float CONSUMO_ENERGIA_BASE = 0.0008f;
    public const float RIGENERAZIONE_SALUTE_BASE = 0.0006f;

    public const float SOGLIA_DISIDRATAZIONE = 0.85f;
    public const float SOGLIA_SOFFOCAMENTO = 0.15f;
    public const float SOGLIA_CRITICA_SALUTE = 0.20f;
    public const float SOGLIA_FAME_ENERGIA = 0.10f;

    public const float TEMPERATURA_GELIDA = -15.0f;
    public const float TEMPERATURA_FREDDA = 2.0f;
    public const float TEMPERATURA_FRESCA = 10.0f;
    public const float TEMPERATURA_IDEALE_MIN = 18.0f;
    public const float TEMPERATURA_IDEALE_MAX = 26.0f;
    public const float TEMPERATURA_CALDA = 34.0f;
    public const float TEMPERATURA_TORRIDA = 45.0f;

    public const float PROBABILITA_PARASSITI_BASE = 0.0006f;
    public const float DANNO_PARASSITI_BASE = 0.006f;
    public const float DROP_FOGLIE_BASE = 0.004f;

    public GameLogicPianta(Obj_Plant Pianta)
    {
        pianta = Pianta;
        EventSystem = new PlantEventSystem(this);
    }

    private static readonly Dictionary<DayPhase, float> TemperatureBaseFase = new()
    {
        { DayPhase.Night, 10f },
        { DayPhase.Dawn, 13f },
        { DayPhase.Morning, 18f },
        { DayPhase.Afternoon, 24f },
        { DayPhase.Dusk, 19f },
        { DayPhase.Evening, 14f }
    };

    public SeedStats bonus => pianta.seedBonus;
    public PlantStats stats => pianta.Stats;

    public float VitalitaMax => 1.0f * bonus.vitalita;
    public float ConsumoAcquaMult => bonus.idratazione;

    public float ResistenzaFreddoTotale => Math.Clamp(stats.ResistenzaFreddo + bonus.resistenzaFreddo, -1f, 1f);
    public float ResistenzaCaldoTotale => Math.Clamp(stats.ResistenzaCaldo + bonus.resistenzaCaldo, -1f, 1f);
    public float ResistenzaParassitiTotale => Math.Clamp(stats.ResistenzaParassiti + bonus.resistenzaParassiti, -1f, 1f);

    public int FoglieMassime => (int)(stats.FoglieBase * bonus.vegetazione);
    public float MetabolismoEffettivo => Math.Clamp(stats.Metabolismo * bonus.metabolismo, 0.1f, 3f);
    public float PercentualeSalute => stats.Salute / VitalitaMax;

    public bool IsViva => stats.Salute > 0;
    public bool IsCritica => PercentualeSalute < SOGLIA_CRITICA_SALUTE;
    public bool IsDisidratata => stats.Idratazione < SOGLIA_DISIDRATAZIONE;
    public bool IsSoffocamento => stats.Ossigeno < SOGLIA_SOFFOCAMENTO;
    public bool IsFame => stats.Metabolismo < SOGLIA_FAME_ENERGIA;

    public bool IsGelida => stats.Temperatura <= TEMPERATURA_GELIDA;
    public bool IsFredda => stats.Temperatura < TEMPERATURA_FREDDA;
    public bool IsCalda => stats.Temperatura > TEMPERATURA_CALDA;
    public bool IsTorrida => stats.Temperatura >= TEMPERATURA_TORRIDA;
    public bool IsTemperaturaIdeale => stats.Temperatura >= TEMPERATURA_IDEALE_MIN && stats.Temperatura <= TEMPERATURA_IDEALE_MAX;

    public SeedStats GetSeedBonus() => bonus;

    public float CalcolaTemperaturaAmbientale(DayPhase fase, Weather meteo, WorldModifier worldMod)
    {
        float tempBase = TemperatureBaseFase.GetValueOrDefault(fase, 20f);

        if (worldMod.IsMeteoOn)
        {
            tempBase += meteo switch
            {
                Weather.Sunny => 4f,
                Weather.Cloudy => -2f,
                Weather.Rainy => -5f,
                Weather.Stormy => -8f,
                Weather.Foggy => -3f,
                Weather.Snowy => -15f,
                _ => 0f
            };
        }

        tempBase += worldMod.TemperatureModifier;
        tempBase += RandomHelper.Float(-1f, 1f);

        return tempBase;
    }

    public void AggiornaTemperatura(DayPhase fase, Weather meteo, WorldModifier worldMod)
    {
        float tempAmbientale = CalcolaTemperaturaAmbientale(fase, meteo, worldMod);
        float differenza = tempAmbientale - stats.Temperatura;

        float velocitaAdattamento = 0.12f;

        if (differenza < 0 && ResistenzaFreddoTotale > 0)
        {
            velocitaAdattamento *= (1f - ResistenzaFreddoTotale * 0.5f);
        }
        else if (differenza > 0 && ResistenzaCaldoTotale > 0)
        {
            velocitaAdattamento *= (1f - ResistenzaCaldoTotale * 0.5f);
        }

        stats.Temperatura += differenza * velocitaAdattamento;
    }

    public float GetMoltiplicatoreCrescitaTemperatura()
    {
        float temp = stats.Temperatura;

        if (temp <= TEMPERATURA_GELIDA) return 0f;
        if (temp < TEMPERATURA_FREDDA)
        {
            float t = (temp - TEMPERATURA_GELIDA) / (TEMPERATURA_FREDDA - TEMPERATURA_GELIDA);
            return 0.1f + t * 0.3f;
        }
        if (temp < TEMPERATURA_FRESCA)
        {
            float t = (temp - TEMPERATURA_FREDDA) / (TEMPERATURA_FRESCA - TEMPERATURA_FREDDA);
            return 0.4f + t * 0.3f;
        }
        if (temp >= TEMPERATURA_IDEALE_MIN && temp <= TEMPERATURA_IDEALE_MAX)
        {
            return 1.0f + RandomHelper.Float(0f, 0.05f);
        }
        if (temp < TEMPERATURA_IDEALE_MIN)
        {
            float t = (temp - TEMPERATURA_FRESCA) / (TEMPERATURA_IDEALE_MIN - TEMPERATURA_FRESCA);
            return 0.7f + t * 0.3f;
        }
        if (temp <= TEMPERATURA_CALDA)
        {
            float t = (temp - TEMPERATURA_IDEALE_MAX) / (TEMPERATURA_CALDA - TEMPERATURA_IDEALE_MAX);
            return 1.0f - t * 0.3f;
        }
        if (temp < TEMPERATURA_TORRIDA)
        {
            float t = (temp - TEMPERATURA_CALDA) / (TEMPERATURA_TORRIDA - TEMPERATURA_CALDA);
            return 0.7f - t * 0.5f;
        }
        return Math.Max(0f, 0.2f - (temp - TEMPERATURA_TORRIDA) * 0.02f);
    }

    public void ApplicaDanniTemperatura(WorldModifier worldMod)
    {
        float temp = stats.Temperatura;
        float danno = 0f;
        string causa = "";

        float moltiplicatoreMondo = worldMod.TemperatureDamage;

        if (temp <= TEMPERATURA_GELIDA)
        {
            float intensita = Math.Abs(temp - TEMPERATURA_GELIDA) / 20f;
            danno = intensita * 0.025f * (1f - ResistenzaFreddoTotale) * moltiplicatoreMondo;
            causa = "gelo";
        }
        else if (temp < TEMPERATURA_FREDDA)
        {
            float intensita = (TEMPERATURA_FREDDA - temp) / (TEMPERATURA_FREDDA - TEMPERATURA_GELIDA);
            danno = intensita * 0.01f * (1f - ResistenzaFreddoTotale) * moltiplicatoreMondo;
            causa = "freddo";
        }
        else if (temp >= TEMPERATURA_TORRIDA)
        {
            float intensita = (temp - TEMPERATURA_TORRIDA) / 20f;
            danno = intensita * 0.025f * (1f - ResistenzaCaldoTotale) * moltiplicatoreMondo;
            causa = "calore estremo";
        }
        else if (temp > TEMPERATURA_CALDA)
        {
            float intensita = (temp - TEMPERATURA_CALDA) / (TEMPERATURA_TORRIDA - TEMPERATURA_CALDA);
            danno = intensita * 0.008f * (1f - ResistenzaCaldoTotale) * moltiplicatoreMondo;
            causa = "caldo";
        }

        if (danno > 0)
        {
            ApplicaDanno(Math.Max(0, danno), causa);
        }
    }

    public float CalcolaConsumoAcqua(WorldModifier worldMod)
    {
        float consumo = CONSUMO_ACQUA_BASE;

        consumo *= ConsumoAcquaMult;
        consumo *= worldMod.WaterConsumption;
        consumo *= (0.5f + MetabolismoEffettivo * 0.5f);

        float temp = stats.Temperatura;
        if (temp <= TEMPERATURA_FREDDA) consumo *= 0.5f;
        else if (temp < TEMPERATURA_IDEALE_MIN) consumo *= 0.7f;
        else if (temp <= TEMPERATURA_IDEALE_MAX) consumo *= 1.0f;
        else if (temp <= TEMPERATURA_CALDA) consumo *= 1.3f;
        else if (temp < TEMPERATURA_TORRIDA) consumo *= 1.8f;
        else consumo *= 2.5f;

        float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, stats.FoglieBase);
        consumo *= (0.5f + rapportoFoglie * 0.5f);

        return consumo;
    }

    public bool AggiornaIdratazione(WorldModifier worldMod, Weather meteo)
    {
        float consumo = CalcolaConsumoAcqua(worldMod);

        if (worldMod.IsMeteoOn && (meteo == Weather.Rainy || meteo == Weather.Stormy))
        {
            float reidratazione = 0.003f * worldMod.HydrationFromRain;

            if (worldMod.HydrationFromRain < 0)
            {
                ApplicaDanno(Math.Abs(worldMod.HydrationFromRain) * 0.01f, "pioggia acida");
            }
            else
            {
                stats.Idratazione = Math.Min(1.0f, stats.Idratazione + reidratazione);
            }
        }

        stats.Idratazione = Math.Max(0, stats.Idratazione - consumo);

        Game.pianta.ControlloCrescita();

        if (IsDisidratata)
        {
            contatoreSecondi++;
            if (contatoreSecondi >= 3)
            {
                float danno = (SOGLIA_DISIDRATAZIONE - stats.Idratazione) * 0.012f;
                ApplicaDanno(danno, "disidratazione");
                contatoreSecondi = 0;
                return true;
            }
        }

        return false;
    }

    public void Annaffia(float quantita)
    {
        stats.Idratazione = Math.Clamp(stats.Idratazione + quantita, 0f, 1f);
    }

    public float CalcolaProbabilitaInfestazione(Weather meteo, WorldModifier worldMod)
    {
        float probabilita = PROBABILITA_PARASSITI_BASE;

        probabilita *= worldMod.ParasiteChance;

        if (worldMod.IsMeteoOn && (meteo == Weather.Rainy || meteo == Weather.Foggy))
            probabilita *= 1.5f;

        if (IsDisidratata) probabilita *= 1.3f;
        if (IsCritica) probabilita *= 1.5f;
        if (IsFredda || IsTorrida) probabilita *= 0.3f;

        float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, stats.FoglieBase);
        probabilita *= (0.5f + rapportoFoglie * 0.5f);

        float moltiplicatore = 1f - ResistenzaParassitiTotale;
        return Math.Clamp(probabilita * moltiplicatore, 0f, 1f);
    }

    public float CalcolaDannoParassiti(WorldModifier worldMod)
    {
        if (!stats.Infestata) return 0f;

        float danno = DANNO_PARASSITI_BASE * stats.IntensitaInfestazione;
        danno *= worldMod.ParasiteDamage;
        danno *= (1f - ResistenzaParassitiTotale);

        return Math.Max(0, danno);
    }

    public void AggiornaParassiti(Weather meteo, WorldModifier worldMod)
    {
        if (!stats.Infestata)
        {
            float probabilita = CalcolaProbabilitaInfestazione(meteo, worldMod);
            if (RandomHelper.Float(0, 1) < probabilita)
            {
                stats.Infestata = true;
                stats.IntensitaInfestazione = RandomHelper.Float(0.1f, 0.25f);
            }
        }
        else
        {
            float peggioramento = 0.001f * (1f + worldMod.ParasiteChance * 0.5f);
            stats.IntensitaInfestazione = Math.Min(1f, stats.IntensitaInfestazione + peggioramento);

            float danno = CalcolaDannoParassiti(worldMod);
            if (danno > 0)
            {
                ApplicaDanno(danno, "parassiti");

                if (RandomHelper.Float(0, 1) < stats.IntensitaInfestazione * 0.1f * worldMod.ParasiteDamage)
                {
                    PerdiFoglia();
                }
            }
        }
    }

    public void CuraParassiti(float efficacia = 1.0f)
    {
        stats.IntensitaInfestazione = Math.Max(0, stats.IntensitaInfestazione - efficacia);
        if (stats.IntensitaInfestazione <= 0)
        {
            stats.Infestata = false;
            stats.IntensitaInfestazione = 0;
        }
    }

    public void AggiornaOssigeno(WorldModifier worldMod)
    {
        float consumoBase = CONSUMO_OSSIGENO_BASE * worldMod.OxygenConsumption;

        if (worldMod.OxygenLevel < 0.5f)
        {
            float deficit = 1f - worldMod.OxygenLevel;
            stats.Ossigeno = Math.Max(0, stats.Ossigeno - consumoBase * deficit * 2f);

            if (IsSoffocamento)
            {
                float danno = (SOGLIA_SOFFOCAMENTO - stats.Ossigeno) * 0.02f;
                ApplicaDanno(danno, "soffocamento");
            }
        }
        else
        {
            float recupero = 0.004f * worldMod.OxygenLevel;
            stats.Ossigeno = Math.Min(1f, stats.Ossigeno + recupero);
        }
    }

    public void FornisciOssigeno(float quantita)
    {
        stats.Ossigeno = Math.Clamp(stats.Ossigeno + quantita, 0f, 1f);
    }

    public float CalcolaFotosintesi(DayPhase fase, Weather meteo, WorldModifier worldMod)
    {
        float energiaBase = fase switch
        {
            DayPhase.Morning => 0.002f,
            DayPhase.Afternoon => 0.0025f,
            DayPhase.Dawn => 0.001f,
            DayPhase.Dusk => 0.001f,
            _ => -0.0005f
        };

        energiaBase *= worldMod.SolarMultiplier;

        if (worldMod.IsMeteoOn)
        {
            energiaBase *= meteo switch
            {
                Weather.Foggy => 0.3f,
                Weather.Cloudy => 0.5f,
                Weather.Rainy => 0.35f,
                Weather.Stormy => 0.15f,
                Weather.Snowy => 0.4f,
                _ => 1.0f
            };
        }

        if (IsFredda) energiaBase *= 0.5f;
        else if (IsTorrida) energiaBase *= 0.3f;
        else if (IsCalda) energiaBase *= 0.7f;

        float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, FoglieMassime);
        energiaBase *= (0.3f + rapportoFoglie * 0.7f);

        energiaBase -= CONSUMO_ENERGIA_BASE * (worldMod.EnergyDrain - 1f);

        return energiaBase;
    }

    public void AggiornaMetabolismo(DayPhase fase, Weather meteo, WorldModifier worldMod)
    {
        float energia = CalcolaFotosintesi(fase, meteo, worldMod);
        stats.Metabolismo = Math.Clamp(stats.Metabolismo + energia, 0f, 1f);

        if (IsFame && worldMod.EnergyDrain > 1.5f)
        {
            ApplicaDanno(0.003f * worldMod.EnergyDrain, "fame energetica");
        }
    }

    public float CalcolaVelocitaCrescita(WorldModifier worldMod)
    {
        float media = (stats.Idratazione + stats.Metabolismo + stats.Ossigeno) / 3f;

        float velocita = media * MetabolismoEffettivo;
        velocita *= worldMod.GrowthRateMultiplier;
        velocita *= GetMoltiplicatoreCrescitaTemperatura();

        if (PercentualeSalute < 0.5f)
        {
            velocita *= PercentualeSalute * 2f;
        }

        if (IsCritica || IsDisidratata || IsSoffocamento || IsFame)
        {
            velocita = 0;
        }

        return Math.Max(0, velocita);
    }

    public float CalcolaProbabilitaPerdiaFoglia(Weather meteo, WorldModifier worldMod)
    {
        float probabilita = DROP_FOGLIE_BASE;

        probabilita *= worldMod.LeafDropRate;

        if (IsDisidratata) probabilita *= 2f;
        if (IsCritica) probabilita *= 1.8f;

        if (meteo == Weather.Stormy)
        {
            probabilita *= 3f * worldMod.StormDamage;
        }

        if (stats.Infestata)
        {
            probabilita *= (1f + stats.IntensitaInfestazione * worldMod.ParasiteDamage);
        }

        if (IsGelida) probabilita *= 4f;
        else if (IsFredda) probabilita *= 2f;
        else if (IsTorrida) probabilita *= 3.5f;
        else if (IsCalda) probabilita *= 1.5f;

        return probabilita;
    }

    public void AggiornaFoglie(Weather meteo, WorldModifier worldMod)
    {
        float probabilita = CalcolaProbabilitaPerdiaFoglia(meteo, worldMod);

        if (RandomHelper.Float(0, 1) < probabilita && stats.FoglieAttuali > 0)
        {
            PerdiFoglia();
        }
    }

    public void PerdiFoglia()
    {
        if (stats.FoglieAttuali > 0)
        {
            stats.FoglieAttuali--;
        }
    }

    public float PercentualeFoglie => (float)stats.FoglieAttuali / Math.Max(1, FoglieMassime);

    public void ApplicaDanno(float danno, string causa = "")
    {
        stats.Salute = Math.Max(0, stats.Salute - danno);
    }

    public void Rigenera(float quantita)
    {
        stats.Salute = Math.Min(VitalitaMax, stats.Salute + quantita);
    }

    public float CalcolaRigenerazioneNaturale(WorldModifier worldMod)
    {
        if (IsDisidratata || IsSoffocamento || stats.Infestata || IsFame)
            return 0f;

        if (IsFredda || IsTorrida)
            return 0f;

        float rigenera = RIGENERAZIONE_SALUTE_BASE;
        rigenera *= (stats.Idratazione + stats.Metabolismo + stats.Ossigeno) / 3f;
        rigenera *= worldMod.HealthRegen;

        if (IsTemperaturaIdeale)
            rigenera *= 1.3f;

        return rigenera;
    }

    public void AggiornaRigenerazione(WorldModifier worldMod)
    {
        float rigenera = CalcolaRigenerazioneNaturale(worldMod);
        if (rigenera > 0 && stats.Salute < VitalitaMax)
        {
            Rigenera(rigenera);
        }
    }

    public void ApplicaDanniTempesta(Weather meteo, WorldModifier worldMod)
    {
        if (!worldMod.IsMeteoOn) return;
        if (meteo != Weather.Stormy) return;

        float probabilita = 0.1f * worldMod.StormChance;

        if (RandomHelper.Float(0, 1) < probabilita)
        {
            float danno = 0.01f * worldMod.StormDamage;
            ApplicaDanno(danno, "tempesta");

            if (RandomHelper.Float(0, 1) < 0.3f * worldMod.StormDamage)
            {
                PerdiFoglia();
            }
        }
    }

    public void AggiornaTutto(DayPhase fase, Weather meteo, WorldModifier worldMod)
    {
        if (!IsViva)
        {
            if (stats.FoglieAttuali > 0)
            {
                LeafHarvestSystem.HarvestAndShow("Pianta morta");
            }
            return;
        }

        AggiornaTemperatura(fase, meteo, worldMod);
        ApplicaDanniTemperatura(worldMod);

        AggiornaIdratazione(worldMod, meteo);
        AggiornaOssigeno(worldMod);
        AggiornaMetabolismo(fase, meteo, worldMod);

        AggiornaParassiti(meteo, worldMod);
        AggiornaFoglie(meteo, worldMod);
        ApplicaDanniTempesta(meteo, worldMod);

        //AggiornaRigenerazione(worldMod);

        stats.ClampAllValues();
        EventSystem?.CheckAndFireEvents();

    }

    public void Reset()
    {
        stats.Salute = VitalitaMax;
        stats.Idratazione = 0.5f;
        stats.Ossigeno = 1.0f;
        stats.Metabolismo = 0.8f;
        stats.Temperatura = 20.0f;
        stats.FoglieAttuali = 0;
        stats.Altezza = 0f;
        stats.Infestata = false;
        stats.IntensitaInfestazione = 0f;
        contatoreSecondi = 0;
    }

    public string GetRiepilogo()
    {
        if (!IsViva)
            return "La pianta è morta.";

        if (Room.GetActiveId() != Game.room_main.id)
            return "";

        var worldMod = WorldManager.GetCurrentModifiers();
        string status = "";

        if (IsCritica) status += "CRITICO ";
        if (IsDisidratata) status += "ASSETATO ";
        if (IsSoffocamento) status += "SOFFOCA ";
        if (IsFame) status += "FAME ";
        if (stats.Infestata) status += $"INFESTATO ({stats.IntensitaInfestazione:P0}) ";

        string difficolta = WorldManager.GetDifficultyName(worldMod.Difficulty);

        return $"[{SeedDataType.GetName(Game.pianta.TipoSeme)}] - {difficolta}\n" +
               $"Salute: {PercentualeSalute:P0}\n" +
               $"Idratazione: {stats.Idratazione:P0}\n" +
               $"Ossigeno: {stats.Ossigeno:P0}\n" +
               $"Energia: {stats.Metabolismo:P0}\n" +
               $"Temp: {stats.Temperatura:F1}°C ({GetStatoTemperatura()})\n" +
               $"Foglie: {stats.FoglieAttuali}/{FoglieMassime}\n" +
               $"Altezza: {stats.Altezza:F0}m\n" +
               (string.IsNullOrEmpty(status) ? "" : $"\n{status}");
    }

    public string GetStatoTemperatura()
    {
        if (IsGelida) return "GELO";
        if (IsFredda) return "Freddo";
        if (stats.Temperatura < TEMPERATURA_IDEALE_MIN) return "Fresco";
        if (IsTemperaturaIdeale) return "Ideale";
        if (stats.Temperatura <= TEMPERATURA_CALDA) return "Caldo";
        if (IsTorrida) return "TORRIDO";
        return "Normale";
    }
}
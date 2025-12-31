using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants
{
    public class GameLogicPianta
    {
        public int contatoreSecondi = 0;

        public const float CONSUMO_ACQUA_BASE = 0.002f;
        public const float CONSUMO_OSSIGENO_BASE = 0.001f;
        public const float RIGENERAZIONE_SALUTE_BASE = 0.001f;
        public const float CRESCITA_BASE = 0.5f;
        public const float SOGLIA_DISIDRATAZIONE = 0.2f;
        public const float SOGLIA_SOFFOCAMENTO = 0.3f;

        public const float TEMPERATURA_GELIDA = 0.0f;
        public const float TEMPERATURA_FREDDA = 10.0f;
        public const float TEMPERATURA_FRESCA = 15.0f;
        public const float TEMPERATURA_IDEALE_MIN = 18.0f;
        public const float TEMPERATURA_IDEALE_MAX = 25.0f;
        public const float TEMPERATURA_CALDA = 30.0f;
        public const float TEMPERATURA_TORRIDA = 38.0f;

        private static readonly Dictionary<DayPhase, float> TemperatureBaseFase = new()
        {
            { DayPhase.Night, 8f },
            { DayPhase.Dawn, 12f },
            { DayPhase.Morning, 18f },
            { DayPhase.Afternoon, 26f },
            { DayPhase.Dusk, 20f },
            { DayPhase.Evening, 14f }
        };

        public SeedBonus bonus => Game.pianta.seedBonus;
        public PlantStats stats => Game.pianta.Stats;

        public float VitalitaMax => 1.0f * bonus.Vitalita;
        public float ConsumoAcquaMult => bonus.Idratazione;
        public float ResistenzaFreddoTotale => Math.Clamp(stats.ResistenzaFreddo + bonus.ResistenzaFreddo, -1f, 1f);
        public float ResistenzaCaldoTotale => Math.Clamp(stats.ResistenzaCaldo + bonus.ResistenzaCaldo, -1f, 1f);
        public float ResistenzaParassitiTotale => Math.Clamp(stats.ResistenzaParassiti + bonus.ResistenzaParassiti, -1f, 1f);
        public int FoglieMassime => (int)(stats.FoglieBase * bonus.Vegetazione);
        public float MetabolismoEffettivo => stats.Metabolismo * bonus.Metabolismo;
        public float PercentualeSalute => stats.Salute / VitalitaMax;

        public bool IsViva => stats.Salute > 0;
        public bool IsCritica => PercentualeSalute < 0.25f;
        public bool IsDisidratata => stats.Idratazione < SOGLIA_DISIDRATAZIONE;
        public bool IsSoffocamento => stats.Ossigeno < SOGLIA_SOFFOCAMENTO;

        public bool IsGelida => stats.Temperatura <= TEMPERATURA_GELIDA;
        public bool IsFredda => stats.Temperatura < TEMPERATURA_FREDDA;
        public bool IsCalda => stats.Temperatura > TEMPERATURA_CALDA;
        public bool IsTorrida => stats.Temperatura >= TEMPERATURA_TORRIDA;
        public bool IsTemperaturaIdeale => stats.Temperatura >= TEMPERATURA_IDEALE_MIN && stats.Temperatura <= TEMPERATURA_IDEALE_MAX;

        public SeedBonus GetSeedBonus() => bonus;


        public float CalcolaTemperaturaAmbientale(DayPhase fase, Weather meteo, WorldModifier worldMod)
        {
            float tempBase = TemperatureBaseFase.GetValueOrDefault(fase, 20f);

            if (worldMod.IsMeteoOn)
            {
                tempBase += meteo switch
                {
                    Weather.Sunny => 4f,
                    Weather.Cloudy => -2f,
                    Weather.Rainy => -6f,
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

            float velocitaAdattamento = 0.3f;

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

            if (temp <= TEMPERATURA_GELIDA)
                return 0f; 

            if (temp < TEMPERATURA_FREDDA)
            {
                float t = (temp - TEMPERATURA_GELIDA) / (TEMPERATURA_FREDDA - TEMPERATURA_GELIDA);
                return 0.2f + t * 0.3f;
            }

            if (temp < TEMPERATURA_FRESCA)
            {
                float t = (temp - TEMPERATURA_FREDDA) / (TEMPERATURA_FRESCA - TEMPERATURA_FREDDA);
                return 0.5f + t * 0.3f;
            }

            if (temp >= TEMPERATURA_IDEALE_MIN && temp <= TEMPERATURA_IDEALE_MAX)
            {
                return 1.0f + RandomHelper.Float(0f, 0.1f);
            }

            if (temp < TEMPERATURA_IDEALE_MIN)
            {
                float t = (temp - TEMPERATURA_FRESCA) / (TEMPERATURA_IDEALE_MIN - TEMPERATURA_FRESCA);
                return 0.8f + t * 0.2f;
            }

            if (temp <= TEMPERATURA_CALDA)
            {
                float t = (temp - TEMPERATURA_IDEALE_MAX) / (TEMPERATURA_CALDA - TEMPERATURA_IDEALE_MAX);
                return 1.0f - t * 0.2f;
            }

            if (temp < TEMPERATURA_TORRIDA)
            {
                float t = (temp - TEMPERATURA_CALDA) / (TEMPERATURA_TORRIDA - TEMPERATURA_CALDA);
                return 0.8f - t * 0.4f;
            }

            return Math.Max(0f, 0.4f - (temp - TEMPERATURA_TORRIDA) * 0.05f);
        }

        public float GetMoltiplicatoreAcquaTemperatura()
        {
            float temp = stats.Temperatura;

            if (temp <= TEMPERATURA_FREDDA)
                return 0.6f;

            if (temp < TEMPERATURA_IDEALE_MIN)
                return 0.8f;

            if (temp <= TEMPERATURA_IDEALE_MAX)
                return 1.0f;

            if (temp <= TEMPERATURA_CALDA)
            {
                float t = (temp - TEMPERATURA_IDEALE_MAX) / (TEMPERATURA_CALDA - TEMPERATURA_IDEALE_MAX);
                return 1.0f + t * 0.5f;
            }

            if (temp < TEMPERATURA_TORRIDA)
            {
                float t = (temp - TEMPERATURA_CALDA) / (TEMPERATURA_TORRIDA - TEMPERATURA_CALDA);
                return 1.5f + t * 1.0f;
            }

            return 2.5f + (temp - TEMPERATURA_TORRIDA) * 0.1f;
        }

        public float GetMoltiplicatoreFoglieTemperatura()
        {
            float temp = stats.Temperatura;

            if (temp <= TEMPERATURA_GELIDA)
                return 5.0f; 

            if (temp < TEMPERATURA_FREDDA)
            {
                float t = (TEMPERATURA_FREDDA - temp) / TEMPERATURA_FREDDA;
                return 1.0f + t * 3.0f;
            }

            if (temp >= TEMPERATURA_IDEALE_MIN && temp <= TEMPERATURA_IDEALE_MAX)
                return 0.5f;

            if (temp > TEMPERATURA_CALDA)
            {
                float t = (temp - TEMPERATURA_CALDA) / 10f;
                return 1.0f + t * 2.0f;
            }

            if (temp >= TEMPERATURA_TORRIDA)
                return 4.0f;

            return 1.0f;
        }

        public void ApplicaDanniTemperatura()
        {
            float temp = stats.Temperatura;
            float danno = 0f;
            string causa = "";

            if (temp <= TEMPERATURA_GELIDA)
            {
                float intensita = Math.Abs(temp) / 10f;
                danno = intensita * 0.03f * (1f - ResistenzaFreddoTotale);
                causa = "gelo";
            }
            else if (temp < TEMPERATURA_FREDDA)
            {
                float intensita = (TEMPERATURA_FREDDA - temp) / TEMPERATURA_FREDDA;
                danno = intensita * 0.01f * (1f - ResistenzaFreddoTotale);
                causa = "freddo";
            }
            else if (temp >= TEMPERATURA_TORRIDA)
            {
                float intensita = (temp - TEMPERATURA_TORRIDA) / 10f;
                danno = intensita * 0.03f * (1f - ResistenzaCaldoTotale);
                causa = "calore";
            }
            else if (temp > TEMPERATURA_CALDA)
            {
                float intensita = (temp - TEMPERATURA_CALDA) / (TEMPERATURA_TORRIDA - TEMPERATURA_CALDA);
                danno = intensita * 0.005f * (1f - ResistenzaCaldoTotale);
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

            consumo *= GetMoltiplicatoreAcquaTemperatura();

            float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, stats.FoglieBase);
            consumo *= (0.7f + rapportoFoglie * 0.3f);

            return consumo;
        }

        public bool AggiornaIdratazione(WorldModifier worldMod, Weather meteo)
        {
            float consumo = CalcolaConsumoAcqua(worldMod);

            if (worldMod.IsMeteoOn && meteo == Weather.Rainy)
            {
                stats.Idratazione = Math.Min(1.0f, stats.Idratazione + 0.005f);
            }
            else
            {
                stats.Idratazione = Math.Max(0, stats.Idratazione - consumo);
            }

            Game.pianta.ControlloCrescita();

            if (IsDisidratata)
            {
                contatoreSecondi++;
                if (contatoreSecondi == 5)
                {
                    float danno = (SOGLIA_DISIDRATAZIONE - stats.Idratazione) * 0.01f;
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


        public float CalcolaProbabilitaInfestazione(Weather meteo)
        {
            float probabilitaBase = 0.001f;

            if (meteo == Weather.Rainy || meteo == Weather.Foggy)
                probabilitaBase *= 2f;

            if (IsDisidratata)
                probabilitaBase *= 1.5f;

            if (IsCritica)
                probabilitaBase *= 2f;

            if (IsFredda || IsTorrida)
                probabilitaBase *= 0.3f;

            float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, stats.FoglieBase);
            probabilitaBase *= rapportoFoglie;

            float moltiplicatore = 1f - ResistenzaParassitiTotale;

            return Math.Clamp(probabilitaBase * moltiplicatore, 0f, 1f);
        }

        public float CalcolaDannoParassiti()
        {
            if (!stats.Infestata) return 0f;

            float dannoBase = stats.IntensitaInfestazione * 0.01f;
            float moltiplicatore = 1f - ResistenzaParassitiTotale;

            return Math.Max(0, dannoBase * moltiplicatore);
        }

        public void AggiornaParassiti(Weather meteo)
        {
            if (!stats.Infestata)
            {
                float probabilita = CalcolaProbabilitaInfestazione(meteo);
                if (RandomHelper.Float(0, 1) < probabilita)
                {
                    stats.Infestata = true;
                    stats.IntensitaInfestazione = RandomHelper.Float(0.1f, 0.3f);
                }
            }
            else
            {
                stats.IntensitaInfestazione = Math.Min(1f, stats.IntensitaInfestazione + 0.001f);

                float danno = CalcolaDannoParassiti();
                if (danno > 0)
                {
                    ApplicaDanno(danno, "parassiti");

                    if (RandomHelper.Float(0, 1) < stats.IntensitaInfestazione * 0.1f)
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
            if (worldMod.OxygenLevel < 0.5f)
            {
                stats.Ossigeno = Math.Max(0, stats.Ossigeno - CONSUMO_OSSIGENO_BASE);

                if (IsSoffocamento)
                {
                    float danno = (SOGLIA_SOFFOCAMENTO - stats.Ossigeno) * 0.02f;
                    ApplicaDanno(danno, "soffocamento");
                }
            }
            else
            {
                stats.Ossigeno = Math.Min(1f, stats.Ossigeno + 0.005f);
            }
        }

        public void FornisciOssigeno(float quantita)
        {
            stats.Ossigeno = Math.Clamp(stats.Ossigeno + quantita, 0f, 1f);
        }


        public float CalcolaFotosintesi(DayPhase fase, Weather meteo, WorldModifier worldMod)
        {
            float energiaBase = 0f;

            switch (fase)
            {
                case DayPhase.Morning:
                case DayPhase.Afternoon:
                    energiaBase = 0.003f;
                    break;
                case DayPhase.Dawn:
                case DayPhase.Dusk:
                    energiaBase = 0.001f;
                    break;
                default:
                    energiaBase = -0.001f;
                    break;
            }

            energiaBase *= worldMod.SolarMultiplier;

            if (worldMod.IsMeteoOn)
            {
                switch (meteo)
                {
                    case Weather.Foggy:
                        energiaBase *= 0.3f;
                        break;
                    case Weather.Cloudy:
                        energiaBase *= 0.6f;
                        break;
                    case Weather.Rainy:
                        energiaBase *= 0.4f;
                        break;
                    case Weather.Stormy:
                        energiaBase *= 0.2f;
                        break;
                }
            }

            if (IsFredda)
                energiaBase *= 0.5f;
            else if (IsTorrida)
                energiaBase *= 0.3f;
            else if (IsCalda)
                energiaBase *= 0.7f;

            float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, FoglieMassime);
            energiaBase *= (0.3f + rapportoFoglie * 0.7f);

            return energiaBase;
        }

        public void AggiornaMetabolismo(DayPhase fase, Weather meteo, WorldModifier worldMod)
        {
            float energia = CalcolaFotosintesi(fase, meteo, worldMod);
            stats.Metabolismo = Math.Clamp(stats.Metabolismo + energia, 0f, 1f);
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

            if (IsCritica || IsDisidratata || IsSoffocamento)
            {
                velocita = 0;
            }

            return Math.Max(0, velocita);
        }

        public bool TentaCrescita(WorldModifier worldMod)
        {
            float velocita = CalcolaVelocitaCrescita(worldMod);

            if (velocita <= 0.01f || stats.Altezza >= stats.AltezzaMassima)
                return false;

            float incrementoAltezza = CRESCITA_BASE * velocita;
            stats.Altezza = Math.Min(stats.Altezza + incrementoAltezza, stats.AltezzaMassima);

            if (stats.FoglieAttuali < FoglieMassime)
            {
                float probabilitaFoglia = velocita * 0.1f * (1f - (float)stats.FoglieAttuali / FoglieMassime);
                if (RandomHelper.Float(0, 1) < probabilitaFoglia)
                {
                    stats.FoglieAttuali++;
                }
            }

            return true;
        }


        public float CalcolaProbabilitaPerdiaFoglia(Weather meteo)
        {
            float probabilita = stats.DropRateFoglie;

            if (IsDisidratata)
                probabilita *= 3f;

            if (IsCritica)
                probabilita *= 2f;

            if (meteo == Weather.Stormy)
                probabilita *= 5f;

            if (stats.Infestata)
                probabilita *= (1f + stats.IntensitaInfestazione);

            probabilita *= GetMoltiplicatoreFoglieTemperatura();

            return probabilita;
        }

        public void AggiornaFoglie(Weather meteo)
        {
            float probabilita = CalcolaProbabilitaPerdiaFoglia(meteo);

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
            Console.WriteLine($"Pianta danneggiata: -{danno:F3} ({causa}) - Salute: {PercentualeSalute:P0}");
        }

        public void Rigenera(float quantita)
        {
            stats.Salute = Math.Min(VitalitaMax, stats.Salute + quantita);
        }

        public float CalcolaRigenerazioneNaturale()
        {
            if (IsDisidratata || IsSoffocamento || stats.Infestata)
                return 0f;

            if (IsFredda || IsTorrida)
                return 0f;

            float rigenera = RIGENERAZIONE_SALUTE_BASE;
            rigenera *= (stats.Idratazione + stats.Metabolismo + stats.Ossigeno) / 3f;

            if (IsTemperaturaIdeale)
                rigenera *= 1.5f;

            return rigenera;
        }

        public void AggiornaRigenerazione()
        {
            float rigenera = CalcolaRigenerazioneNaturale();
            if (rigenera > 0 && stats.Salute < VitalitaMax)
            {
                Rigenera(rigenera);
            }
        }


        public void AggiornaTutto(DayPhase fase, Weather meteo, WorldModifier worldMod)
        {
            if (!IsViva) return;

            AggiornaTemperatura(fase, meteo, worldMod);

            ApplicaDanniTemperatura();

            AggiornaIdratazione(worldMod, meteo);
            AggiornaOssigeno(worldMod);
            AggiornaMetabolismo(fase, meteo, worldMod);

            AggiornaParassiti(meteo);
            AggiornaFoglie(meteo);

            AggiornaRigenerazione();
        }

        public void Reset()
        {
            stats.Salute = VitalitaMax;
            stats.Idratazione = 1.0f;
            stats.Ossigeno = 1.0f;
            stats.Metabolismo = 1.0f;
            stats.Temperatura = 20.0f;
            stats.FoglieAttuali = FoglieMassime;
            stats.Altezza = 0f;
            stats.Infestata = false;
            stats.IntensitaInfestazione = 0f;
        }

        public string GetRiepilogo()
        {
            return $"[{SeedDataType.GetName(Game.pianta.TipoSeme)}]\n" +
                   $"Salute: {PercentualeSalute:P0}\n" +
                   $"Idratazione: {stats.Idratazione:P0}\n" +
                   $"Ossigeno: {stats.Ossigeno:P0}\n" +
                   $"Metabolismo: {stats.Metabolismo:P0}\n" +
                   $"Temperatura: {stats.Temperatura:P0}\n" +
                   $"Foglie: {stats.FoglieAttuali}/{FoglieMassime}\n" +
                   $"Altezza: {stats.Altezza:F1}\n" +
                   (stats.Infestata ? $"⚠ INFESTATA ({stats.IntensitaInfestazione:P0})\n" : "");
        }

    }
}
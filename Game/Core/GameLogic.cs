using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants
{
    public class GameLogicPianta
    {
        public const float CONSUMO_ACQUA_BASE = 0.002f;
        public const float CONSUMO_OSSIGENO_BASE = 0.001f;
        public const float RIGENERAZIONE_SALUTE_BASE = 0.001f;
        public const float CRESCITA_BASE = 0.5f;
        public const float SOGLIA_DISIDRATAZIONE = 0.2f;
        public const float SOGLIA_SOFFOCAMENTO = 0.3f;
        public const float TEMPERATURA_FREDDO = 5.0f;
        public const float TEMPERATURA_CALDO = 35.0f;

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
        public SeedBonus GetSeedBonus() => bonus;


        public float CalcolaConsumoAcqua(WorldModifier worldMod)
        {
            float consumo = CONSUMO_ACQUA_BASE;

            consumo *= ConsumoAcquaMult;

            consumo *= worldMod.WaterConsumption;

            consumo *= (0.5f + MetabolismoEffettivo * 0.5f);

            float rapportoFoglie = (float)stats.FoglieAttuali / Math.Max(1, stats.FoglieBase);
            consumo *= (0.7f + rapportoFoglie * 0.3f);

            return consumo;
        }

        public bool AggiornaIdratazione(WorldModifier worldMod, Weather meteo)
        {
            float consumo = CalcolaConsumoAcqua(worldMod);
            if (Game.controller.annaffiatoioAttivo == true && Game.controller.isButtonRightPressed == true)
            {
                Annaffia(0.01f);
            }
            else if (worldMod.IsMeteoOn && meteo == Weather.Rainy)
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
                float danno = (SOGLIA_DISIDRATAZIONE - stats.Idratazione) * 0.01f;
                ApplicaDanno(danno, "disidratazione");
                return true;
            }

            return false;
        }


        public void Annaffia(float quantita)
        {
            stats.Idratazione = Math.Clamp(stats.Idratazione + quantita, 0f, 1f);
        }

        public float CalcolaDannoTemperatura(float temperatura)
        {
            if (temperatura < TEMPERATURA_FREDDO)
            {
                float intensita = (TEMPERATURA_FREDDO - temperatura) / TEMPERATURA_FREDDO;
                float resistenza = ResistenzaFreddoTotale;

                float moltiplicatore = 1f - resistenza;
                return Math.Max(0, intensita * 0.05f * moltiplicatore);
            }
            else if (temperatura > TEMPERATURA_CALDO)
            {
                float intensita = (temperatura - TEMPERATURA_CALDO) / 20f;
                float resistenza = ResistenzaCaldoTotale;

                float moltiplicatore = 1f - resistenza;
                return Math.Max(0, intensita * 0.05f * moltiplicatore);
            }

            return 0f;
        }

        public float ApplicaDannoTemperatura(float temperatura)
        {
            float danno = CalcolaDannoTemperatura(temperatura);
            if (danno > 0)
            {
                ApplicaDanno(danno, temperatura < TEMPERATURA_FREDDO ? "freddo" : "caldo");
            }
            return danno;
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

            if (velocita > 0.01f && stats.Altezza < stats.AltezzaMassima)
            {
                stats.Altezza += CRESCITA_BASE * velocita;

                if (stats.FoglieAttuali < FoglieMassime && RandomHelper.Float(0, 1) < velocita * 0.1f)
                {
                    stats.FoglieAttuali++;
                }

                return true;
            }

            return false;
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

            float rigenera = RIGENERAZIONE_SALUTE_BASE;
            rigenera *= (stats.Idratazione + stats.Metabolismo + stats.Ossigeno) / 3f;

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

        public void AggiornaTutto(DayPhase fase, Weather meteo, WorldModifier worldMod, float temperatura = 20f)
        {
            if (!IsViva) return;

            AggiornaIdratazione(worldMod, meteo);
            AggiornaOssigeno(worldMod);
            AggiornaMetabolismo(fase, meteo, worldMod);

            ApplicaDannoTemperatura(temperatura);
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
            stats.FoglieAttuali = FoglieMassime;
            stats.Altezza = 0f;
            stats.Infestata = false;
            stats.IntensitaInfestazione = 0f;
        }
    }
}

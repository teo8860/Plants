using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Plant : GameElement
{

    public float idratazione = 0;
    public Vector2 posizione = new(0,0);

    private List<Vector2> puntiSpline = new(); 
    private const int margineMinimo = 40;

    private List<Ramo> rami = new(); 
    private int contatorePuntiPerRamo = 0; 

    DayPhase Fase = Game.Phase;

    public PlantStats Stats = new PlantStats();

    public Plant()
    {
        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

        for(int a = 0; a <100; a++)
        {
            Crescita();
        }
    }

    public void PosizionaAlCentroInBasso()
    {
        float centroX = 0.5f;
        float bassoY = 0.04f;
        posizione = new (GameProperties.windowWidth/2, GameProperties.windowHeight-GameProperties.groundPosition);
    }

    public float GetCrescitaRate()
    {
        float base_rate = (Stats.Idratazione + Stats.Metabolismo + Stats.Ossigeno) / 3f;
        float world_rate = base_rate * WorldManager.GetCurrentModifiers().GrowthRateMultiplier;

        if (Stats.Salute < 0.5f)
            world_rate *= Stats.Salute;

        return Math.Max(0, world_rate);
    }

    public void Crescita()
    {
        GeneraPuntoCasuale();

        contatorePuntiPerRamo++;
        if (contatorePuntiPerRamo == 5)
        {
            Vector2 puntoAttacco = puntiSpline[^2];

            Direzione direction;

            float margineSicurezza = 100f;

            if (puntoAttacco.X < margineSicurezza)
            {
                direction = Direzione.Destra;
            }
            else if (puntoAttacco.X > GameProperties.windowWidth - margineSicurezza)
            {
                direction = Direzione.Sinistra;
            }
            else
            {
                if(RandomHelper.Int(0, 2) == 0)
                    direction = Direzione.Sinistra;
                else
                    direction = Direzione.Destra;

            }

            rami.Add(new Ramo(puntoAttacco, direction));

            contatorePuntiPerRamo = 0;
        }

        foreach (var ramo in rami)
        {
            ramo.Cresci();
        }

        Vector2 ultimoPunto = puntiSpline[^1];
        if (ultimoPunto.Y + Game.controller.offsetY <= 100)
        {
            Game.controller.offsetY += 50;
        }
        
    }

    public void Annaffia()
    {
        float incrementoCasuale = RandomHelper.Float(0,1) * 0.5f + 0.1f; 

        float crescitaEffettiva = incrementoCasuale * GetCrescitaRate();
        if (crescitaEffettiva > 0.01f)
        {
            Crescita();
        }
    }

    public void Reset()
    {
        idratazione = 0;
        Game.controller.offsetY = 0;
        puntiSpline.Clear();
        rami.Clear();
        GeneraPuntoIniziale();
    }

    private void GeneraPuntoIniziale()
    {
        puntiSpline.Clear();

        puntiSpline.Add(new Vector2(posizione.X, posizione.Y));

        puntiSpline.Add(new Vector2(posizione.X, posizione.Y));

        float terzoX = Math.Clamp(
            puntiSpline[1].X + RandomHelper.Int(-15, 15),
            margineMinimo,
            GameProperties.windowWidth - margineMinimo
        );
        float terzoY = puntiSpline[1].Y - RandomHelper.Int(30, 50);
        puntiSpline.Add(new Vector2(terzoX, terzoY));
    }

    private void GeneraPuntoCasuale()
    {
        Vector2 ultimoPunto = puntiSpline[^1];
        float nuovoX = Math.Clamp(ultimoPunto.X + Raylib.GetRandomValue(-15,15), margineMinimo, GameProperties.windowWidth - margineMinimo);
        float nuovoY = ultimoPunto.Y - Raylib.GetRandomValue(30, 50);

        puntiSpline.Add(new Vector2(nuovoX, nuovoY));
    }

    public override void Update()
    {
        Weather currentWeather = WeatherManager.GetCurrentWeather();
        WorldModifier currentWorldModifier = WorldManager.GetCurrentModifiers();

        float consumoAcqua = 0.002f * currentWorldModifier.WaterConsumption;

        if (currentWeather == Weather.Rainy && currentWorldModifier.IsMeteoOn == true)
        {
            idratazione = Math.Min(1.0f, idratazione + 0.005f);
        }
        else
        {
            idratazione = Math.Max(0, idratazione - consumoAcqua);
        }

        float energiaGuadagnata = 0f;

        if (Fase == DayPhase.Morning || Fase == DayPhase.Afternoon)
        {
            energiaGuadagnata = 0.003f * currentWorldModifier.SolarMultiplier;

            if (currentWeather == Weather.Foggy && currentWorldModifier.IsMeteoOn == true)
                energiaGuadagnata *= 0.3f;
        }
        else if (Fase == DayPhase.Dawn || Fase == DayPhase.Dusk)
        {
            energiaGuadagnata = 0.001f * currentWorldModifier.SolarMultiplier;
        }

        else
        {
            energiaGuadagnata = -0.001f;
        }

        Stats.Metabolismo = Math.Clamp(Stats.Metabolismo + energiaGuadagnata, 0f, 1f);

        if (currentWorldModifier.OxygenLevel < 0.5f)
        {

            Stats.Ossigeno = Math.Max(0, Stats.Ossigeno - 0.001f);
        }
        else
        {
            Stats.Ossigeno = 1.0f;
        }

    }

    public override void Draw()
    {
        if (puntiSpline.Count >= 4) 
        {
            Span<Vector2> puntiConOffset = stackalloc Vector2[puntiSpline.Count];

            for (int i = 0; i < puntiSpline.Count; i++)
            {
                puntiConOffset[i] = new Vector2(
                    puntiSpline[i].X,
                    puntiSpline[i].Y + Game.controller.offsetY
                );
            }

            for (int i = 0; i < puntiSpline.Count - 3; i++)
            {
                float spessore = 8 + ((puntiSpline.Count - i) / 5); 

                if(i+4 <= puntiConOffset.Length)
                {
                    Span<Vector2> segmento = puntiConOffset.Slice(i, 4);
                    for(int o=0; o<segmento.Length; o++)
                    {
                        segmento[o].X += (float)Math.Sin(Time.GetTime());
                    }
                    Graphics.DrawSplineCatmullRom(segmento, spessore, Color.Green);
                }
            }


        }
        
        foreach (var ramo in rami)
        {
            ramo.Draw(Game.controller.offsetY);
        }

        Graphics.DrawEllipse((int)posizione.X, (int)((int)posizione.Y+10+ Game.controller.offsetY), 15, 25, Color.DarkBrown);
    }
}

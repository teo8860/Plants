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

    public Vector2 posizione = new(0,0);

    private List<Vector2> puntiSpline = new(); 
    private const int margineMinimo = 40;

    private List<Ramo> rami = new(); 
    private List<Radice> radici = new(); 
    private int contatorePuntiPerRamo = 0; 
    private int contatorePuntiPerRadice = 0;

    DayPhase Fase = Game.Phase;

    public SeedType TipoSeme = SeedType.Normale;
    public SeedBonus seedBonus = SeedBonus.Default;

    public PlantStats Stats = new PlantStats();

    public GameLogicPianta proprieta;

    public Plant()
    {
        proprieta = new GameLogicPianta();
        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();
        /* Test di crescita rapida
        for(int a = 0; a <100; a++)
        {
            Crescita();
        }
       */
        SetSeed(SeedType.Normale);
    }

    public Plant(SeedType seedType)
    {
        SetSeed(seedType);
    }

    public void SetSeed(SeedType seedType)
    {
        TipoSeme = seedType;
        seedBonus = SeedDataType.GetBonus(seedType);
    }


    public void PosizionaAlCentroInBasso()
    {
        float centroX = 0.5f;
        float bassoY = 0.04f;
        posizione = new (GameProperties.windowWidth/2, GameProperties.windowHeight-GameProperties.groundPosition);
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

        
        contatorePuntiPerRadice++;
        if (contatorePuntiPerRadice == 25)
        {
            Vector2 puntoAttacco = posizione;
            puntoAttacco.Y += 20;

            Vector2 pos = posizione;
            pos.X += RandomHelper.Int(-45,45);
            pos.Y += RandomHelper.Int(30,40);

            radici.Add(new Radice(puntoAttacco, pos));

            contatorePuntiPerRadice = 0;
        }


        foreach (var ramo in rami)
        {
            ramo.Cresci();
        }
        
        foreach (var radice in radici)
        {
            radice.Cresci();
        }

        Vector2 ultimoPunto = puntiSpline[^1];
        if (ultimoPunto.Y + Game.controller.offsetY <= 100)
        {
            Game.controller.offsetY += 50;
        }
        
    }

    public void ControlloCrescita()
    {
        if (Stats.Altezza >= Stats.AltezzaMassima)
            return;

        float incrementoCasuale = RandomHelper.Float(0, 1) * 0.5f + 0.1f;
        float crescitaEffettiva = incrementoCasuale * proprieta.CalcolaVelocitaCrescita(WorldManager.GetCurrentModifiers());

        if (crescitaEffettiva > 0.01f)
        {
            if (proprieta.TentaCrescita(WorldManager.GetCurrentModifiers()))
            {
                Crescita();
            }
        }
    }

    public void Reset()
    {
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
        
        foreach (var radice in radici)
        {
            radice.Draw(Game.controller.offsetY);
        }
        Graphics.DrawEllipse((int)posizione.X, (int)((int)posizione.Y+10+ Game.controller.offsetY), 15, 25, Color.DarkBrown);
    }
}

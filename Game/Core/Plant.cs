using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Plants;

public class Plant : GameElement
{

    public Vector2 posizione = new(0, 0);

    public List<Vector2> puntiSpline = new();
    private const int margineMinimo = 40;

    private List<Ramo> rami = new();
    private List<Radice> radici = new();
    private int contatorePuntiPerRamo = 0;
    private int contatorePuntiPerRadice = 0;

    public float spessore;

    DayPhase Fase = Game.Phase;

    public SeedType TipoSeme = SeedType.Normale;
    public SeedBonus seedBonus = SeedBonus.Default;

    public PlantStats Stats = new PlantStats();

    public GameLogicPianta proprieta;
    
    public int y3 => (int)(GameProperties.windowHeight - GameProperties.groundPosition - (Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)) + 340;
    public int y4 => (int)(GameProperties.windowHeight - (Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)) + 300;

    public float nextWorldGroundY => y4 - y3;
    public Plant()
    {
        proprieta = new GameLogicPianta(this);
        SetSeed(SeedType.Normale);

        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

        // /* Test di crescita rapida
        if (!Game.tutorial.isTutorialActive)
        {
            for (int a = 0; a < 1240; a++)
            {
                Crescita();
            }
        }
        // */
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
        posizione = new(GameProperties.windowWidth / 2, GameProperties.windowHeight - GameProperties.groundPosition);
    }

    public void Crescita()
    {

        float velocita = proprieta.CalcolaVelocitaCrescita(WorldManager.GetCurrentModifiers());
        float metabolismo = proprieta.MetabolismoEffettivo;

        float incrementoBase = 30f + RandomHelper.Float(0, 20f);
        float incrementoFinale = incrementoBase * metabolismo * (0.5f + velocita * 0.5f);

        incrementoFinale = Math.Max(15f, incrementoFinale);

        if (Stats.Altezza + incrementoFinale > Stats.AltezzaMassima)
        {
            incrementoFinale = Stats.AltezzaMassima - Stats.Altezza;
        }

        Stats.Altezza += incrementoFinale;

        if (Stats.FoglieAttuali < proprieta.FoglieMassime)
        {
            float probabilitaFoglia = velocita * 0.15f * (1f - (float)Stats.FoglieAttuali / proprieta.FoglieMassime);
            if (RandomHelper.Float(0, 1) < probabilitaFoglia)
            {
                Stats.FoglieAttuali++;
            }
        }

        GeneraPuntoCasuale(incrementoFinale);

        contatorePuntiPerRamo++;

        int ramiMassimi = Math.Max(1, proprieta.FoglieMassime / 5);

        if (contatorePuntiPerRamo >= 5 && rami.Count < ramiMassimi)
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
                if (RandomHelper.Int(0, 2) == 0)
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
            pos.X += RandomHelper.Int(-45, 45);
            pos.Y += RandomHelper.Int(90, 90);

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
        
        if (puntiSpline.Count >= 3)
        {
            float deltaY = puntiSpline[^3].Y - puntiSpline[^2].Y; 
            if (Game.controller.offsetY < Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)
                Game.controller.Scorri(deltaY);
        }
    }

    public void ControlloCrescita()
    {
        if (Stats.Altezza >= Stats.AltezzaMassima)
            return;

        float velocita = proprieta.CalcolaVelocitaCrescita(WorldManager.GetCurrentModifiers());

        if (velocita > 0.01f && RandomHelper.Float(0, 1) < velocita)
        {
            Crescita();
        }
    }

    public void Reset()
    {
        Game.controller.offsetY = 0;
        puntiSpline.Clear();
        rami.Clear();
        radici.Clear();

        Stats.Altezza = 0;
        Stats.FoglieAttuali = 0;

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

    private void GeneraPuntoCasuale(float incrementoAltezza)
    {
        Vector2 ultimoPunto = puntiSpline[^1];

        float nuovoX = Math.Clamp(ultimoPunto.X + Raylib.GetRandomValue(-15, 15), margineMinimo, GameProperties.windowWidth - margineMinimo);

        float nuovoY = ultimoPunto.Y - incrementoAltezza;

        puntiSpline.Add(new Vector2(nuovoX, nuovoY));
    }

    public override void Update()
    {

    }

    public override void Draw()
    {
        float offsetY = Game.controller.offsetY;

        if (puntiSpline.Count >= 4)
        {
            Span<Vector2> puntiConOffset = stackalloc Vector2[puntiSpline.Count];

            for (int i = 0; i < puntiSpline.Count; i++)
            {
                puntiConOffset[i] = new Vector2(
                    puntiSpline[i].X,
                    puntiSpline[i].Y + offsetY
                );
            }

            foreach (var ramo in rami)
            {
                if (ramo.IsInView(offsetY))
                    ramo.Draw(offsetY);
            }


            Vector2 ultimoPunto = puntiSpline[^1];
            for (int i = 0; i < puntiSpline.Count - 3; i++)
            {
                float segmentMinY = Math.Min(puntiSpline[i].Y, puntiSpline[i + 3].Y);
                float segmentMaxY = Math.Max(puntiSpline[i].Y, puntiSpline[i + 3].Y);

                if (!ViewCulling.IsRangeVisible(segmentMinY, segmentMaxY, offsetY))
                    continue;

                spessore = Math.Min(8 + ((puntiSpline.Count - i) / 5), 50);

                if (i + 4 <= puntiConOffset.Length)
                {
                    Span<Vector2> segmento = puntiConOffset.Slice(i, 4);
                    for (int o = 0; o < segmento.Length; o++)
                    {
                        segmento[o].X += getSinOffset();
                    }
                    Graphics.DrawSplineCatmullRom(segmento, spessore, Color.Green);
                    if (spessore > 10)
                    {
                        Graphics.DrawSplineCatmullRom(segmento, spessore - 10, Color.DarkGreen);
                    }
                }
            }

            if (Stats.Altezza >= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)
            {
 
                Vector2 puntoPartenza = new Vector2(
                    puntiSpline[^2].X,
                    puntiSpline[^2].Y + Game.controller.offsetMaxY
                );

                Vector2 puntoArrivo = new Vector2(
                    puntiSpline[^2].X,
                    nextWorldGroundY 
                );
                Console.WriteLine(puntoPartenza.Y + " ; " + puntoArrivo.Y);
                Span<Vector2> segmento = stackalloc Vector2[4];
                segmento[0] = puntoPartenza;
                segmento[1] = new Vector2(
                               puntoPartenza.X + getSinOffset(),
                               puntoPartenza.Y
                           );
                segmento[2] = new Vector2(
                    puntoArrivo.X - getSinOffset(),
                    puntoArrivo.Y
                );
                segmento[3] = puntoArrivo;

                
                Graphics.DrawSplineCatmullRom(segmento, spessore, Color.Green);
                if (spessore > 10)
                {
                    Graphics.DrawSplineCatmullRom(segmento, spessore - 10, Color.DarkGreen);
                }
                

                //Graphics.DrawLineEx(new Vector2(puntiConOffset[^2].X, puntiConOffset[^2].Y), new Vector2(puntiConOffset[^2].X, nextWorldGroundY), spessore, Color.Yellow);


                //Graphics.DrawCircleV(new Vector2(puntoArrivo.X,puntoArrivo.Y - 10), spessore, Color.DarkGreen);
                //Graphics.DrawEllipse((int)puntoArrivo.X, (int)(puntoArrivo.Y + 10 + offsetY), 15, 25, Color.DarkBrown);
            }
        }

        foreach (var radice in radici)
        {
            if (radice.IsInView(offsetY))
                radice.Draw(offsetY);
        }

        if (ViewCulling.IsYVisible(posizione.Y + 10, offsetY))
        {
            Graphics.DrawEllipse((int)posizione.X, (int)(posizione.Y + 10 + offsetY), 15, 25, Color.DarkBrown);
        }
    }

    public float getSinOffset()
    {
        return (float)Math.Sin(Time.GetTime());
    }

}
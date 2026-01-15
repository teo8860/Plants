using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Plants;

public class RamoEdera
{
    public float StartX, StartY;
    public float Direction;
    public List<Vector2> Punti = new();
    public List<(Vector2 pos, float size, float angle)> MiniFoglie = new();
    public float GrowthProgress = 0;
    public int MaxSegments = 12;
    public Color Colore;

    private Random rng;
    private float oscillationTime = 0;

    public RamoEdera(float x, float y, float direction, Color colore, int seed)
    {
        StartX = x;
        StartY = y;
        Direction = direction;
        Colore = colore;
        rng = new Random(seed);

        float curX = x;
        float curY = y;
        Punti.Add(new Vector2(curX, curY));

        for (int i = 0; i < MaxSegments; i++)
        {
            float dx = direction * (8 + (float)rng.NextDouble() * 6);
            float dy = (float)(rng.NextDouble() - 0.5) * 4;
            dy += MathF.Sin(i * 0.3f) * 1.5f;

            curX += dx;
            curY += dy;
            curX = Math.Clamp(curX, 8, GameProperties.cameraWidth - 8);

            Punti.Add(new Vector2(curX, curY));

            if (i % 2 == 0)
            {
                float leafAngle = (float)(rng.NextDouble() * MathF.PI - MathF.PI / 2);
                MiniFoglie.Add((new Vector2(curX, curY), 4 + (float)rng.NextDouble() * 2, leafAngle));
            }
        }
    }

    public void Update(float deltaTime)
    {
        oscillationTime += deltaTime;
        if (GrowthProgress < 1)
        {
            GrowthProgress += deltaTime * 0.4f;
            if (GrowthProgress > 1) GrowthProgress = 1;
        }
    }

    public void Draw(float cameraY)
    {
        if (!ViewCulling.IsValueVisible(StartY, cameraY)) return;

        int visibleSegments = (int)(Punti.Count * GrowthProgress);
        if (visibleSegments < 2) return;

        for (int i = 0; i < visibleSegments - 1; i++)
        {
            Vector2 p1 = Punti[i];
            Vector2 p2 = Punti[i + 1];

            float osc = MathF.Sin(oscillationTime * 2 + i * 0.5f) * 1.2f;
            float thickness = 2.5f * (1 - (float)i / Punti.Count * 0.4f);

            Graphics.DrawLineEx(
                new Vector2(p1.X + osc, p1.Y),
                new Vector2(p2.X + osc, p2.Y),
                thickness, Colore);
        }

        int visibleLeaves = (int)(MiniFoglie.Count * GrowthProgress);
        for (int i = 0; i < visibleLeaves; i++)
        {
            var (pos, size, angle) = MiniFoglie[i];
            float leafOsc = MathF.Sin(oscillationTime * 2.5f + i) * 0.08f;
            DrawMiniLeaf(pos.X, pos.Y, size, angle + leafOsc, Colore);
        }
    }

    private void DrawMiniLeaf(float x, float y, float size, float angle, Color color)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        Vector2 tip = new Vector2(x + cos * size, y - sin * size);
        Vector2 left = new Vector2(x - sin * size * 0.25f, y - cos * size * 0.25f);
        Vector2 right = new Vector2(x + sin * size * 0.25f, y + cos * size * 0.25f);

        Graphics.DrawTriangle(tip, left, right, color);
    }
}

public class Plant : GameElement
{
    public Vector2 posizione = new(0, 0);

    public List<Vector2> puntiSpline = new();
    private const int margineMinimo = 40;

    private List<Ramo> rami = new();
    private List<Radice> radici = new();
    private int contatorePuntiPerRamo = 0;
    private int contatorePuntiPerRadice = 0;

    private List<RamoEdera> ramiEdera = new();
    private bool ederaCreata = false;

    public float spessore;

    DayPhase Fase = Game.Phase;

    public SeedType TipoSeme = SeedType.Normale;
    public SeedStats seedBonus = new SeedStats();

    public PlantStats Stats = new PlantStats();

    public GameLogicPianta proprieta;

    public Color colore1, colore2;

    public float nextWorldGroundY => Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier + GameProperties.groundHeight - 10;

    public void setColori(Color Colore1, Color Colore2)
    {
        colore1 = Colore1;
        colore2 = Colore2;
    }

    public void SetNaturalColors(WorldType world)
    {
        (float hue, float sat, float val) = world switch
        {
            WorldType.Luna => (180, 0.20f, 0.55f),
            WorldType.Marte => (15, 0.60f, 0.55f),
            WorldType.Europa => (200, 0.40f, 0.60f),
            WorldType.Venere => (40, 0.70f, 0.60f),
            WorldType.Titano => (30, 0.50f, 0.50f),
            WorldType.ReameMistico => (280, 0.50f, 0.60f),
            WorldType.GiardinoMistico => (140, 0.70f, 0.65f),
            WorldType.Origine => (0, 0.0f, 0.90f),
            _ => (125, 0.55f, 0.55f)
        };

        colore1 = Color.FromHSV(hue, sat, val);
        colore2 = Color.FromHSV(hue, sat * 1.1f, val * 0.7f);
    }

    public Plant()
    {
        proprieta = new GameLogicPianta(this);
        SetSeed(SeedType.Normale);

        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

         ///* 
        if (!Game.tutorial.isTutorialActive)
        {
            for (int a = 0; a < 400; a++)
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
        float centroX = Rendering.camera.view.X / 2;
        posizione = new(centroX, GameProperties.groundPosition);
    }

    public void Crescita()
    {
        float velocita = proprieta.CalcolaVelocitaCrescita(WorldManager.GetCurrentModifiers());
        float metabolismo = proprieta.MetabolismoEffettivo;

        float incrementoBase = 15f + RandomHelper.Float(0, 8f);
        float incrementoFinale = incrementoBase * metabolismo * (0.5f + velocita * 0.5f);

        incrementoFinale = Math.Max(7f, incrementoFinale);

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
            else if (puntoAttacco.X > GameProperties.viewWidth - margineSicurezza)
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
        if (contatorePuntiPerRadice == 45)
        {
            Vector2 puntoAttacco = posizione;
            puntoAttacco.Y -= 5;

            Vector2 pos = posizione;
            //pos.X += RandomHelper.Int(-45, 45);
            pos.Y += posizione.Y;

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

        if (!ederaCreata && Stats.Altezza >= Stats.AltezzaMassima * 0.95f)
        {
            CreaEdera();
            ederaCreata = true;
        }

        if (Rendering.camera.position.Y <= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier && Game.controller.autoscroll == true)
        {
            Rendering.camera.position.Y += incrementoFinale;
        }
    }

    private void CreaEdera()
    {
        float ederaY = nextWorldGroundY;
        Random rng = new Random();

        for (int i = 0; i < 6; i++)
        {
            float direction = (i % 2 == 0) ? -1 : 1;
            float startOffset = (i / 2) * 4 - 8;

            ramiEdera.Add(new RamoEdera(
                posizione.X + startOffset,
                ederaY + rng.Next(-8, 8),
                direction,
                colore1,
                rng.Next()));
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
        Rendering.camera.position.Y = 0;
        puntiSpline.Clear();
        rami.Clear();
        radici.Clear();
        ramiEdera.Clear(); 
        ederaCreata = false; 

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
            puntiSpline[1].X + RandomHelper.Int(-10, 10),
            margineMinimo,
            GameProperties.cameraWidth - margineMinimo
        );
        float terzoY = puntiSpline[1].Y + RandomHelper.Int(13, 26);
        puntiSpline.Add(new Vector2(terzoX, terzoY));
    }

    private void GeneraPuntoCasuale(float incrementoAltezza)
    {
        Vector2 ultimoPunto = puntiSpline[^1];

        float nuovoX = Math.Clamp(ultimoPunto.X + Raylib.GetRandomValue(-10, 10), margineMinimo, GameProperties.cameraWidth - margineMinimo);

        float nuovoY = ultimoPunto.Y + incrementoAltezza;

        puntiSpline.Add(new Vector2(nuovoX, nuovoY));
    }

    public override void Update()
    {
        float deltaTime = Time.GetFrameTime();
        foreach (var edera in ramiEdera)
        {
            edera.Update(deltaTime);
        }
    }

    public override void Draw()
    {
        float cameraY = Rendering.camera.position.Y;

        if (puntiSpline.Count >= 4)
        {
            Span<Vector2> puntiSplineSpan = stackalloc Vector2[puntiSpline.Count];

            for (int i = 0; i < puntiSpline.Count; i++)
            {
                puntiSplineSpan[i] = puntiSpline[i];
            }

            foreach (var ramo in rami)
            {
                ramo.Draw();
            }

            Vector2 ultimoPunto = puntiSpline[^1];
            for (int i = 0; i < puntiSpline.Count - 3; i++)
            {
                float segmentMinY = Math.Min(puntiSpline[i].Y, puntiSpline[i + 3].Y);
                float segmentMaxY = Math.Max(puntiSpline[i].Y, puntiSpline[i + 3].Y);

                if (ViewCulling.IsValueVisible(segmentMinY, cameraY) == false && ViewCulling.IsValueVisible(segmentMaxY, cameraY) == false)
                    continue;

                spessore = Math.Min(5f + ((puntiSpline.Count - i) / 10), 22);

                if (i + 4 <= puntiSplineSpan.Length)
                {
                    Span<Vector2> segmento = puntiSplineSpan.Slice(i, 4);
                    for (int o = 0; o < segmento.Length; o++)
                    {
                        segmento[o].X += getSinOffset();
                    }

                    Graphics.DrawSplineCatmullRom(segmento, spessore, colore1);

                    if (spessore > 10)
                    {
                        Graphics.DrawSplineCatmullRom(segmento, spessore - 10, colore2);
                    }
                }
            }

            if (Stats.Altezza >= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)
            {
                Vector2 puntoPartenza = puntiSplineSpan[^2];

                Vector2 puntoArrivo = new Vector2(
                    puntiSplineSpan[^2].X,
                    nextWorldGroundY
                );

                Span<Vector2> segmento = stackalloc Vector2[4];
                segmento[0] = puntoPartenza;
                segmento[1] = new Vector2(
                    puntoPartenza.X + getSinOffset(),
                    puntoPartenza.Y
                );
                segmento[2] = new Vector2(
                    puntoArrivo.X + getSinOffset(),
                    puntoArrivo.Y
                );
                segmento[3] = puntoArrivo;

                Graphics.DrawSplineCatmullRom(segmento, spessore, colore1);
                if (spessore > 10)
                {
                    Graphics.DrawSplineCatmullRom(segmento, spessore - 10, colore2);
                }
            }
        }

        foreach (var edera in ramiEdera)
        {
            edera.Draw(cameraY);
        }

        foreach (var radice in radici)
        {
                radice.Draw();
        }

        if (ViewCulling.IsValueVisible(posizione.Y, cameraY))
        {
            Vector2 screenPos = posizione;
            Graphics.DrawEllipse((int)screenPos.X, (int)screenPos.Y, 8, 12, Color.DarkBrown);
        }
    }

    public float getSinOffset()
    {
        return (float)Math.Sin(Time.GetTime());
    }
}

using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Plants;

public class Obj_Plant : GameElement
{
    public int rseed = 470131434;
    public Vector2 posizione = new(0, 0);

    public List<Vector2> puntiSpline = new();
    private const int margineMinimo = 40;

    private List<Obj_Ramo> rami = new();
    private List<Obj_Radice> radici = new();
    private int contatorePuntiPerRamo = 0;
    private int contatorePuntiPerRadice = 0;

    private List<Obj_RamoEdera> ramiEdera = new();
    private bool ederaCreata = false;

    public float spessore;

    DayPhase Fase = Game.Phase;

    public SeedType TipoSeme = SeedType.Normale;
    public SeedStats seedBonus = new SeedStats();

    public PlantStats Stats = new PlantStats();

    public GameLogicPianta proprieta;

    public Color colore1, colore2;

    public float nextWorldGroundY => Stats.EffectiveMaxHeight + GameProperties.groundHeight - 10;


    public Obj_Plant()
    {
        proprieta = new GameLogicPianta(this);
        SetSeed(SeedType.Normale);

        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

    }


    public Obj_Plant(SeedType seedType)
    {
        SetSeed(seedType);
    }

    
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

        float incrementoBase = 15f + RandomHelper.DeterministicFloatRangeAt(rseed, puntiSpline.Count, 0, 8f);
        float incrementoFinale = incrementoBase * metabolismo * (0.5f + velocita * 0.5f);

        incrementoFinale = Math.Max(7f, incrementoFinale);

        if (Stats.Altezza + incrementoFinale > Stats.EffectiveMaxHeight)
        {
            incrementoFinale = Stats.EffectiveMaxHeight - Stats.Altezza;
        }

        Stats.Altezza += incrementoFinale;

        if (Stats.FoglieAttuali < proprieta.FoglieMassime)
        {
            float probabilitaFoglia = velocita * 0.15f * (1f - (float)Stats.FoglieAttuali / proprieta.FoglieMassime);
            if (RandomHelper.DeterministicFloatRangeAt(rseed, Stats.FoglieAttuali + puntiSpline.Count, 0, 1) < probabilitaFoglia)
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
                if (RandomHelper.DeterministicIntRange(rseed, puntiSpline.Count, 0, 2) == 0)
                    direction = Direzione.Sinistra;
                else
                    direction = Direzione.Destra;
            }

            var ramo = new Obj_Ramo(puntoAttacco, direction);
            ramo.roomId = Game.room_main.id;
            rami.Add(ramo);

            contatorePuntiPerRamo = 0;
        }

        contatorePuntiPerRadice++;
        if (contatorePuntiPerRadice == 45)
        {
            Vector2 puntoAttacco = posizione;
            puntoAttacco.Y -= 5;

            // Alterna sinistra/destra: pari va a sinistra, dispari a destra
            float lato = (radici.Count % 2 == 0) ? -1f : 1f;
            float dirX = lato * RandomHelper.DeterministicFloatRangeAt(rseed, radici.Count, 0.15f, 0.5f);
            Vector2 direzione = new Vector2(dirX, -1f);

            var radice = new Obj_Radice(puntoAttacco, direzione);
            radice.roomId = Game.room_main.id;
            radici.Add(radice);

            // La prima radice crea subito una gemella nella direzione opposta
            if (radici.Count == 1)
            {
                Vector2 direzioneOpposta = new Vector2(-dirX, -1f);
                var radiceGemella = new Obj_Radice(puntoAttacco, direzioneOpposta);
                radiceGemella.roomId = Game.room_main.id;
                radici.Add(radiceGemella);
            }

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

        if (Game.controller.targetScrollY <= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier && Game.controller.autoscroll == true)
        {
            Game.controller.targetScrollY += incrementoFinale;
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

            var edera = new Obj_RamoEdera(
                posizione.X + startOffset,
                ederaY + rng.Next(-8, 8),
                direction,
                colore1,
                rng.Next());
            edera.roomId = Game.room_main.id;
            ramiEdera.Add(edera);
        }
    }

    public void ControlloCrescita()
    {
        if (Game.isPaused) return;

        if (Stats.Altezza >= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier)
            return;

        float velocita = proprieta.CalcolaVelocitaCrescita(WorldManager.GetCurrentModifiers());

        if (velocita > 0.01f && RandomHelper.DeterministicFloatRangeAt(rseed, puntiSpline.Count, 0, 1) < velocita)
        {
            Crescita();
        }
    }

    public void Reset()
    {
        Rendering.camera.position.Y = 0;
        puntiSpline.Clear();

        foreach (var item in rami)
            item.Destroy();

        foreach (var item in radici)
            item.Destroy();

        foreach (var item in ramiEdera)
            item.Destroy();

        rami.Clear();
        radici.Clear();
        ramiEdera.Clear();
        ederaCreata = false;

        contatorePuntiPerRamo = 0;
        contatorePuntiPerRadice = 0;

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
            puntiSpline[1].X + RandomHelper.DeterministicIntRange(rseed, 0, -10, 10),
            margineMinimo,
            GameProperties.cameraWidth - margineMinimo
        );
        float terzoY = puntiSpline[1].Y + RandomHelper.DeterministicIntRange(rseed, 0, 13, 26);
        puntiSpline.Add(new Vector2(terzoX, terzoY));
    }

    private void GeneraPuntoCasuale(float incrementoAltezza)
    {
        Vector2 ultimoPunto = puntiSpline[^1];

        float nuovoX = Math.Clamp(ultimoPunto.X + RandomHelper.DeterministicIntRange(rseed, puntiSpline.Count, -10, 10), margineMinimo, GameProperties.cameraWidth - margineMinimo);

        float nuovoY = ultimoPunto.Y + incrementoAltezza;

        puntiSpline.Add(new Vector2(nuovoX, nuovoY));
    }

    public override void Update()
    {
        if(Input.IsKeyPressed(KeyboardKey.R))
        {
            Reset();

            for (int i = 0; i < 100; i++)
                Crescita();

            Rendering.camera.position.Y = 0;
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

    public PlantSaveData ToSaveData()
    {
        var data = new PlantSaveData();
        data.PuntiSpline = new List<Vector2>(puntiSpline);
        data.EderaCreata = ederaCreata;
        data.ContatorePuntiPerRamo = contatorePuntiPerRamo;
        data.ContatorePuntiPerRadice = contatorePuntiPerRadice;

        foreach (var ramo in rami)
            data.Rami.Add(ramo.ToSaveData());

        foreach (var radice in radici)
            data.Radici.Add(radice.ToSaveData());

        foreach (var edera in ramiEdera)
            data.Edera.Add(edera.ToSaveData());

        return data;
    }

    public void RestoreFromSaveData(PlantSaveData data)
    {
        foreach (var item in rami)
            item.Destroy();
        foreach (var item in radici)
            item.Destroy();
        foreach (var item in ramiEdera)
            item.Destroy();

        rami.Clear();
        radici.Clear();
        ramiEdera.Clear();
        puntiSpline.Clear();

        puntiSpline.AddRange(data.PuntiSpline);
        ederaCreata = data.EderaCreata;
        contatorePuntiPerRamo = data.ContatorePuntiPerRamo;
        contatorePuntiPerRadice = data.ContatorePuntiPerRadice;

        if (puntiSpline.Count > 0)
            posizione = puntiSpline[0];

        foreach (var ramoData in data.Rami)
        {
            var ramo = Obj_Ramo.FromSaveData(ramoData);
            ramo.roomId = Game.room_main.id;
            rami.Add(ramo);
        }

        foreach (var radiceData in data.Radici)
        {
            var radice = Obj_Radice.FromSaveData(radiceData);
            radice.roomId = Game.room_main.id;
            radici.Add(radice);
        }

        foreach (var ederaData in data.Edera)
        {
            var edera = Obj_RamoEdera.FromSaveData(ederaData, colore1);
            edera.roomId = Game.room_main.id;
            ramiEdera.Add(edera);
        }
    }
}

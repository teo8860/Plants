using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public List<Obj_Ramo> rami = new();
    public List<Obj_Radice> radici = new();
    private int contatorePuntiPerRamo = 0;
    private int contatorePuntiPerRadice = 0;

    private List<Obj_RamoEdera> ramiEdera = new();
    private bool ederaCreata = false;

    public float spessore;

    DayPhase Fase = Game.Phase;

    public SeedType TipoSeme = SeedType.Normale;
    public SeedStats seedBonus = new SeedStats();
    public List<string> equippedItemIds = new() { null, null, null };

    public PlantStats Stats = new PlantStats();

    public GameLogicPianta proprieta;

    public Color colore1, colore2;

    public float nextWorldGroundY => Stats.EffectiveMaxHeight + GameProperties.groundHeight - 10;

    // Foglie dorate - minigioco
    private const float GOLDEN_LEAF_BASE_CHANCE = 0.008f; // 0.8% per foglia che cresce
    private int fogliaDorata_ramoIdx = -1;
    private int fogliaDorata_fogliaIdx = -1;
    private const float CLICK_DISTANCE = 18f; // generoso in coordinate mondo (~100px wide)

    // Animazione click foglia dorata
    private bool animazioneDorata = false;
    private float animazioneDorataTimer = 0f;
    private const float ANIMAZIONE_DORATA_DURATA = 0.6f;
    private Vector2 animazioneDorataPos = Vector2.Zero;
    private TipoMinigioco animazioneDorataTipo;


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

        ItemHookCaller.CallOnGrow(this);

        if (Stats.FoglieAttuali < proprieta.FoglieMassime)
        {
            float probabilitaFoglia = velocita * 0.15f * (1f - (float)Stats.FoglieAttuali / proprieta.FoglieMassime);
            if (RandomHelper.DeterministicFloatRangeAt(rseed, Stats.FoglieAttuali + puntiSpline.Count, 0, 1) < probabilitaFoglia)
            {
                Stats.FoglieAttuali++;
                ItemHookCaller.CallOnLeafNew(this);
                TentaFogliaDorata();
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
            ItemHookCaller.CallOnBranchNew(this);

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
        ItemHookCaller.CallOnBranchGrow(this);
        ItemHookCaller.CallOnLeafGrow(this);

        foreach (var radice in radici)
        {
            radice.Cresci();
        }

        if (!ederaCreata && Stats.Altezza >= Stats.AltezzaMassima * 0.95f)
        {
            CreaEdera();
            ederaCreata = true;
        }

        if (!Game.IsOfflineSimulation && Game.controller.targetScrollY <= Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier && Game.controller.autoscroll == true)
        {
            float cameraDistFromTarget = Math.Abs(Rendering.camera.position.Y - Game.controller.targetScrollY);
            if (cameraDistFromTarget < 20f)
            {
                Game.controller.targetScrollY += incrementoFinale;
            }
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

    // --- Metodi per il sistema di recupero seme ---

    public int GetSplineCount() => puntiSpline.Count;
    public int GetBranchCount() => rami.Count;
    public int GetRootCount() => radici.Count;

    /// <summary>
    /// Rimuove l'ultimo punto spline e riduce proporzionalmente rami, radici, foglie e edera.
    /// Ritorna false se non ci sono piu' punti da rimuovere.
    /// </summary>
    public bool RewindStep(int initSpline, int initBranches, int initRoots, int initLeaves)
    {
        if (puntiSpline.Count <= 3) return false;

        puntiSpline.RemoveAt(puntiSpline.Count - 1);

        // Ricalcola altezza dall'ultimo punto
        if (puntiSpline.Count > 1)
            Stats.Altezza = Math.Max(0, puntiSpline[^1].Y - posizione.Y);
        else
            Stats.Altezza = 0;

        // Rapporto crescita rimanente (esclusi i 3 punti iniziali)
        float ratio = initSpline > 3
            ? (float)(puntiSpline.Count - 3) / (initSpline - 3)
            : 0f;

        // Rimuovi rami proporzionalmente
        int targetBranches = (int)(initBranches * ratio);
        while (rami.Count > targetBranches && rami.Count > 0)
        {
            rami[^1].Destroy();
            rami.RemoveAt(rami.Count - 1);
        }

        // Rimuovi radici proporzionalmente
        int targetRoots = (int)(initRoots * ratio);
        while (radici.Count > targetRoots && radici.Count > 0)
        {
            radici[^1].Destroy();
            radici.RemoveAt(radici.Count - 1);
        }

        // Rimuovi edera sotto il 90% dell'altezza massima
        if (ederaCreata && Stats.Altezza < Stats.AltezzaMassima * 0.90f)
        {
            foreach (var e in ramiEdera) e.Destroy();
            ramiEdera.Clear();
            ederaCreata = false;
        }

        // Foglie proporzionali
        Stats.FoglieAttuali = (int)(initLeaves * ratio);

        // Resetta foglia dorata
        fogliaDorata_ramoIdx = -1;
        fogliaDorata_fogliaIdx = -1;
        animazioneDorata = false;

        // Camera segue il rewind verso il basso
        if (!Game.IsOfflineSimulation)
        {
            Game.controller.targetScrollY = Math.Max(0, Stats.Altezza - 30);
        }

        return true;
    }

    public void ControlloCrescita()
    {
        if (Game.isPaused && !Game.IsOfflineSimulation) return;
        if (SeedRecoverySystem.IsRewinding) return;

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
        Game.controller.targetScrollY = 0;
        Rendering.camera.Update();
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
        fogliaDorata_ramoIdx = -1;
        fogliaDorata_fogliaIdx = -1;
        animazioneDorata = false;

        Stats.Altezza = 0;
        Stats.FoglieAttuali = 0;

        proprieta.Reset();
        PosizionaAlCentroInBasso();
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
        float dt = Raylib_CSharp.Time.GetFrameTime();

        // Animazione click in corso → aspetta che finisca, poi lancia il minigioco
        if (animazioneDorata)
        {
            animazioneDorataTimer += dt;
            if (animazioneDorataTimer >= ANIMAZIONE_DORATA_DURATA)
            {
                animazioneDorata = false;
                ManagerMinigames.AvviaProcesso(animazioneDorataTipo);
            }
            return; // non processare click durante l'animazione
        }

        // Click su foglia dorata → avvia animazione
        if (Input.IsMouseButtonPressed(Raylib_CSharp.Interact.MouseButton.Left) && !ManagerMinigames.InCorso)
        {
            ControllaClickFogliaDorata();
        }

        // Controlla risultati minigioco tornato
        ControllaRisultatoMinigioco();
    }

    /// <summary>
    /// Rende una foglia casuale dorata (debug).
    /// </summary>
    public void RendiFogliaDorataCasuale()
    {
        var candidati = new List<(int ramoIdx, int fogliaIdx)>();
        for (int r = 0; r < rami.Count; r++)
        {
            var indici = rami[r].GetIndiciFoglieValide();
            foreach (int fi in indici)
            {
                if (!rami[r].IsFogliaDorata(fi))
                    candidati.Add((r, fi));
            }
        }

        if (candidati.Count == 0) return;

        var (ri, fli) = candidati[RandomHelper.Int(0, candidati.Count)];
        rami[ri].SetFogliaDorata(fli, true);
        Console.WriteLine($"[DEBUG] Foglia dorata su ramo {ri}, foglia {fli}");
    }

    private void TentaFogliaDorata()
    {
        float chance = GOLDEN_LEAF_BASE_CHANCE * seedBonus.vegetazione;

        if (RandomHelper.Float(0, 1) < chance && rami.Count > 0)
        {
            var ultimoRamo = rami[^1];
            var indici = ultimoRamo.GetIndiciFoglieValide();
            if (indici.Count > 0)
            {
                int lastIdx = indici[^1];
                if (!ultimoRamo.IsFogliaDorata(lastIdx))
                {
                    ultimoRamo.SetFogliaDorata(lastIdx, true);
                }
            }
        }
    }

    private void ControllaClickFogliaDorata()
    {
        Vector2 mouse = Input.GetMousePosition();
        mouse = CoordinateHelper.ToWorld(mouse, Rendering.camera.position);

        // Trova la foglia dorata più vicina al click
        float distMin = float.MaxValue;
        int bestRamo = -1, bestFoglia = -1;
        Vector2 bestPos = Vector2.Zero;

        for (int r = 0; r < rami.Count; r++)
        {
            var dorate = rami[r].GetIndiciFoglieDorate();
            foreach (int fi in dorate)
            {
                Vector2 pos = rami[r].GetPosizioneFoglia(fi);
                if (pos == Vector2.Zero) continue;

                float dist = Vector2.Distance(mouse, pos);
                if (dist < distMin)
                {
                    distMin = dist;
                    bestRamo = r;
                    bestFoglia = fi;
                    bestPos = pos;
                }
            }
        }

        if (bestRamo >= 0 && distMin <= CLICK_DISTANCE)
        {
            // Salva foglia cliccata
            fogliaDorata_ramoIdx = bestRamo;
            fogliaDorata_fogliaIdx = bestFoglia;

            // Scegli minigioco casuale
            var tipi = ManagerMinigames.GetTipiDisponibili();
            animazioneDorataTipo = tipi[RandomHelper.Int(0, tipi.Count)];

            // Avvia animazione prima di aprire il minigioco
            animazioneDorata = true;
            animazioneDorataTimer = 0f;
            animazioneDorataPos = bestPos;
        }
    }

    private void ControllaRisultatoMinigioco()
    {
        if (fogliaDorata_ramoIdx >= 0 && !ManagerMinigames.InCorso)
        {
            if (fogliaDorata_ramoIdx < rami.Count)
            {
                rami[fogliaDorata_ramoIdx].SetFogliaDorata(fogliaDorata_fogliaIdx, false);
            }
            fogliaDorata_ramoIdx = -1;
            fogliaDorata_fogliaIdx = -1;
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

        // Animazione burst al click su foglia dorata
        if (animazioneDorata && ViewCulling.IsValueVisible(animazioneDorataPos.Y, cameraY))
        {
            DrawAnimazioneDorata();
        }
    }

    private void DrawAnimazioneDorata()
    {
        float t = animazioneDorataTimer / ANIMAZIONE_DORATA_DURATA; // 0→1
        Vector2 pos = animazioneDorataPos;

        // Cerchi che si espandono verso l'esterno
        float raggio1 = t * 25f;
        float raggio2 = Math.Max(0, (t - 0.15f)) * 20f;
        float raggio3 = Math.Max(0, (t - 0.3f)) * 15f;
        byte alpha1 = (byte)(200 * (1f - t));
        byte alpha2 = (byte)(160 * Math.Max(0, 1f - (t / 0.85f)));
        byte alpha3 = (byte)(120 * Math.Max(0, 1f - ((t - 0.15f) / 0.85f)));

        Graphics.DrawCircleV(pos, raggio1, new Color(255, 240, 80, alpha1));
        if (t > 0.15f)
            Graphics.DrawCircleV(pos, raggio2, new Color(255, 255, 150, alpha2));
        if (t > 0.3f)
            Graphics.DrawCircleV(pos, raggio3, new Color(255, 255, 220, alpha3));

        // Raggi/particelle che partono dal centro
        int numRaggi = 8;
        for (int i = 0; i < numRaggi; i++)
        {
            float angolo = (MathF.PI * 2f / numRaggi) * i + t * 1.5f;
            float distanza = t * 18f;
            float lunghezza = 3f * (1f - t);
            byte alphaRaggio = (byte)(255 * (1f - t));

            Vector2 start = pos + new Vector2(MathF.Cos(angolo) * distanza, MathF.Sin(angolo) * distanza);
            Vector2 end = pos + new Vector2(MathF.Cos(angolo) * (distanza + lunghezza), MathF.Sin(angolo) * (distanza + lunghezza));

            Graphics.DrawLineEx(start, end, 1.5f * (1f - t), new Color(255, 230, 50, alphaRaggio));
        }

        // Flash bianco centrale all'inizio
        if (t < 0.3f)
        {
            float flashAlpha = (1f - t / 0.3f) * 200f;
            Graphics.DrawCircleV(pos, 4f * (1f - t / 0.3f), new Color(255, 255, 255, (byte)flashAlpha));
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

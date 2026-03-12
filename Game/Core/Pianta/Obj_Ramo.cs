using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

struct ParametriFoglie
{
    public float scostamento;
    public float rotazione;
    public float flip;
    public Vector2 posizioneRelativa;
    public float dimensione;
    public float faseOscillazione;
    public bool isDorata;

    public ParametriFoglie(float Scostamento, float Rotazione, float Flip, Vector2 PosizioneRelativa, float Dimensione = 1f)
    {
        this.scostamento = Scostamento;
        this.rotazione = Rotazione;
        this.flip = Flip;
        this.posizioneRelativa = PosizioneRelativa;
        this.dimensione = Dimensione;
        this.faseOscillazione = RandomHelper.Float(0, MathF.PI * 2);
        this.isDorata = false;
    }
}

public enum Direzione
{
    Sinistra = -1,
    Destra = 1
}

public class Obj_Ramo: GameElement
{
    private List<Vector2> punti = new();
    private Direzione direzione;
    private Vector2 puntoIniziale;

    private int crescitaAttuale = 0;
    private int maxCrescita = 6;
    private float tempoVita = 0;

    private float spessoreBase = 5.0f;
    private float spessoreAttuale = 5.0f;
    private const float decrementoSpessore = 0.6f;

    private const int fogliePerSegmento = 1;
    private List<ParametriFoglie> parametriFoglie = new();

    private float minY = float.MaxValue;
    private float maxY = float.MinValue;

    private Color coloreRamo = Color.DarkBrown;
    private Color coloreRamoChiaro = new Color(100, 70, 50, 255);

    public Obj_Ramo(Vector2 puntoIniziale, Direzione direzione, bool restore = false)
    {
        this.direzione = direzione;
        this.puntoIniziale = puntoIniziale;

        if (!restore)
        {
            this.punti.Add(puntoIniziale);
            maxCrescita = 5 + RandomHelper.DeterministicIntRange(Game.pianta.rseed, 4, 0, 3);
        }

        UpdateBounds();
    }

    private void UpdateBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        foreach (var p in punti)
        {
            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;
        }
        minY -= 20;
        maxY += 20;
    }

    public void Cresci()
    {
        if (crescitaAttuale >= maxCrescita) return;

        Vector2 ultimoPunto = punti[^1];

        float deltaXBase = 10 + RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, punti.Count, 0, 5);
        float deltaX = deltaXBase * (direzione == Direzione.Destra ? 1 : -1);

        float progressione = (float)crescitaAttuale / maxCrescita;
        float deltaY = 8 + RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, punti.Count, 0, 5) - progressione * 3;

        Vector2 nuovoPunto = new Vector2(ultimoPunto.X + deltaX, ultimoPunto.Y + deltaY);

        nuovoPunto.X = Math.Clamp(nuovoPunto.X, 15, GameProperties.cameraWidth - 15);

        punti.Add(nuovoPunto);
        crescitaAttuale++;

        GeneraParametriFoglia();
        UpdateBounds();
    }

    private void GeneraParametriFoglia()
    {
        if (punti.Count < 2) return;

        float flip;
        float centroSchermoX = GameProperties.cameraWidth / 2.0f;
        const float margineSicurezza = 60.0f;

        if (puntoIniziale.X < centroSchermoX - margineSicurezza)
        {
            flip = -1.0f;
        }
        else if (puntoIniziale.X > centroSchermoX + margineSicurezza)
        {
            flip = 1.0f;
        }
        else
        {
            flip = puntoIniziale.X < centroSchermoX ? -1.0f : 1.0f;
        }

        if (RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, parametriFoglie.Count, 0, 1) < 0.25f)
            flip *= -1.0f;

        float rotazioneCasuale = RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, parametriFoglie.Count, -25f, 25f);
        float scostamentoCasuale = RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, parametriFoglie.Count, 4f, 8f);
        float posizioneRelativa = 0.4f + RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, parametriFoglie.Count, 0f, 0.3f);
        float dimensione = 0.6f + RandomHelper.DeterministicFloatRangeAt(Game.pianta.rseed, parametriFoglie.Count, 0f, 0.3f);

        parametriFoglie.Add(new ParametriFoglie(
            scostamentoCasuale,
            rotazioneCasuale,
            flip,
            new Vector2(posizioneRelativa, 0),
            dimensione
        ));
    }

    public override void Draw()
    {
        float cameraY = Rendering.camera.position.Y;

        if (!ViewCulling.IsValueVisible(minY, cameraY) &&
            !ViewCulling.IsValueVisible(maxY, cameraY))
            return;

        if (punti.Count < 2) return;

        tempoVita += Time.GetFrameTime();

        Span<Vector2> puntiSpan = stackalloc Vector2[punti.Count];
        for (int i = 0; i < punti.Count; i++)
        {
            puntiSpan[i] = punti[i];
        }

        Sprite sprite = AssetLoader.spriteLeaf;

        float distanzaDaBase = puntoIniziale.Y - GameProperties.groundPosition;
        float altezzaPianta = Math.Max(100, Game.pianta.Stats.Altezza);
        float fattoreAltezza = Math.Clamp(distanzaDaBase / altezzaPianta, 0f, 1f);

        spessoreBase = Math.Max(2f, 7f - fattoreAltezza * 4f);

        for (int i = 0; i < puntiSpan.Length - 1; i++)
        {
			Vector2 pStart = puntiSpan[i];
            Vector2 pEnd = puntiSpan[i + 1];

            float oscillazione = MathF.Sin(tempoVita * 1.2f + i * 0.4f) * 5f;
            float fattoreOscillazione = (float)(i + 1) / puntiSpan.Length;
            pStart.X += oscillazione * fattoreOscillazione * 0.5f;
            pEnd.X += oscillazione * fattoreOscillazione;

            float fattoreSegmento = (float)(puntiSpan.Length - 1 - i) / Math.Max(1, puntiSpan.Length - 1);
            spessoreAttuale = spessoreBase * (0.25f + fattoreSegmento * 0.6f);
            spessoreAttuale = Math.Max(1f, spessoreAttuale);

            if (spessoreAttuale > 1.5f)
            {
                Graphics.DrawLineEx(
                    new Vector2(pStart.X + 1, pStart.Y + 1),
                    new Vector2(pEnd.X + 1, pEnd.Y + 1),
                    spessoreAttuale + 0.5f,
                    new Color(40, 25, 15, 100)
                );
            }

            Graphics.DrawLineEx(pStart, pEnd, spessoreAttuale, coloreRamo);

            if (spessoreAttuale > 2.5f)
            {
                Graphics.DrawLineEx(pStart, pEnd, spessoreAttuale * 0.4f, coloreRamoChiaro);
            }

            DrawFoglia(sprite, i, pStart, pEnd, oscillazione);
        }
    }

    private static readonly Color gialloDorato = new Color(255, 230, 50, 255);
    private static readonly Color gialloBrillante = new Color(255, 255, 150, 255);
    private static readonly Color auraEsterna = new Color(255, 220, 50, 40);
    private static readonly Color auraInterna = new Color(255, 240, 100, 70);
    private static readonly Color scintillaColor = new Color(255, 255, 200, 255);

    /// <summary>
    /// Calcola la posizione mondo di una foglia (senza oscillazione animata).
    /// </summary>
    public Vector2 GetPosizioneFoglia(int indice)
    {
        if (indice >= parametriFoglie.Count || indice < 1) return Vector2.Zero;
        if (punti.Count < 2 || indice >= punti.Count - 1) return Vector2.Zero;

        var paramsFoglia = parametriFoglie[indice];
        Vector2 pStart = punti[indice];
        Vector2 pEnd = punti[indice + 1];
        Vector2 posizioneRamo = Vector2.Lerp(pStart, pEnd, paramsFoglia.posizioneRelativa.X);

        float deltaY = pEnd.Y - pStart.Y;
        float deltaX = pEnd.X - pStart.X;
        float rotazioneBase = MathF.Atan2(deltaY, deltaX) * (180.0f / MathF.PI);
        float angoloPerpendicolareRad = rotazioneBase * (MathF.PI / 180.0f);

        Vector2 offset = new Vector2(
            MathF.Cos(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip,
            MathF.Sin(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip
        );

        return posizioneRamo + offset;
    }

    public bool IsFogliaDorata(int indice)
    {
        if (indice < 0 || indice >= parametriFoglie.Count) return false;
        return parametriFoglie[indice].isDorata;
    }

    public void SetFogliaDorata(int indice, bool dorata)
    {
        if (indice < 0 || indice >= parametriFoglie.Count) return;
        var p = parametriFoglie[indice];
        p.isDorata = dorata;
        parametriFoglie[indice] = p;
    }

    /// <summary>
    /// Restituisce gli indici delle foglie valide (indice >= 1).
    /// </summary>
    public List<int> GetIndiciFoglieValide()
    {
        var result = new List<int>();
        for (int i = 1; i < parametriFoglie.Count; i++)
            result.Add(i);
        return result;
    }

    /// <summary>
    /// Restituisce gli indici delle foglie dorate.
    /// </summary>
    public List<int> GetIndiciFoglieDorate()
    {
        var result = new List<int>();
        for (int i = 0; i < parametriFoglie.Count; i++)
            if (parametriFoglie[i].isDorata)
                result.Add(i);
        return result;
    }

    private void DrawFoglia(Sprite sprite, int indice, Vector2 pStart, Vector2 pEnd, float oscillazione)
    {
        if (sprite == null || sprite.texture.Width <= 0) return;
        if (indice >= parametriFoglie.Count) return;
        if (indice < 1) return;

        var paramsFoglia = parametriFoglie[indice];
        Vector2 posizioneRamo = Vector2.Lerp(pStart, pEnd, paramsFoglia.posizioneRelativa.X);

        float deltaY = pEnd.Y - pStart.Y;
        float deltaX = pEnd.X - pStart.X;
        float rotazioneBase = MathF.Atan2(deltaY, deltaX) * (180.0f / MathF.PI);

        float angoloPerpendicolareRad = rotazioneBase * (MathF.PI / 180.0f);
        Vector2 offset = new Vector2(
            MathF.Cos(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip,
            MathF.Sin(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip
        );

        Vector2 posizioneFinale = posizioneRamo + offset;

        float oscFoglia = MathF.Sin(tempoVita * 2f + paramsFoglia.faseOscillazione) * 8f;
        float rotazioneFinale = rotazioneBase + paramsFoglia.rotazione + oscFoglia;

        float scala = paramsFoglia.dimensione * 0.65f;

        if (paramsFoglia.isDorata)
        {
            float pulse = (MathF.Sin(tempoVita * 3f + paramsFoglia.faseOscillazione) + 1f) * 0.5f;
            float pulse2 = (MathF.Sin(tempoVita * 4.5f + paramsFoglia.faseOscillazione + 1f) + 1f) * 0.5f;

            // Centro visivo della foglia: offset dallo stem lungo la direzione della rotazione
            float rotRad = rotazioneFinale * MathF.PI / 180f;
            float offsetCentro = scala * 5f; // circa metà della lunghezza foglia
            Vector2 centroFoglia = posizioneFinale + new Vector2(
                MathF.Cos(rotRad) * offsetCentro,
                MathF.Sin(rotRad) * offsetCentro
            );

            // Alone esterno grande pulsante centrato sulla foglia
            Graphics.DrawCircleV(centroFoglia, 7f + pulse * 3f, auraEsterna);
            // Alone interno più luminoso
            Graphics.DrawCircleV(centroFoglia, 4f + pulse2 * 2f, auraInterna);

            // Foglia gialla brillante, leggermente più grande
            float scalaDorata = scala * 1.15f;
            GameFunctions.DrawSprite(sprite, posizioneFinale, rotazioneFinale, new Vector2(scalaDorata, scalaDorata), gialloDorato);

            // Scintille che orbitano attorno al centro della foglia
            for (int s = 0; s < 3; s++)
            {
                float angolo = tempoVita * (2.5f + s * 0.7f) + s * MathF.PI * 0.66f + paramsFoglia.faseOscillazione;
                float raggio = 3f + pulse * 1.5f;
                float sparkAlpha = (MathF.Sin(tempoVita * 4f + s * 2f) + 1f) * 0.5f;
                if (sparkAlpha > 0.5f)
                {
                    float sparkSize = 0.8f + (sparkAlpha - 0.5f) * 2f;
                    Vector2 sparkPos = centroFoglia + new Vector2(
                        MathF.Cos(angolo) * raggio,
                        MathF.Sin(angolo) * raggio
                    );
                    Graphics.DrawCircleV(sparkPos, sparkSize, scintillaColor);
                }
            }

            // Lampo bianco intermittente
            if (pulse > 0.85f)
            {
                float flashSize = (pulse - 0.85f) * 15f;
                Graphics.DrawCircleV(centroFoglia, flashSize, new Color(255, 255, 255, 60));
            }
        }
        else
        {
            GameFunctions.DrawSprite(sprite, posizioneFinale, rotazioneFinale, new Vector2(scala, scala));
        }
    }

    public bool IsComplete => crescitaAttuale >= maxCrescita;
    public int NumeroFoglie => parametriFoglie.Count;

    public RamoSaveData ToSaveData()
    {
        var data = new RamoSaveData { Punti = new List<Vector2>(punti) };
        data.FoglieDorate = GetIndiciFoglieDorate();
        return data;
    }

    public static Obj_Ramo FromSaveData(RamoSaveData data)
    {
        Direzione dir = data.Punti.Count >= 2 && data.Punti[1].X >= data.Punti[0].X
            ? Direzione.Destra : Direzione.Sinistra;

        var ramo = new Obj_Ramo(data.Punti[0], dir, restore: true);
        ramo.punti = new List<Vector2>(data.Punti);
        ramo.crescitaAttuale = data.Punti.Count - 1;
        ramo.maxCrescita = 5 + RandomHelper.DeterministicIntRange(Game.pianta.rseed, 4, 0, 3);

        for (int i = 1; i < ramo.punti.Count; i++)
            ramo.GeneraParametriFoglia();

        // Ripristina foglie dorate
        if (data.FoglieDorate != null)
        {
            foreach (int idx in data.FoglieDorate)
                ramo.SetFogliaDorata(idx, true);
        }

        ramo.UpdateBounds();
        return ramo;
    }
}
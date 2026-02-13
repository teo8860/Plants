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

    public ParametriFoglie(float Scostamento, float Rotazione, float Flip, Vector2 PosizioneRelativa, float Dimensione = 1f)
    {
        this.scostamento = Scostamento;
        this.rotazione = Rotazione;
        this.flip = Flip;
        this.posizioneRelativa = PosizioneRelativa;
        this.dimensione = Dimensione;
        this.faseOscillazione = RandomHelper.Float(0, MathF.PI * 2);
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

    public Obj_Ramo(Vector2 puntoIniziale, Direzione direzione)
    {
        this.direzione = direzione;
        this.puntoIniziale = puntoIniziale;
        this.punti.Add(puntoIniziale);

        maxCrescita = 5 + RandomHelper.DeterministicIntRange(Game.pianta.rseed, 4, 0, 3);

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

        for (int i = 0; i < punti.Count - 1; i++)
        {
            if(puntiSpan.Length < i +1) continue;

			Vector2 pStart = puntiSpan[i];
            Vector2 pEnd = puntiSpan[i + 1];

            float oscillazione = MathF.Sin(tempoVita * 1.2f + i * 0.4f) * 5f;
            float fattoreOscillazione = (float)(i + 1) / punti.Count;
            pStart.X += oscillazione * fattoreOscillazione * 0.5f;
            pEnd.X += oscillazione * fattoreOscillazione;

            float fattoreSegmento = (float)(punti.Count - 1 - i) / Math.Max(1, punti.Count - 1);
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

        GameFunctions.DrawSprite(sprite, posizioneFinale, rotazioneFinale, new Vector2(scala, scala));
    }

    public bool IsComplete => crescitaAttuale >= maxCrescita;
    public int NumeroFoglie => parametriFoglie.Count;
}
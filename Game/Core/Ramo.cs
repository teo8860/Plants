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

    public ParametriFoglie(float Scostamento, float Rotazione, float Flip, Vector2 PosizioneRelativa)
    {
        this.scostamento = Scostamento;
        this.rotazione = Rotazione;
        this.flip = Flip;
        this.posizioneRelativa = PosizioneRelativa;
    }
}

public enum Direzione
{
    Sinistra = -1,
    Destra = 1
}

public class Ramo
{
    private List<Vector2> punti = new();
    private Direzione direzione;

    private int crescitaAttuale = 0;
    private const int maxCrescita = 5;

    private float spessoreAttuale = 2.0f; 
    private const float incrementoSpessore = 1.5f;

    private const int fogliePerSegmento = 1;
    private List<ParametriFoglie> parametriFoglie = new();
    private bool foglieGenerate = false;

    private Vector2 puntoIniziale;

    private int scaleX = RandomHelper.Int(0,100) > 50 ? 1 : -1;

    public Ramo(Vector2 puntoIniziale, Direzione direzione)
    {
        this.direzione = direzione;
        this.punti.Add(puntoIniziale);
        this.puntoIniziale = puntoIniziale;
    }

    public void Cresci()
    {
        if (crescitaAttuale >= maxCrescita) return;

        Vector2 ultimoPunto = punti[^1];

        float deltaX = RandomHelper.Int(20, 20) * (direzione == Direzione.Destra? 1 : -1);
        float deltaY = -RandomHelper.Int(10, 15);

        Vector2 nuovoPunto = new Vector2(ultimoPunto.X + deltaX, ultimoPunto.Y + deltaY);
        punti.Add(nuovoPunto);

        spessoreAttuale += incrementoSpessore;
        crescitaAttuale++;

        GeneraParametriFoglia();
    }

    private void GeneraParametriFoglia()
    {
        if (punti.Count >= 2)
        {
            float flip;
            float centroSchermoX = GameProperties.windowWidth / 2.0f;
            const float margineSicurezza = 80.0f;

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
                if (puntoIniziale.X < centroSchermoX)
                    flip = -1.0f;
                else
                    flip = 1.0f;
            }

            float rotazioneCasuale = RandomHelper.Float(0,1) * 40f - 20f;
            float scostamentoCasuale = RandomHelper.Float(0,1) * 5f + 5f;
            float posizioneRelativa = 0.5f;

            if (Math.Abs(puntoIniziale.X - centroSchermoX) <= margineSicurezza)
            {
                if (RandomHelper.Float(0,1) < 0.2)
                    flip *= -1.0f;
            }

            parametriFoglie.Add(new ParametriFoglie(scostamentoCasuale, rotazioneCasuale, flip, new Vector2(posizioneRelativa, 0)));
        }
    }

    public void Draw(float offsetY)
    {
        if(punti.Count < 2)
            return;

        Span<Vector2> puntiOffset = stackalloc Vector2[punti.Count];
        for (int i = 0; i < punti.Count; i++)
        {
            puntiOffset[i] = new Vector2(punti[i].X, punti[i].Y + offsetY);
        }

        Sprite sprite = AssetLoader.spriteLeaf;

        for (int i = 0; i < punti.Count - 1; i++)
        {
            Vector2 pStart = puntiOffset[i];
            Vector2 pEnd = puntiOffset[i + 1];

            pStart.X += (float)Math.Sin(Time.GetTime())*10f;
            pEnd.X += (float)Math.Sin(Time.GetTime())*10f;
               
            Graphics.DrawLineEx(pStart, pEnd, spessoreAttuale, Color.DarkGreen);
            int w = sprite.texture.Width;
            int h = sprite.texture.Height;

            if (w > 0 && h > 0)
            {
                var paramsFoglia = parametriFoglie[i];
                float scala = 0.8f;

                Vector2 posizioneRamo = Vector2.Lerp(pStart, pEnd, paramsFoglia.posizioneRelativa.X);

                float deltaY = pEnd.Y - pStart.Y;
                float deltaX = pEnd.X - pStart.X;
                float rotazioneBase = MathF.Atan2(deltaY, deltaX) * (180.0f / MathF.PI);

                float angoloPerpendicolareRad = (rotazioneBase ) * (MathF.PI / 180.0f);

                Vector2 offset = new Vector2(
                    MathF.Cos(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip,
                    MathF.Sin(angoloPerpendicolareRad) * paramsFoglia.scostamento * paramsFoglia.flip
                );

                Vector2 posizioneFinale = posizioneRamo + offset;
                float rotazioneFinale = rotazioneBase + paramsFoglia.rotazione;

                if(i > 1)
                    GameFunctions.DrawSprite(sprite, posizioneFinale, rotazioneFinale, new Vector2(1,1));
            }
            else
            {
                Vector2 midPoint = (pStart + pEnd) / 2.0f;
            //    Graphics.DrawCircleV(midPoint, 8, Color.Lime);
            }
        }

        for (int i = 0; i < punti.Count; i++)
        {
          //  Graphics.DrawCircleV(puntiOffset[i], spessoreAttuale / 2, Color.DarkGreen);
        }
    }
}
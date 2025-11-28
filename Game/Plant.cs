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
 
    public float Idratazione = 0;
    public float Altezza = 1.0f;
    public (float X, float Y) Posizione = (0, 0);

    private List<Vector2> puntiSpline = new(); 
    private Random random = new();
    private const int MargineMinimo = 40;

    private List<Ramo> rami = new(); 
    private int contatorePuntiPerRamo = 0; 
    private bool textureCaricata = false;
     
    public Plant()
    {
        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

    }

    public void PosizionaAlCentroInBasso()
    {
        float centroX = 0.5f;
        float bassoY = 0.04f;
        Posizione = (centroX, bassoY);
    }

    public void Crescita(float incremento)
    {
        if (incremento > 0)
        {
            Altezza += incremento;
            GeneraPuntoCasuale();

            contatorePuntiPerRamo++;
            if (contatorePuntiPerRamo == 5)
            {
                Vector2 puntoAttacco = puntiSpline[^2];

                bool vaADestra;

                float margineSicurezza = 100f;

                if (puntoAttacco.X < margineSicurezza)
                {
                    vaADestra = true;
                }
                else if (puntoAttacco.X > GameProperties.screenWidth - margineSicurezza)
                {
                    vaADestra = false;
                }
                else
                {
                    vaADestra = random.Next(0, 2) == 0;
                }

                rami.Add(new Ramo(puntoAttacco, vaADestra, random, true));

                contatorePuntiPerRamo = 0;
            }

            foreach (var ramo in rami)
            {
                ramo.Cresci();
            }

            Vector2 ultimoPunto = puntiSpline[^1];
            if (ultimoPunto.Y + Game.controller.offsetY < 100)
            {
                Game.controller.offsetY += 50;
            }

        }
    }

    public void Annaffia()
    {
        float incrementoCasuale = (float)random.NextDouble() * 0.5f + 0.1f; 
        Crescita(incrementoCasuale);
    }

    public void Reset()
    {
        Idratazione = 0;
        Altezza = 1.0f;
        Game.controller.offsetY = 0;
        puntiSpline.Clear();
        rami.Clear();
        GeneraPuntoIniziale();
    }

    private void GeneraPuntoIniziale()
    {
        puntiSpline.Clear();

        puntiSpline.Add(new Vector2(
            Posizione.X * GameProperties.screenWidth,
            GameProperties.screenHeight - Posizione.Y * GameProperties.screenHeight
        ));

        puntiSpline.Add(new Vector2(Posizione.X * GameProperties.screenWidth,
            GameProperties.screenHeight - Posizione.Y * GameProperties.screenHeight));

        float terzoX = Math.Clamp(
            puntiSpline[1].X + random.Next(-50, 50),
            MargineMinimo,
            GameProperties.screenWidth - MargineMinimo
        );
        float terzoY = puntiSpline[1].Y - random.Next(30, 70);
        puntiSpline.Add(new Vector2(terzoX, terzoY));
    }

    private void GeneraPuntoCasuale()
    {
        Vector2 ultimoPunto = puntiSpline[^1];
        float nuovoX = Math.Clamp(ultimoPunto.X + random.Next(-50, 50), MargineMinimo, GameProperties.screenWidth - MargineMinimo);
        float nuovoY = ultimoPunto.Y - random.Next(30, 70);

        puntiSpline.Add(new Vector2(nuovoX, nuovoY));
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

                if(i+4 < puntiConOffset.Length)
                {
                    Span<Vector2> segmento = puntiConOffset.Slice(i, 4);
                    Graphics.DrawSplineCatmullRom(segmento, spessore, Color.Green);
                }
            }


        }
        foreach (var ramo in rami)
        {
            ramo.Draw(Game.controller.offsetY);
        }
    }
}

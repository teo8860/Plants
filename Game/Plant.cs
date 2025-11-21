using Raylib_CSharp.Rendering;
using Raylib_CSharp.Colors;
using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_CSharp;

namespace Plants;

public class Plant : GameElement
{
    public float Idratazione = 0;
    public float Altezza = 1.0f;
    public (float X, float Y) Posizione = (0, 0);

    private List<Vector2> puntiSpline = new(); 
    private Random random = new();
    public float offsetY = 0;
    private const int MargineMinimo = 20;

    public Plant()
    {
        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();
    }

    public void PosizionaAlCentroInBasso()
    {
        float centroX = 0.5f; 
        float bassoY = 0.1f; 
        Posizione = (centroX, bassoY);
    }

    public void Crescita(float incremento)
    {
        if (incremento > 0)
        {
            Altezza += incremento;
            GeneraPuntoCasuale();

            Vector2 ultimoPunto = puntiSpline[^1];
            if (ultimoPunto.Y + offsetY < 100) 
            {
                offsetY += 50;
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
        offsetY = 0;
        puntiSpline.Clear();
        GeneraPuntoIniziale();
    }

    public void Scorri(float delta)
    {
        if (offsetY + delta > 0) { 
            offsetY = offsetY + delta;
        }
    }

    private void GeneraPuntoIniziale()
    {
        puntiSpline.Add(new Vector2(Posizione.X * GameProperties.screenWidth, GameProperties.screenHeight - Posizione.Y * GameProperties.screenHeight));
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
        int totalLines = puntiSpline.Count - 1;

        for (int i = 0; i < totalLines; i++)
        {

            float spessore = 8 + ((totalLines - i) / 5); 

            for (float offset = -spessore / 2; offset <= spessore / 2; offset += 1.0f)
            {
                Graphics.DrawLine(
                    (int)(puntiSpline[i].X + offset), (int)(puntiSpline[i].Y + offsetY),
                    (int)(puntiSpline[i + 1].X + offset), (int)(puntiSpline[i + 1].Y + offsetY),
                    Color.Green
                );
            }
        }
    }
}

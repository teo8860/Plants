using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;



public class Radice
{
    private List<Vector2> punti = new();
    private Vector2 direzione;

    private int crescitaAttuale = 0;
    private const int maxCrescita = 30;
    private int profondita = 0;

    private float spessoreAttuale = 0.5f; 
    private const float incrementoSpessore = 0.1f;

    private Vector2 puntoIniziale;

    private int scaleX = RandomHelper.Int(0,100) > 50 ? 1 : -1;

    private float minY = float.MaxValue;
    private float maxY = float.MinValue;

    public Radice(Vector2 puntoIniziale, Vector2 direzione)
    {
        this.direzione = direzione;
        this.punti.Add(puntoIniziale);
        this.puntoIniziale = puntoIniziale;
        UpdateBounds();
    }

    public void Cresci()
    {
        if (crescitaAttuale >= maxCrescita) return;

        Vector2 ultimoPunto = punti[^1];

        direzione.X += RandomHelper.Int(-2,2);
        direzione.Y -= RandomHelper.Int(1,1);
        Vector2 dir = direzione - ultimoPunto;
        dir = Vector2.Normalize(dir);

        Vector2 nuovoPunto = ultimoPunto - dir * RandomHelper.Int(2, 6);

        punti.Add(nuovoPunto);

        spessoreAttuale += incrementoSpessore;
        crescitaAttuale++;
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
    }

    public bool IsInView(float cameraY)
    {
        return ViewCulling.IsRangeVisible(minY, maxY, cameraY);
    }


    public void Draw()
    {
        if(punti.Count < 2)
            return;

        Span<Vector2> puntiSpan = stackalloc Vector2[punti.Count];
        for (int i = 0; i < punti.Count; i++)
        {
            puntiSpan[i] = new Vector2(punti[i].X, punti[i].Y);
        }

        for (int i = 0; i < punti.Count - 1; i++)
        {
            Vector2 pStart = puntiSpan[i];
            Vector2 pEnd = puntiSpan[i + 1];

               
            Graphics.DrawLineEx(pStart, pEnd, spessoreAttuale, Color.White);
        }
    }
}
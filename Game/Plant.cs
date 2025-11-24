using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Transformations;
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
    private Texture2D textureFoglia; 
    private bool textureCaricata = false;

    public Plant()
    {
        PosizionaAlCentroInBasso();
        GeneraPuntoIniziale();

        try
        {

            Image fogliaImage = Image.Load("../Resources/leaf.png");

            fogliaImage.Resize(60, 60);

            textureFoglia = Texture2D.LoadFromImage(fogliaImage);

            fogliaImage.Unload();

            textureCaricata = true;
        }
        catch
        {
            Console.WriteLine("Impossibile caricare texture foglia. Assicurati che il percorso sia corretto.");
        }

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

                rami.Add(new Ramo(puntoAttacco, vaADestra, random, textureFoglia, textureCaricata));

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

                Span<Vector2> segmento = puntiConOffset.Slice(i, 4);
                Graphics.DrawSplineCatmullRom(segmento, spessore, Color.Green);
            }


        }
        foreach (var ramo in rami)
        {
            ramo.Draw(Game.controller.offsetY);
        }
    }
}
public class Ramo
{
    private List<Vector2> punti = new();
    private bool vaADestra;
    private Random random;

    private int crescitaAttuale = 0;
    private const int MaxCrescita = 5;

    private float spessoreAttuale = 2.0f; 
    private const float IncrementoSpessore = 1.5f;

    private Texture2D textureFoglia;
    private bool haTexture;

    public Ramo(Vector2 puntoIniziale, bool direzioneDestra, Random sharedRandom, Texture2D tex, bool texCaricata)
    {
        vaADestra = direzioneDestra;
        random = sharedRandom;
        textureFoglia = tex;
        haTexture = texCaricata;

        punti.Add(puntoIniziale);
    }

    public void Cresci()
    {
        if (crescitaAttuale >= MaxCrescita) return;

        Vector2 ultimoPunto = punti[^1];

        float deltaX = random.Next(25, 25) * (vaADestra ? 1 : -1);
        float deltaY = -random.Next(5, 25);

        Vector2 nuovoPunto = new Vector2(ultimoPunto.X + deltaX, ultimoPunto.Y + deltaY);
        punti.Add(nuovoPunto);

        spessoreAttuale += IncrementoSpessore;
        crescitaAttuale++;
    }

    public void Draw(float offsetY)
    {
        if (punti.Count < 2) return;

        Span<Vector2> puntiOffset = stackalloc Vector2[punti.Count];
        for (int i = 0; i < punti.Count; i++)
        {
            puntiOffset[i] = new Vector2(punti[i].X, punti[i].Y + offsetY);
        }

        for (int i = 0; i < punti.Count - 1; i++)
        {
            Vector2 pStart = puntiOffset[i];
            Vector2 pEnd = puntiOffset[i + 1];

            Graphics.DrawLineEx(pStart, pEnd, spessoreAttuale, Color.DarkGreen);

            Vector2 midPoint = (pStart + pEnd) / 2.0f;
            //haTexture (non funziona, controllare perchè)
            if (haTexture && textureFoglia.Width > 0 && textureFoglia.Height > 0)
            {

                float deltaY = pEnd.Y - pStart.Y;
                float deltaX = pEnd.X - pStart.X;
                float rotazioneBase = MathF.Atan2(deltaY, deltaX) * (180.0f / MathF.PI);

                float flipLato = (random.Next(0, 2) == 0) ? 1.0f : -1.0f;

                float rotazioneCasuale = (float)random.NextDouble() * 40f - 20f;
                float rotazioneFinale = rotazioneBase + rotazioneCasuale;

                float angoloPerpendicolareRad = (rotazioneBase + 90) * (MathF.PI / 180.0f);

                float scostamentoCasuale = (float)random.NextDouble() * 5f + 5f;

                Vector2 offset = new Vector2(
                    MathF.Cos(angoloPerpendicolareRad) * scostamentoCasuale * flipLato,
                    MathF.Sin(angoloPerpendicolareRad) * scostamentoCasuale * flipLato
                );

                Vector2 posizioneFinale = midPoint + offset;

                float scala = 0.4f;

                Rectangle source = new Rectangle(0, 0, textureFoglia.Width, textureFoglia.Height);
                Rectangle dest = new Rectangle(posizioneFinale.X, posizioneFinale.Y, textureFoglia.Width * scala, textureFoglia.Height * scala);
                Vector2 origin = new Vector2((textureFoglia.Width * scala) / 2, (textureFoglia.Height * scala) / 2);

                Graphics.DrawTexturePro(textureFoglia, source, dest, origin, rotazioneFinale, Color.White);
            }
            else
            {
                Graphics.DrawCircleV(midPoint, 8, Color.Lime);
            }
        }

        for (int i = 0; i < punti.Count; i++)
        {
            Graphics.DrawCircleV(puntiOffset[i], spessoreAttuale / 2, Color.DarkGreen);
        }
    }
}
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Devices.Radios;

namespace Plants;

public class SegmentoRadice
{
    public Vector2 Start;
    public Vector2 End;
    public float Thickness;
    public float Age;
    public bool IsMainRoot;

    public SegmentoRadice(Vector2 start, Vector2 end, float thickness, bool isMain = false)
    {
        Start = start;
        End = end;
        Thickness = thickness;
        Age = 0;
        IsMainRoot = isMain;
    }
}

public class Obj_Radice : GameElement
{
    private List<SegmentoRadice> segmenti = new();
    private List<Obj_Radice> ramificazioni = new();

    private Vector2 origine;
    private Vector2 direzioneBase;
    private float profonditaMassima;
    private float profonditaAttuale = 0;

    private int generazione;
    private float spessoreBase;
    private float velocitaCrescita;
    private int maxRamificazioni;
    private float probabilitaRamificazione;

    private bool completamenteCresta = false;
    private float tempoVita = 0;
    private Random rng;

    private float minY = float.MaxValue;
    private float maxY = float.MinValue;
    private float minX = float.MaxValue;
    private float maxX = float.MaxValue;

    private static readonly Color ColorePrimario = new Color(139, 90, 43, 255);
    private static readonly Color ColoreSecondario = new Color(101, 67, 33, 255);
    private static readonly Color ColorePunta = new Color(180, 140, 100, 255);

    public Obj_Radice(Vector2 origine, Vector2 direzione, int generazione = 0, int seed = -1)
    {
        this.origine = origine;
        this.direzioneBase = Vector2.Normalize(direzione);
        this.generazione = generazione;
        this.rng = seed >= 0 ? new Random(seed) : new Random();

        ConfiguraParametri();
        segmenti.Add(new SegmentoRadice(origine, origine, spessoreBase, generazione == 0));
    }
    ~Obj_Radice()
    {
        foreach (var item in ramificazioni)
            item.Destroy();
    }

    private void ConfiguraParametri()
    {
        float fattoreGenerazione = MathF.Pow(0.65f, generazione);

        spessoreBase = Math.Max(1f, 4f * fattoreGenerazione);
        profonditaMassima = 120f * fattoreGenerazione;
        velocitaCrescita = 2f + (float)rng.NextDouble() * 1.5f;

        maxRamificazioni = Math.Max(0, 4 - generazione);
        probabilitaRamificazione = Math.Max(0, 0.15f - generazione * 0.04f);
    }

    public void Cresci()
    {
        if (completamenteCresta) return;

        tempoVita += 0.016f;

        CresciSegmento();

        foreach (var ramo in ramificazioni)
        {
            ramo.Cresci();
        }

        if (ramificazioni.Count < maxRamificazioni && segmenti.Count > 3)
        {
            TentaRamificazione();
        }

        UpdateBounds();
    }

    private void CresciSegmento()
    {
        if (profonditaAttuale >= profonditaMassima)
        {
            completamenteCresta = true;
            return;
        }

        var ultimo = segmenti[^1];

        Vector2 nuovaDirezione = CalcolaDirezioneOrganica(ultimo.End);

        float lunghezza = velocitaCrescita + (float)rng.NextDouble() * 1.5f;

        if (nuovaDirezione.Y > 0)
        {
            nuovaDirezione.Y = -((float)rng.NextDouble() * 0.4f);
            nuovaDirezione = Vector2.Normalize(nuovaDirezione);
        }

        Vector2 nuovaFine = ultimo.End + nuovaDirezione * lunghezza;

        float limitX = 80f + generazione * 20f;
        nuovaFine.X = Math.Clamp(nuovaFine.X, origine.X - limitX, origine.X + limitX);

        float progressione = profonditaAttuale / profonditaMassima;
        float nuovoSpessore = spessoreBase * (1f - progressione * 0.7f);
        nuovoSpessore = Math.Max(0.5f, nuovoSpessore);

        ultimo.End = nuovaFine;
        ultimo.Thickness = nuovoSpessore;

        float distanza = Vector2.Distance(ultimo.Start, ultimo.End);
        if (distanza > 8f)
        {
            segmenti.Add(new SegmentoRadice(ultimo.End, ultimo.End, nuovoSpessore, generazione == 0));
        }

        profonditaAttuale += lunghezza * Math.Abs(nuovaDirezione.Y);
    }

    private Vector2 CalcolaDirezioneOrganica(Vector2 posizione)
    {
        Vector2 dir = direzioneBase;

        float oscillazione = MathF.Sin(profonditaAttuale * 0.05f + tempoVita) * 0.3f;
        dir.X += oscillazione;

        dir.X += (float)(rng.NextDouble() - 0.5) * 0.4f;
        dir.Y += (float)(rng.NextDouble() - 0.3) * 0.2f;

        dir.Y = Math.Max(0.4f, dir.Y + 0.1f);

        if (segmenti.Count > 5)
        {
            Vector2 mediaPosizioni = Vector2.Zero;
            int count = Math.Min(5, segmenti.Count);
            for (int i = segmenti.Count - count; i < segmenti.Count; i++)
            {
                mediaPosizioni += segmenti[i].End;
            }
            mediaPosizioni /= count;

            float offsetDalCentro = posizione.X - origine.X;
            dir.X += Math.Sign(offsetDalCentro) * 0.1f;
        }

        return Vector2.Normalize(dir);
    }

    private void TentaRamificazione()
    {
        if ((float)rng.NextDouble() > probabilitaRamificazione) return;
        if (generazione >= 3) return;

        int indiceMin = Math.Max(2, segmenti.Count / 4);
        int indiceMax = segmenti.Count - 2;

        if (indiceMax <= indiceMin) return;

        int indice = rng.Next(indiceMin, indiceMax);
        var segmento = segmenti[indice];

        Vector2 dirSegmento = Vector2.Normalize(segmento.End - segmento.Start);
        Vector2 perpendicolare = new Vector2(-dirSegmento.Y, dirSegmento.X);

        if (rng.NextDouble() > 0.5)
            perpendicolare = -perpendicolare;

        Vector2 dirRamo = perpendicolare * 0.6f + new Vector2(0, 0.8f);
        dirRamo = Vector2.Normalize(dirRamo);

        var nuovaRadice = new Obj_Radice(segmento.End, dirRamo, generazione + 1, rng.Next());
        ramificazioni.Add(nuovaRadice);
    }

    private void UpdateBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        minX = float.MaxValue;
        maxX = float.MinValue;

        foreach (var seg in segmenti)
        {
            minY = Math.Min(minY, Math.Min(seg.Start.Y, seg.End.Y));
            maxY = Math.Max(maxY, Math.Max(seg.Start.Y, seg.End.Y));
            minX = Math.Min(minX, Math.Min(seg.Start.X, seg.End.X));
            maxX = Math.Max(maxX, Math.Max(seg.Start.X, seg.End.X));
        }

        foreach (var ramo in ramificazioni)
        {
            ramo.UpdateBounds();
            minY = Math.Min(minY, ramo.minY);
            maxY = Math.Max(maxY, ramo.maxY);
            minX = Math.Min(minX, ramo.minX);
            maxX = Math.Max(maxX, ramo.maxX);
        }
    }

    public bool IsInView(float cameraY)
    {
        return ViewCulling.IsRangeVisible(minY - 50, maxY + 50, cameraY);
    }

    public override void Draw()
    {
        foreach (var ramo in ramificazioni)
        {
            ramo.Draw();
        }

        if (segmenti.Count < 2) return;

        for (int i = 0; i < segmenti.Count - 1; i++)
        {
            var seg = segmenti[i];
            var nextSeg = segmenti[i + 1];

            float profRatio = (float)i / segmenti.Count;
            Color colore = LerpColor(ColorePrimario, ColoreSecondario, profRatio);

            if (seg.Thickness > 1.5f)
            {
                Color ombra = new Color(60, 40, 20, 180);
                Graphics.DrawLineEx(seg.Start, nextSeg.Start, seg.Thickness + 1f, ombra);
            }

            Graphics.DrawLineEx(seg.Start, nextSeg.Start, seg.Thickness, colore);
        }

        if (segmenti.Count > 1 && !completamenteCresta)
        {
            var punta = segmenti[^1];
            Graphics.DrawCircleV(punta.End, punta.Thickness * 0.7f, ColorePunta);
        }

        if (generazione == 0)
        {
            DrawPeliRadicali();
        }
    }

    private void DrawPeliRadicali()
    {
        for (int i = 2; i < segmenti.Count; i += 3)
        {
            var seg = segmenti[i];
            Vector2 dir = Vector2.Normalize(seg.End - seg.Start);
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            float lunghezzaPelo = 3f + (float)Math.Sin(i * 0.7f) * 2f;

            Vector2 pelo1End = seg.Start + perp * lunghezzaPelo;
            Vector2 pelo2End = seg.Start - perp * lunghezzaPelo;

            Color colorePelo = new Color(180, 140, 100, 150);
            Graphics.DrawLineEx(seg.Start, pelo1End, 0.5f, colorePelo);
            Graphics.DrawLineEx(seg.Start, pelo2End, 0.5f, colorePelo);
        }
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }

    public int GetTotalSegments()
    {
        int total = segmenti.Count;
        foreach (var ramo in ramificazioni)
        {
            total += ramo.GetTotalSegments();
        }
        return total;
    }

    public bool IsComplete => completamenteCresta && ramificazioni.TrueForAll(r => r.IsComplete);

    public RadiceSaveData ToSaveData()
    {
        var data = new RadiceSaveData();
        data.Generazione = generazione;

        foreach (var seg in segmenti)
        {
            data.Start.Add(seg.Start);
            data.End.Add(seg.End);
        }

        foreach (var ramo in ramificazioni)
            data.Rami.Add(ramo.ToSaveData());

        return data;
    }

    public static Obj_Radice FromSaveData(RadiceSaveData data)
    {
        return new Obj_Radice(data);
    }

    private Obj_Radice(RadiceSaveData data)
    {
        this.generazione = data.Generazione;
        this.rng = new Random();

        ConfiguraParametri();

        segmenti.Clear();
        for (int i = 0; i < data.Start.Count; i++)
        {
            float progressione = data.Start.Count > 1 ? (float)i / (data.Start.Count - 1) : 0;
            float spessore = spessoreBase * (1f - progressione * 0.7f);
            spessore = Math.Max(0.5f, spessore);

            var seg = new SegmentoRadice(data.Start[i], data.End[i], spessore, data.Generazione == 0);
            segmenti.Add(seg);
        }

        if (segmenti.Count > 0)
            this.origine = segmenti[0].Start;

        this.profonditaAttuale = profonditaMassima;
        this.completamenteCresta = true;

        foreach (var ramoData in data.Rami)
        {
            var ramo = Obj_Radice.FromSaveData(ramoData);
            ramificazioni.Add(ramo);
        }

        UpdateBounds();
    }
}
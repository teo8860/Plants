using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

/// <summary>
/// Minigioco: i semi cadono dal cielo, raccoglili con il cestino!
/// </summary>
public class MinigiocoSemi : MinigiocoBase
{
    public override string Nome => "Raccogli i Semi";
    public override string Descrizione => "Muovi il cestino per raccogliere i semi che cadono!";
    public override TipoMinigioco Tipo => TipoMinigioco.Semi;

    // Semi attivi
    private List<Semi> semi = new();
    
    // Cestino
    private Vector2 cestinoPos;
    private float cestinoLarghezza = 80f;
    private float cestinoAltezza = 25f;
    private float cestinoY;
    
    // Generazione semi
    private float tempoGenerazione = 0f;
    private float intervalloGenerazione = 1.2f;
    private float intervalloMinimo = 0.4f;
    
    // Difficoltà progressiva
    private float difficoltaTimer = 0f;
    private float velocitaBase = 120f;
    private float velocitaMax = 280f;
    
    // Animazioni
    private float animCestino = 0f;
    private float pulseTime = 0f;
    
    // Feedback raccolta
    private List<RaccoltaFeedback> feedbackRaccolta = new();

    private class Semi
    {
        public Vector2 posizione;
        public float velocita;
        public float dimensione;
        public Color colore;
        public bool attivo = true;
    }

    private class RaccoltaFeedback
    {
        public Vector2 posizione;
        public float timer;
        public int punti;
    }

    // Area di gioco
    private int marginX = 30;
    private int marginTop = 40;
    private int marginBottom = 40;

    public MinigiocoSemi() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 20f;
        punteggioMassimo = 15;
        
        semi.Clear();
        feedbackRaccolta.Clear();
        
        tempoGenerazione = 0f;
        intervalloGenerazione = 1.2f;
        difficoltaTimer = 0f;
        
        cestinoY = sh - 70;
        cestinoPos = new Vector2(sw / 2f, cestinoY);
        animCestino = 0f;
        
        // Genera primo seme subito
        GeneraSeme();
    }

    private void GeneraSeme()
    {
        var seme = new Semi();
        
        int areaW = sw - marginX * 2;
        seme.posizione = new Vector2(
            marginX + RandomHelper.Int(20, areaW - 20),
            -20
        );
        
        // Velocità progressiva
        float progress = 1f - (intervalloGenerazione - intervalloMinimo) / (1.2f - intervalloMinimo);
        progress = Math.Clamp(progress, 0f, 1f);
        seme.velocita = velocitaBase + (velocitaMax - velocitaBase) * progress;
        
        // Dimensione variabile
        seme.dimensione = RandomHelper.Float(12f, 18f);
        
        // Colore semi (toni di marrone/arancione)
        int colVar = RandomHelper.Int(0, 3);
        seme.colore = colVar switch
        {
            0 => new Color(210, 160, 60, 255),
            1 => new Color(180, 130, 50, 255),
            _ => new Color(160, 110, 40, 255)
        };
        
        semi.Add(seme);
    }

    protected override void UpdateGioco(float dt)
    {
        pulseTime += dt;
        animCestino = Math.Min(1f, animCestino + dt * 6f);
        
        // Aggiorna feedback
        for (int i = feedbackRaccolta.Count - 1; i >= 0; i--)
        {
            feedbackRaccolta[i].timer -= dt;
            if (feedbackRaccolta[i].timer <= 0)
                feedbackRaccolta.RemoveAt(i);
        }
        
        // Posizione cestino segue mouse (solo asse X)
        int mx = Input.GetMouseX();
        cestinoPos.X = Math.Clamp(mx, cestinoLarghezza / 2 + marginX, sw - cestinoLarghezza / 2 - marginX);
        
        // Generazione semi
        tempoGenerazione += dt;
        if (tempoGenerazione >= intervalloGenerazione)
        {
            tempoGenerazione = 0f;
            GeneraSeme();
            
            // Aumenta difficoltà
            intervalloGenerazione = Math.Max(intervalloMinimo, intervalloGenerazione - 0.03f);
        }
        
        // Aggiorna semi e verifica collisioni
        for (int i = semi.Count - 1; i >= 0; i--)
        {
            var seme = semi[i];
            if (!seme.attivo) continue;
            
            seme.posizione.Y += seme.velocita * dt;
            
            // Collisione con cestino
            if (seme.posizione.Y >= cestinoY - cestinoAltezza / 2 &&
                seme.posizione.Y <= cestinoY + cestinoAltezza / 2)
            {
                float distX = Math.Abs(seme.posizione.X - cestinoPos.X);
                if (distX <= cestinoLarghezza / 2 + seme.dimensione / 2)
                {
                    // Raccolto!
                    seme.attivo = false;
                    semi.RemoveAt(i);
                    
                    punteggio++;
                    feedbackRaccolta.Add(new RaccoltaFeedback
                    {
                        posizione = seme.posizione,
                        timer = 0.6f,
                        punti = 1
                    });
                    
                    // Vinci se raggiungi il massimo
                    if (punteggio >= punteggioMassimo)
                    {
                        Termina(true);
                        return;
                    }
                    continue;
                }
            }
            
            // Seme caduto fuori dallo schermo
            if (seme.posizione.Y > sh + 20)
            {
                semi.RemoveAt(i);
            }
        }
    }

    protected override void DrawGioco()
    {
        // Linee area di gioco
        Graphics.DrawRectangleLines(marginX - 2, marginTop - 2, 
            sw - marginX * 2 + 4, sh - marginTop - marginBottom + 4,
            new Color(60, 100, 60, 80));
        
        // Disegna semi
        foreach (var seme in semi)
        {
            if (!seme.attivo) continue;
            
            // Alone
            byte aloneA = (byte)(40 + MathF.Sin(pulseTime * 3f + seme.posizione.Y * 0.01f) * 20);
            Graphics.DrawCircleV(new Vector2(seme.posizione.X, seme.posizione.Y), seme.dimensione + 4, 
                new Color(seme.colore.R, seme.colore.G, seme.colore.B, aloneA));
            
            // Seme principale
            Graphics.DrawCircleV(seme.posizione, seme.dimensione, seme.colore);
            
            // Lucentezza
            Graphics.DrawCircleV(new Vector2(seme.posizione.X - seme.dimensione * 0.3f, seme.posizione.Y - seme.dimensione * 0.3f), 
                seme.dimensione * 0.25f, new Color(255, 230, 180, 150));
        }
        
        // Disegna cestino
        float scale = EaseOutBack(animCestino);
        float cW = cestinoLarghezza * scale;
        float cH = cestinoAltezza * scale;
        
        Vector2 cPos = new Vector2(cestinoPos.X, cestinoY);
        
        // Effetto "hover" cestino
        float hover = (MathF.Sin(pulseTime * 5f) + 1f) * 0.5f;
        
        // Cestino - parte principale
        Graphics.DrawRectangleRounded(
            new Rectangle(cPos.X - cW / 2, cPos.Y - cH / 2, cW, cH),
            0.2f, 4, new Color(139, 90, 43, 255));
        
        // Bordo cestino
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(cPos.X - cW / 2, cPos.Y - cH / 2, cW, cH),
            0.2f, 4, 2, new Color(200, 140, 60, 200));
        
        // Manico cestino
        float manicoH = 15f * scale;
        Graphics.DrawLine(
            (int)(cPos.X - cW / 3), (int)(cPos.Y - cH / 2),
            (int)(cPos.X - cW / 3), (int)(cPos.Y - cH / 2 - manicoH),
            new Color(160, 110, 50, 200));
        Graphics.DrawLine(
            (int)(cPos.X + cW / 3), (int)(cPos.Y - cH / 2),
            (int)(cPos.X + cW / 3), (int)(cPos.Y - cH / 2 - manicoH),
            new Color(160, 110, 50, 200));
        
        // Righe cestino
        for (int i = 1; i < 4; i++)
        {
            float xOff = -cW / 2 + (cW / 4) * i;
            Graphics.DrawLine(
                (int)(cPos.X + xOff), (int)(cPos.Y - cH / 2),
                (int)(cPos.X + xOff), (int)(cPos.Y + cH / 2),
                new Color(100, 60, 20, 100));
        }
        
        // Feedback raccolta
        foreach (var fb in feedbackRaccolta)
        {
            float alpha = fb.timer / 0.6f;
            float yOff = (1f - alpha) * 25f;
            byte textA = (byte)(255 * alpha);
            
            Graphics.DrawText("+1",
                (int)(fb.posizione.X - 8),
                (int)(fb.posizione.Y - 15 - yOff),
                14, new Color(255, 220, 100, textA));
        }
        
        // Hint all'inizio
        if (tempoTotale - tempoRimasto < 2f)
        {
            float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;
            byte hintA = (byte)(150 + pulse * 80);
            string hint = "Muovi il mouse per raccogliere!";
            int hintW = hint.Length * 5;
            Graphics.DrawText(hint, (sw - hintW) / 2, sh / 2, 10, new Color(200, 200, 150, hintA));
        }
        
        // Indicatore difficoltà
        float diff = 1f - (intervalloGenerazione - intervalloMinimo) / (1.2f - intervalloMinimo);
        int barW = 80;
        int barH = 6;
        int barX = sw - barW - 20;
        int barY = 15;
        
        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, barW, barH), 0.3f, 2, new Color(30, 30, 30, 180));
        
        Color diffCol = diff > 0.7f ? rosso : diff > 0.4f ? new Color(220, 180, 50, 255) : verdeChiaro;
        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, (int)(barW * diff), barH), 0.3f, 2, diffCol);
        
        string diffLabel = "Difficoltà";
        Graphics.DrawText(diffLabel, barX, barY + 10, 8, grigioChiaro);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

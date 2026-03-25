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
/// Minigioco: il treno passa alla stazione! Clicca sui passeggeri che saltano per farli salire a bordo.
/// </summary>
public class MinigiocoTreni : MinigiocoBase
{
    public override string Nome => "Treno Express";
    public override string Descrizione => "Clicca sui passeggeri che saltano per farli salire a bordo!";
    public override TipoMinigioco Tipo => TipoMinigioco.Treni;

    // Treno
    private Vector2 trenoPos;
    private float trenoVelocita = 0f;
    private bool trenoInStazione = false;
    private bool trenoPartito = false;
    private float trenoLarghezza = 200f;
    private float trenoAltezza = 70f;
    
    // Binario e stazione
    private int binarioY;
    private int piattaformaY;
    private int marginX = 30;
    
    // Passeggeri
    private List<Passeggero> passeggeri = new();
    private float tempoGenerazione = 0f;
    private float intervalloGenerazione = 1.5f;
    private float intervalloMinimo = 0.5f;
    
    // Animazioni
    private float pulseTime = 0f;
    private float animTreno = 0f;
    
    // Feedback click
    private List<FeedbackClick> feedbackClick = new();
    
    // Difficoltà
    private float difficoltaTimer = 0f;
    private int passeggeriPersi = 0;
    private int maxPersi = 3;
    
    private class Passeggero
    {
        public Vector2 posizione;
        public Vector2 velocita;
        public float dimensione = 20f;
        public Color colore;
        public bool attivo = true;
        public bool salito = false;
        public float animSalto = 0f;
    }
    
    private class FeedbackClick
    {
        public Vector2 posizione;
        public float timer;
        public bool positivo;
    }

    public MinigiocoTreni() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 20f;
        punteggioMassimo = 12;
        passeggeriPersi = 0;
        
        // Setup posizioni
        binarioY = sh - 120;
        piattaformaY = sh - 50;
        
        // Treno parte da sinistra
        trenoPos = new Vector2(-trenoLarghezza - 50, binarioY);
        trenoVelocita = 250f;
        trenoInStazione = false;
        trenoPartito = false;
        animTreno = 0f;
        
       
        passeggeri.Clear();
        // Reset liste feedbackClick.Clear();
        
        // Tempo generazione
        tempoGenerazione = 0f;
        intervalloGenerazione = 1.5f;
        difficoltaTimer = 0f;
        
        // Genera primo passeggero
        GeneraPasseggero();
    }
    
    private void GeneraPasseggero()
    {
        var p = new Passeggero();
        
        // Posizione sulla piattaforma (lato destro)
        int areaX = sw - marginX * 2 - 100;
        p.posizione = new Vector2(
            sw - marginX - 30,
            piattaformaY - 15
        );
        
        // Velocità - salta verso il treno (sinistra)
        float jumpX = RandomHelper.Float(-180f, -120f);
        float jumpY = RandomHelper.Float(-280f, -220f);
        p.velocita = new Vector2(jumpX, jumpY);
        
        // Colore passeggero (vari)
        int colVar = RandomHelper.Int(0, 4);
        p.colore = colVar switch
        {
            0 => new Color(70, 130, 180, 255),   // Blu
            1 => new Color(139, 90, 43, 255),    // Marrone
            2 => new Color(100, 149, 237, 255), // Blu cielo
            _ => new Color(205, 92, 92, 255)     // Rosso
        };
        
        passeggeri.Add(p);
    }

    protected override void UpdateGioco(float dt)
    {
        pulseTime += dt;
        
        // Aggiorna treni
        if (!trenoInStazione && !trenoPartito)
        {
            // Treno sta arrivando
            trenoPos.X += trenoVelocita * dt;
            animTreno = Math.Min(1f, animTreno + dt * 2f);
            
            if (trenoPos.X >= 80f)
            {
                trenoInStazione = true;
                trenoPos.X = 80f;
            }
        }
        else if (trenoInStazione && !trenoPartito)
        {
            // Treno in stazione - aspetta un po' e poi parte
            animTreno = 1f;
            
            if (passeggeriPersi >= maxPersi)
            {
                Termina(false);
                return;
            }
            
            if (punteggio >= punteggioMassimo)
            {
                Termina(true);
                return;
            }
        }
        
        // Generazione passeggeri
        if (trenoInStazione && !trenoPartito)
        {
            tempoGenerazione += dt;
            if (tempoGenerazione >= intervalloGenerazione)
            {
                tempoGenerazione = 0f;
                if (passeggeri.Count < 5)
                {
                    GeneraPasseggero();
                }
                
                // Aumenta difficoltà
                intervalloGenerazione = Math.Max(intervalloMinimo, intervalloGenerazione - 0.05f);
            }
        }
        
        // Aggiorna passeggeri
        for (int i = passeggeri.Count - 1; i >= 0; i--)
        {
            var p = passeggeri[i];
            if (!p.attivo) continue;
            
            // Fisica salto
            p.velocita.Y += 600f * dt; // Gravità
            p.posizione.X += p.velocita.X * dt;
            p.posizione.Y += p.velocita.Y * dt;
            
            // Animazione
            p.animSalto = Math.Min(1f, p.animSalto + dt * 3f);
            
            // Collisione con treno (se sta saltando verso il treno)
            if (!p.salito && p.posizione.X < 80f + trenoLarghezza - 20f && p.posizione.Y < binarioY + 10f)
            {
                // Salito sul treno!
                p.salito = true;
                p.attivo = false;
                punteggio++;
                
                feedbackClick.Add(new FeedbackClick
                {
                    posizione = p.posizione,
                    timer = 0.6f,
                    positivo = true
                });
                
                if (punteggio >= punteggioMassimo)
                {
                    Termina(true);
                    return;
                }
            }
            
            // Passeggero caduto fuori dallo schermo o a terra
            if (p.posizione.Y > piattaformaY + 10f || p.posizione.X < -30)
            {
                if (!p.salito)
                {
                    passeggeriPersi++;
                    feedbackClick.Add(new FeedbackClick
                    {
                        posizione = new Vector2(p.posizione.X, piattaformaY),
                        timer = 0.6f,
                        positivo = false
                    });
                    
                    if (passeggeriPersi >= maxPersi)
                    {
                        Termina(false);
                        return;
                    }
                }
                passeggeri.RemoveAt(i);
            }
        }
        
        // Aggiorna feedback click
        for (int i = feedbackClick.Count - 1; i >= 0; i--)
        {
            feedbackClick[i].timer -= dt;
            if (feedbackClick[i].timer <= 0)
                feedbackClick.RemoveAt(i);
        }
        
        // Click sui passeggeri
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();
            
            foreach (var p in passeggeri)
            {
                if (!p.attivo) continue;
                
                float dist = Vector2.Distance(new Vector2(mx, my), p.posizione);
                if (dist <= p.dimensione + 15f)
                {
                    // Boost al passeggero!
                    p.velocita.X = RandomHelper.Float(-200f, -150f);
                    p.velocita.Y = RandomHelper.Float(-300f, -250f);
                    
                    feedbackClick.Add(new FeedbackClick
                    {
                        posizione = p.posizione,
                        timer = 0.4f,
                        positivo = true
                    });
                    break;
                }
            }
        }
    }

    protected override void DrawGioco()
    {
        // Sfondo - binario
        Graphics.DrawRectangle(0, binarioY, sw, sh - binarioY, new Color(50, 40, 30, 255));
        
        // Binario
        Graphics.DrawRectangle(0, binarioY + 10, sw, 8, new Color(80, 70, 60, 255));
        
        // Rotaie
        for (int i = 0; i < sw; i += 30)
        {
            Graphics.DrawRectangle(i, binarioY + 6, 12, 4, new Color(120, 110, 100, 255));
        }
        
        // Piattaforma
        Graphics.DrawRectangle(marginX, piattaformaY, sw - marginX * 2, sh - piattaformaY, new Color(100, 90, 80, 255));
        Graphics.DrawRectangleLines(marginX, piattaformaY, sw - marginX * 2, sh - piattaformaY, new Color(130, 120, 110, 255));
        
        // Segnali piattaforma
        Graphics.DrawText("STAZIONE", marginX + 10, piattaformaY + 15, 12, new Color(200, 180, 150, 200));
        
        // Disegna treno
        float scale = EaseOutBack(animTreno);
        float tX = trenoPos.X;
        float tY = trenoPos.Y - trenoAltezza;
        float tW = trenoLarghezza * scale;
        float tH = trenoAltezza * scale;
        
        // Corpo treno
        Color trenoCol = new Color(180, 50, 50, 255);
        Graphics.DrawRectangleRounded(new Rectangle(tX, tY, tW, tH), 0.05f, 4, trenoCol);
        
        // Finestrini
        int numFinestre = 5;
        float finestraW = 25f;
        float finestraH = 20f;
        float finestraSpazio = tW / (numFinestre + 1);
        for (int i = 0; i < numFinestre; i++)
        {
            float fx = tX + finestraSpazio * (i + 1) - finestraW / 2;
            float fy = tY + 10;
            Graphics.DrawRectangle((int)fx, (int)fy, (int)finestraW, (int)finestraH, new Color(150, 200, 255, 200));
        }
        
        // Ruote
        Graphics.DrawCircleV(new Vector2(tX + 30, tY + tH), 12, new Color(40, 40, 40, 255));
        Graphics.DrawCircleV(new Vector2(tX + tW - 30, tY + tH), 12, new Color(40, 40, 40, 255));
        
        // Frontale treno (lato sinistro)
        if (tW > 50)
        {
            Graphics.DrawRectangle((int)tX, (int)(tY + 10), 15, (int)(tH - 20), new Color(200, 180, 50, 255)); // Fari
        }
        
        // Disegna passeggeri
        foreach (var p in passeggeri)
        {
            if (!p.attivo) continue;
            
            float pScale = EaseOutBack(p.animSalto);
            
            // Ombra
            Graphics.DrawCircleV(new Vector2(p.posizione.X, p.posizione.Y + p.dimensione * 0.8f), 
                p.dimensione * 0.6f, new Color(0, 0, 0, 50));
            
            // Corpo
            Graphics.DrawCircleV(p.posizione, p.dimensione * pScale, p.colore);
            
            // Bordo
            Graphics.DrawCircleLinesV(p.posizione, p.dimensione * pScale, new Color(255, 255, 255, 150));
            
            // "Braccia" che indicano direzione
            if (p.velocita.X < 0)
            {
                Graphics.DrawLine((int)p.posizione.X, (int)p.posizione.Y, 
                    (int)(p.posizione.X + 15), (int)(p.posizione.Y - 10), new Color(255, 255, 255, 150));
            }
        }
        
        // Feedback click
        foreach (var fb in feedbackClick)
        {
            float alpha = fb.timer / 0.6f;
            float yOff = (1f - alpha) * 20f;
            byte textA = (byte)(255 * alpha);
            
            string text = fb.positivo ? "OK!" : "Mancato!";
            Color col = fb.positivo ? new Color(100, 255, 100, textA) : new Color(255, 100, 100, textA);
            
            int textW = text.Length * 6;
            Graphics.DrawText(text, (int)(fb.posizione.X - textW / 2), (int)(fb.posizione.Y - 25 - yOff), 12, col);
        }
        
        // Indicatori passeggeri persi
        int heartX = sw - 30;
        int heartY = sh - 25;
        for (int i = 0; i < maxPersi; i++)
        {
            bool perso = i < passeggeriPersi;
            Color heartCol = perso ? new Color(220, 80, 80, 255) : new Color(100, 100, 100, 100);
            int hx = heartX - i * 20;
            Graphics.DrawCircleV(new Vector2(hx, heartY), 6, heartCol);
        }
        
        // Hint all'inizio
        if (tempoTotale - tempoRimasto < 2f)
        {
            float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;
            byte hintA = (byte)(150 + pulse * 80);
            string hint = "Clicca i passeggeri per aiutarli a saltare sul treno!";
            int hintW = hint.Length * 5;
            Graphics.DrawText(hint, (sw - hintW) / 2, sh / 2, 10, new Color(200, 200, 150, hintA));
        }
        
        // Status treno
        if (!trenoInStazione)
        {
            float pulse = (MathF.Sin(pulseTime * 3f) + 1f) * 0.5f;
            byte textA = (byte)(150 + pulse * 80);
            string status = "Il treno sta arrivando...";
            int statusW = status.Length * 6;
            Graphics.DrawText(status, (sw - statusW) / 2, sh / 3, 12, new Color(200, 200, 150, textA));
        }
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

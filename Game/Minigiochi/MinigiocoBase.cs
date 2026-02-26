using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

public enum TipoMinigioco
{
    Cerchio,
    Tieni,
    Resta,
    Semi
}

public enum MinigiocoStato
{
    Intro,
    InCorso,
    Vittoria,
    Sconfitta
}


public abstract class MinigiocoBase : GameElement
{
    protected MinigiocoStato stato = MinigiocoStato.Intro;
    protected float tempoTotale = 0f;
    protected float tempoRimasto = 0f;
    protected int punteggio = 0;
    protected int punteggioMassimo = 0;

    // Intro/Outro
    private float introTimer = 0f;
    private const float INTRO_DURATA = 2f;
    private float outroTimer = 0f;
    private const float OUTRO_DURATA = 3f;

    // Animazione pannello
    private float animProgress = 0f;

    // Colori condivisi
    protected readonly Color sfondoOverlay = new Color(0, 0, 0, 180);
    protected readonly Color panelBg = new Color(25, 30, 20, 245);
    protected readonly Color panelBorder = new Color(80, 160, 80, 255);
    protected readonly Color verdeChiaro = new Color(100, 220, 100, 255);
    protected readonly Color rosso = new Color(220, 80, 80, 255);
    protected readonly Color bianco = new Color(240, 240, 240, 255);
    protected readonly Color grigioChiaro = new Color(180, 180, 180, 255);

    // Screen dimensions
    protected int sw => Rendering.camera.screenWidth;
    protected int sh => Rendering.camera.screenHeight;

    public abstract string Nome { get; }
    public abstract string Descrizione { get; }
    public abstract TipoMinigioco Tipo { get; }

    public MinigiocoBase()
    {
        this.guiLayer = true;
        this.depth = -1500;
        this.persistent = false;
        this.active = false;
    }

    /// <summary>
    /// Forza lo stop e disattiva il minigioco, resettando lo stato.
    /// </summary>
    public void Ferma()
    {
        stato = MinigiocoStato.Intro;
        introTimer = 0f;
        outroTimer = 0f;
        animProgress = 0f;
        punteggio = 0;
        tempoRimasto = 0f;
        this.active = false;
    }

    public void Avvia()
    {
        Ferma();
        OnAvvia();
        this.active = true;
    }

    // Da far ereditare a ciascun minigioco
    protected abstract void OnAvvia();
    protected abstract void UpdateGioco(float dt);
    protected abstract void DrawGioco();

    public override void Update()
    {
        float dt = Time.GetFrameTime();

        switch (stato)
        {
            case MinigiocoStato.Intro:
                introTimer += dt;
                animProgress = Math.Min(1f, introTimer / 0.3f);
                if (introTimer >= INTRO_DURATA)
                {
                    stato = MinigiocoStato.InCorso;
                    tempoRimasto = tempoTotale;
                }
                break;

            case MinigiocoStato.InCorso:
                animProgress = 1f;
                tempoRimasto -= dt;
                if (tempoRimasto <= 0f)
                {
                    tempoRimasto = 0f;
                    Termina(false);
                }
                else
                {
                    UpdateGioco(dt);
                }
                break;

            case MinigiocoStato.Vittoria:
            case MinigiocoStato.Sconfitta:
                outroTimer += dt;
                if (outroTimer >= OUTRO_DURATA ||
                    Input.IsMouseButtonPressed(MouseButton.Left) ||
                    Input.IsKeyPressed(KeyboardKey.Enter))
                {
                    if (outroTimer > 0.5f)
                        Chiudi();
                }
                break;
        }
    }

    public override void Draw()
    {
        // Overlay scuro
        byte alpha = (byte)(180 * Math.Min(1f, animProgress));
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, alpha));

        switch (stato)
        {
            case MinigiocoStato.Intro:
                DrawIntro();
                break;
            case MinigiocoStato.InCorso:
                DrawGioco();
                DrawHUD();
                break;
            case MinigiocoStato.Vittoria:
            case MinigiocoStato.Sconfitta:
                DrawRisultato();
                break;
        }
    }

    private void DrawIntro()
    {
        float eased = EaseOutBack(animProgress);
        int pw = (int)(280 * eased);
        int ph = (int)(120 * eased);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.1f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.1f, 8, 2, panelBorder);

        if (pw > 100)
        {
            string nome = Nome;
            int nomeW = nome.Length * 8;
            Graphics.DrawText(nome, px + (pw - nomeW) / 2, py + 20, 16, verdeChiaro);

            string desc = Descrizione;
            int descW = desc.Length * 5;
            Graphics.DrawText(desc, px + (pw - descW) / 2, py + 50, 10, grigioChiaro);

            string tempo = $"Tempo: {tempoTotale:0}s";
            int tempoW = tempo.Length * 5;
            Graphics.DrawText(tempo, px + (pw - tempoW) / 2, py + 75, 10, bianco);

            float countdown = INTRO_DURATA - introTimer;
            if (countdown > 0 && countdown < 1.5f)
            {
                string countText = countdown > 1f ? "2" : countdown > 0.5f ? "1" : "Via!";
                int countW = countText.Length * 12;
                Graphics.DrawText(countText, px + (pw - countW) / 2, py + 92, 20, verdeChiaro);
            }
        }
    }

    private void DrawHUD()
    {
        // Barra tempo in alto
        int barW = sw - 40;
        int barH = 8;
        int barX = 20;
        int barY = 15;

        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, barW, barH), 0.5f, 4, new Color(30, 30, 30, 200));

        float pct = tempoRimasto / tempoTotale;
        Color barColor = pct > 0.3f ? verdeChiaro : pct > 0.1f ? new Color(220, 180, 50, 255) : rosso;
        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, (int)(barW * pct), barH), 0.5f, 4, barColor);

        // Tempo testo
        string tempoStr = $"{tempoRimasto:0.0}s";
        Graphics.DrawText(tempoStr, barX + barW - 40, barY + 12, 10, bianco);

        // Punteggio
        string punteggioStr = $"{punteggio}/{punteggioMassimo}";
        Graphics.DrawText(punteggioStr, barX, barY + 12, 10, verdeChiaro);
    }

    private void DrawRisultato()
    {
        bool vinto = stato == MinigiocoStato.Vittoria;

        int pw = 280;
        int ph = 180;
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        Color borderCol = vinto ? verdeChiaro : rosso;
        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.1f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.1f, 8, 2, borderCol);

        // Titolo
        string titolo = vinto ? "Vittoria!" : "Tempo Scaduto!";
        Color titoloCol = vinto ? verdeChiaro : rosso;
        int titoloW = titolo.Length * 9;
        Graphics.DrawText(titolo, px + (pw - titoloW) / 2, py + 20, 18, titoloCol);

        // Punteggio
        string puntStr = $"Punteggio: {punteggio}/{punteggioMassimo}";
        int puntW = puntStr.Length * 6;
        Graphics.DrawText(puntStr, px + (pw - puntW) / 2, py + 55, 12, bianco);

        // Foglie guadagnate
        int foglie = CalcolaFoglie();
        string foglieStr = $"+{foglie} Foglie";
        int foglieW = foglieStr.Length * 7;
        Color foglieCol = foglie > 0 ? verdeChiaro : grigioChiaro;
        Graphics.DrawText(foglieStr, px + (pw - foglieW) / 2, py + 85, 14, foglieCol);

        // Messaggio
        string msg = vinto ? "Complimenti!" : "Ritenta!";
        int msgW = msg.Length * 6;
        Graphics.DrawText(msg, px + (pw - msgW) / 2, py + 120, 12, grigioChiaro);

        // Hint chiudi
        float pulse = (MathF.Sin(outroTimer * 3f) + 1f) * 0.5f;
        byte hintA = (byte)(120 + pulse * 80);
        string hint = "Clicca per continuare";
        int hintW = hint.Length * 5;
        Graphics.DrawText(hint, px + (pw - hintW) / 2, py + 150, 10, new Color(150, 150, 150, hintA));
    }

    protected void Termina(bool vinto)
    {
        stato = vinto ? MinigiocoStato.Vittoria : MinigiocoStato.Sconfitta;
        outroTimer = 0f;
    }

    private void Chiudi()
    {
        int foglie = CalcolaFoglie();
        AssegnaFoglie(foglie);
        this.active = false;
        Game.room_main.SetActiveRoom();
        ManagerMinigames.OnMinigiocoFinito();
    }

    protected int CalcolaFoglie()
    {
        if (punteggioMassimo <= 0) return 0;
        float ratio = (float)punteggio / punteggioMassimo;
        int base_foglie = (int)(ratio * 50);
        if (stato == MinigiocoStato.Vittoria)
            base_foglie += 20;
        return base_foglie;
    }

    private void AssegnaFoglie(int quantita)
    {
        if (quantita > 0)
            Game.pianta.Stats.FoglieAccumulate += quantita;
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

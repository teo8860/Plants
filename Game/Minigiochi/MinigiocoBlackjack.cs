using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class MinigiocoBlackjack : MinigiocoBase
{
    public override string Nome => "Luigi-Jack";
    public override string Descrizione => "Avvicinati a 21 senza sballare! Batti Luigi!";
    public override TipoMinigioco Tipo => TipoMinigioco.Blackjack;

    // --- Sistema a round con monete (come Luigi-Jack) ---
    private const int MONETE_INIZIALI = 30;
    private const int ROUND_TOTALI = 5;
    private const int BUST_PENALTY = 5;

    private int moneteGiocatore;
    private int moneteDealer;
    private int roundCorrente;

    // Carte
    private List<int> mazzo = new();
    private List<Card> manoGiocatore = new();
    private List<Card> manoDealer = new();

    // Fasi
    private enum FaseBJ
    {
        RoundIntro,         // mostra numero round
        Distribuzione,      // carte distribuite una alla volta
        TurnoGiocatore,     // Hit / Stand
        TurnoDealer,        // dealer pesca automaticamente
        Confronto,          // mostra chi vince il round
        FinePartita          // mostra vincitore finale
    }
    private FaseBJ fase;

    // Timers animazione
    private float distribTimer;
    private int distribCount;
    private const float DISTRIB_DELAY = 0.35f;

    private float dealerTimer;
    private bool dealerPescando;
    private const float DEALER_DELAY = 0.55f;

    private float roundIntroTimer;
    private const float ROUND_INTRO_DURATA = 1.4f;

    private float confrontoTimer;
    private const float CONFRONTO_DURATA = 2.5f;

    private float finePartitaTimer;

    // Bottoni
    private Rectangle btnHit;
    private Rectangle btnStand;
    private bool hoverHit;
    private bool hoverStand;

    // Animazione
    private float animTimer;

    // Risultato round
    private string risultatoRoundTesto = "";
    private string risultatoMoneteTesto = "";
    private bool roundVinto;

    // Colori
    private readonly Color cardBg = new Color(250, 245, 235, 255);
    private readonly Color cardBorder = new Color(60, 60, 70, 255);
    private readonly Color cardRed = new Color(200, 40, 40, 255);
    private readonly Color cardBlack = new Color(30, 30, 40, 255);
    private readonly Color cardBack = new Color(35, 100, 55, 255);
    private readonly Color cardBackPattern = new Color(50, 130, 65, 255);
    private readonly Color cardBackStar = new Color(255, 220, 50, 80);
    private readonly Color feltGreen = new Color(25, 75, 40, 230);
    private readonly Color feltDark = new Color(18, 55, 30, 240);
    private readonly Color feltBorder = new Color(190, 160, 70, 255);
    private readonly Color goldText = new Color(255, 220, 80, 255);
    private readonly Color coinGold = new Color(255, 200, 50, 255);
    private readonly Color coinShadow = new Color(180, 140, 30, 255);
    private readonly Color luigiGreen = new Color(60, 180, 80, 255);
    private readonly Color btnGreen = new Color(50, 140, 60, 255);
    private readonly Color btnGreenHover = new Color(70, 170, 80, 255);
    private readonly Color btnOrange = new Color(200, 120, 30, 255);
    private readonly Color btnOrangeHover = new Color(230, 145, 45, 255);

    private struct Card
    {
        public int Valore;      // 1-13 (1=Asso, 11=J, 12=Q, 13=K)
        public int Seme;        // 0-3
        public float AnimT;
        public bool Coperta;
    }

    public MinigiocoBlackjack() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 999f; // nessun limite di tempo reale, guidato dai round
        punteggioMassimo = 100;

        moneteGiocatore = MONETE_INIZIALI;
        moneteDealer = MONETE_INIZIALI;
        roundCorrente = 0;

        CreaMazzo();
        IniziaRound();
    }

    private void IniziaRound()
    {
        roundCorrente++;
        manoGiocatore.Clear();
        manoDealer.Clear();

        fase = FaseBJ.RoundIntro;
        roundIntroTimer = 0f;
        distribTimer = 0f;
        distribCount = 0;
        dealerTimer = 0f;
        dealerPescando = false;
        animTimer = 0f;
        risultatoRoundTesto = "";
        risultatoMoneteTesto = "";
        hoverHit = false;
        hoverStand = false;

        int btnW = 90;
        int btnH = 30;
        int btnY = sh - 58;
        btnHit = new Rectangle(sw / 2 - btnW - 8, btnY, btnW, btnH);
        btnStand = new Rectangle(sw / 2 + 8, btnY, btnW, btnH);
    }

    private void CreaMazzo()
    {
        mazzo.Clear();
        for (int deck = 0; deck < 4; deck++)
            for (int i = 0; i < 52; i++)
                mazzo.Add(i);

        var rng = new Random();
        for (int i = mazzo.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (mazzo[i], mazzo[j]) = (mazzo[j], mazzo[i]);
        }
    }

    private Card PescaCarta(bool coperta = false)
    {
        if (mazzo.Count < 10) CreaMazzo();
        int id = mazzo[^1];
        mazzo.RemoveAt(mazzo.Count - 1);
        return new Card
        {
            Valore = (id % 13) + 1,
            Seme = id / 13 % 4,
            AnimT = 0f,
            Coperta = coperta
        };
    }

    private int CalcolaPunteggio(List<Card> mano)
    {
        int tot = 0;
        int assi = 0;
        foreach (var c in mano)
        {
            if (c.Coperta) continue;
            int v = c.Valore;
            if (v == 1) { tot += 11; assi++; }
            else if (v >= 10) tot += 10;
            else tot += v;
        }
        while (tot > 21 && assi > 0) { tot -= 10; assi--; }
        return tot;
    }

    private int CalcolaPunteggioCompleto(List<Card> mano)
    {
        int tot = 0;
        int assi = 0;
        foreach (var c in mano)
        {
            int v = c.Valore;
            if (v == 1) { tot += 11; assi++; }
            else if (v >= 10) tot += 10;
            else tot += v;
        }
        while (tot > 21 && assi > 0) { tot -= 10; assi--; }
        return tot;
    }

    private bool HaSballato(List<Card> mano) => CalcolaPunteggioCompleto(mano) > 21;

    // Luigi-Jack: 21 con 2 carte = speciale (batte un 21 normale)
    private bool HasLuigiJack(List<Card> mano) => mano.Count == 2 && CalcolaPunteggioCompleto(mano) == 21;

    // Moltiplicatore vincita in stile Luigi-Jack
    private int GetMultiplier(List<Card> mano)
    {
        int pts = CalcolaPunteggioCompleto(mano);
        if (pts != 21) return 1;
        if (mano.Count == 2) return 3;   // Luigi-Jack! Triple
        if (mano.Count == 3) return 2;   // 21 con 3 carte = double
        return 1;
    }

    protected override void UpdateGioco(float dt)
    {
        animTimer += dt;

        // Anima carte
        AnimateCards(manoGiocatore, dt);
        AnimateCards(manoDealer, dt);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        switch (fase)
        {
            case FaseBJ.RoundIntro:
                roundIntroTimer += dt;
                if (roundIntroTimer >= ROUND_INTRO_DURATA)
                    fase = FaseBJ.Distribuzione;
                break;

            case FaseBJ.Distribuzione:
                distribTimer += dt;
                if (distribTimer >= DISTRIB_DELAY && distribCount < 4)
                {
                    distribTimer = 0f;
                    if (distribCount == 0) manoGiocatore.Add(PescaCarta());
                    else if (distribCount == 1) manoDealer.Add(PescaCarta());
                    else if (distribCount == 2) manoGiocatore.Add(PescaCarta());
                    else if (distribCount == 3) manoDealer.Add(PescaCarta(coperta: true));
                    distribCount++;
                }
                if (distribCount >= 4 && distribTimer >= DISTRIB_DELAY)
                {
                    // Check Luigi-Jack naturale
                    if (HasLuigiJack(manoGiocatore))
                    {
                        ScopriDealer();
                        if (HasLuigiJack(manoDealer))
                            RisolviRound(); // entrambi LJ
                        else
                            RisolviRound(); // giocatore ha LJ
                    }
                    else
                    {
                        fase = FaseBJ.TurnoGiocatore;
                    }
                }
                break;

            case FaseBJ.TurnoGiocatore:
                hoverHit = PointInRect(mx, my, btnHit);
                hoverStand = PointInRect(mx, my, btnStand);

                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (hoverHit)
                    {
                        manoGiocatore.Add(PescaCarta());
                        if (HaSballato(manoGiocatore))
                        {
                            ScopriDealer();
                            RisolviRound();
                        }
                        else if (CalcolaPunteggio(manoGiocatore) == 21)
                        {
                            // Auto-stand a 21
                            ScopriDealer();
                            fase = FaseBJ.TurnoDealer;
                            dealerPescando = true;
                            dealerTimer = 0f;
                        }
                    }
                    else if (hoverStand)
                    {
                        ScopriDealer();
                        fase = FaseBJ.TurnoDealer;
                        dealerPescando = true;
                        dealerTimer = 0f;
                    }
                }
                break;

            case FaseBJ.TurnoDealer:
                if (dealerPescando)
                {
                    dealerTimer += dt;
                    if (dealerTimer >= DEALER_DELAY)
                    {
                        dealerTimer = 0f;
                        int dealerPts = CalcolaPunteggioCompleto(manoDealer);
                        if (dealerPts < 17)
                            manoDealer.Add(PescaCarta());
                        else
                        {
                            dealerPescando = false;
                            RisolviRound();
                        }
                    }
                }
                break;

            case FaseBJ.Confronto:
                confrontoTimer += dt;
                if (confrontoTimer >= CONFRONTO_DURATA)
                {
                    // Controlla fine partita
                    if (moneteGiocatore <= 0 || moneteDealer <= 0 || roundCorrente >= ROUND_TOTALI)
                    {
                        fase = FaseBJ.FinePartita;
                        finePartitaTimer = 0f;
                    }
                    else
                    {
                        IniziaRound();
                    }
                }
                break;

            case FaseBJ.FinePartita:
                finePartitaTimer += dt;
                if (finePartitaTimer >= 3f)
                {
                    // Punteggio finale per il sistema base
                    bool vinto = moneteGiocatore > moneteDealer;
                    punteggio = vinto ? Math.Min(100, 40 + moneteGiocatore) : Math.Max(0, moneteGiocatore);
                    Termina(vinto);
                }
                break;
        }
    }

    private void AnimateCards(List<Card> mano, float dt)
    {
        for (int i = 0; i < mano.Count; i++)
        {
            var c = mano[i];
            if (c.AnimT < 1f) { c.AnimT = Math.Min(1f, c.AnimT + dt * 5f); mano[i] = c; }
        }
    }

    private void ScopriDealer()
    {
        for (int i = 0; i < manoDealer.Count; i++)
        {
            var c = manoDealer[i];
            c.Coperta = false;
            manoDealer[i] = c;
        }
    }

    private void RisolviRound()
    {
        int ptsG = CalcolaPunteggioCompleto(manoGiocatore);
        int ptsD = CalcolaPunteggioCompleto(manoDealer);
        bool bustG = ptsG > 21;
        bool bustD = ptsD > 21;
        bool ljG = HasLuigiJack(manoGiocatore);
        bool ljD = HasLuigiJack(manoDealer);

        int trasferimento = 0;

        if (bustG && bustD)
        {
            // Entrambi sballano: nessun trasferimento
            risultatoRoundTesto = "Entrambi sballati!";
            risultatoMoneteTesto = "Nessun cambio";
            roundVinto = false;
        }
        else if (bustG)
        {
            // Giocatore sballa: paga penalita'
            trasferimento = BUST_PENALTY;
            moneteGiocatore -= trasferimento;
            moneteDealer += trasferimento;
            risultatoRoundTesto = "Sballato!";
            risultatoMoneteTesto = $"-{trasferimento} monete";
            roundVinto = false;
        }
        else if (bustD)
        {
            // Dealer sballa: giocatore incassa penalita'
            trasferimento = BUST_PENALTY;
            moneteGiocatore += trasferimento;
            moneteDealer -= trasferimento;
            risultatoRoundTesto = "Luigi sballa!";
            risultatoMoneteTesto = $"+{trasferimento} monete";
            roundVinto = true;
        }
        else if (ljG && !ljD)
        {
            // Luigi-Jack batte tutto
            int diff = Math.Max(1, 21 - ptsD);
            trasferimento = diff * 3;
            moneteGiocatore += trasferimento;
            moneteDealer -= trasferimento;
            risultatoRoundTesto = "LUIGI-JACK! x3!";
            risultatoMoneteTesto = $"+{trasferimento} monete!";
            roundVinto = true;
        }
        else if (ljD && !ljG)
        {
            int diff = Math.Max(1, 21 - ptsG);
            trasferimento = diff * 3;
            moneteGiocatore -= trasferimento;
            moneteDealer += trasferimento;
            risultatoRoundTesto = "Luigi ha Luigi-Jack! x3!";
            risultatoMoneteTesto = $"-{trasferimento} monete";
            roundVinto = false;
        }
        else if (ptsG == ptsD)
        {
            // Pareggio: se entrambi 21, chi ha meno carte vince
            if (ptsG == 21 && manoGiocatore.Count != manoDealer.Count)
            {
                bool gVince = manoGiocatore.Count < manoDealer.Count;
                int multW = GetMultiplier(gVince ? manoGiocatore : manoDealer);
                int multL = GetMultiplier(gVince ? manoDealer : manoGiocatore);
                trasferimento = Math.Max(1, multW - multL) * 3;
                if (gVince)
                {
                    moneteGiocatore += trasferimento;
                    moneteDealer -= trasferimento;
                    risultatoRoundTesto = $"21 in {manoGiocatore.Count} carte! x{multW}";
                    risultatoMoneteTesto = $"+{trasferimento} monete";
                }
                else
                {
                    moneteGiocatore -= trasferimento;
                    moneteDealer += trasferimento;
                    risultatoRoundTesto = $"Luigi: 21 in {manoDealer.Count} carte! x{multL}";
                    risultatoMoneteTesto = $"-{trasferimento} monete";
                }
                roundVinto = gVince;
            }
            else
            {
                risultatoRoundTesto = $"Pareggio! {ptsG}";
                risultatoMoneteTesto = "Nessun cambio";
                roundVinto = false;
            }
        }
        else
        {
            // Chi ha di piu' vince: trasferimento = differenza * moltiplicatore
            bool gVince = ptsG > ptsD;
            int winner = gVince ? ptsG : ptsD;
            int loser = gVince ? ptsD : ptsG;
            int diff = winner - loser;
            int mult = GetMultiplier(gVince ? manoGiocatore : manoDealer);
            trasferimento = diff * mult;

            if (gVince)
            {
                moneteGiocatore += trasferimento;
                moneteDealer -= trasferimento;
                risultatoRoundTesto = $"Vinci! {ptsG} vs {ptsD}";
                risultatoMoneteTesto = mult > 1 ? $"+{trasferimento} monete (x{mult}!)" : $"+{trasferimento} monete";
                roundVinto = true;
            }
            else
            {
                moneteGiocatore -= trasferimento;
                moneteDealer += trasferimento;
                risultatoRoundTesto = $"Luigi vince! {ptsD} vs {ptsG}";
                risultatoMoneteTesto = mult > 1 ? $"-{trasferimento} monete (x{mult})" : $"-{trasferimento} monete";
                roundVinto = false;
            }
        }

        fase = FaseBJ.Confronto;
        confrontoTimer = 0f;
    }

    private bool PointInRect(int px, int py, Rectangle r)
    {
        return px >= r.X && px <= r.X + r.Width && py >= r.Y && py <= r.Y + r.Height;
    }

    // ========== DRAW ==========

    protected override void DrawGioco()
    {
        // Tavolo
        int tm = 12;
        var tableRect = new Rectangle(tm, 28, sw - tm * 2, sh - 40);
        Graphics.DrawRectangleRounded(tableRect, 0.06f, 8, feltGreen);
        Graphics.DrawRectangleRoundedLines(tableRect, 0.06f, 8, 2, feltBorder);
        var innerRect = new Rectangle(tm + 6, 34, sw - tm * 2 - 12, sh - 52);
        Graphics.DrawRectangleRoundedLines(innerRect, 0.05f, 8, 1, new Color(180, 150, 80, 40));

        // HUD: monete e round
        DrawHUD();

        // Label Luigi (dealer)
        Graphics.DrawText("LUIGI", sw / 2 - 18, 44, 10, luigiGreen);
        int dealerPts = CalcolaPunteggio(manoDealer);
        if (fase != FaseBJ.Distribuzione && fase != FaseBJ.RoundIntro)
        {
            bool hasCoperta = manoDealer.Exists(c => c.Coperta);
            string dealerStr = hasCoperta ? "?" : dealerPts.ToString();
            Graphics.DrawText(dealerStr, sw / 2 + 20, 44, 10, bianco);
        }

        // Carte dealer
        DrawMano(manoDealer, sw / 2, 62);

        // Separatore
        int sepY = sh / 2 - 15;
        Graphics.DrawRectangle(tm + 16, sepY, sw - tm * 2 - 32, 1, new Color(180, 150, 80, 40));

        // Label giocatore
        int playerPts = CalcolaPunteggio(manoGiocatore);
        Graphics.DrawText("TU", sw / 2 - 8, sepY + 8, 10, goldText);
        if (manoGiocatore.Count > 0)
            Graphics.DrawText(playerPts.ToString(), sw / 2 + 14, sepY + 8, 10, bianco);

        // Carte giocatore
        DrawMano(manoGiocatore, sw / 2, sepY + 24);

        // Bottoni
        if (fase == FaseBJ.TurnoGiocatore)
        {
            DrawButton(btnHit, "CARTA", hoverHit, btnGreen, btnGreenHover);
            DrawButton(btnStand, "STAI", hoverStand, btnOrange, btnOrangeHover);
        }

        // Round intro overlay
        if (fase == FaseBJ.RoundIntro)
            DrawRoundIntro();

        // Risultato round overlay
        if (fase == FaseBJ.Confronto)
            DrawConfronto();

        // Fine partita overlay
        if (fase == FaseBJ.FinePartita)
            DrawFinePartita();
    }

    private void DrawHUD()
    {
        // Monete giocatore (sinistra)
        DrawCoin(20, 32);
        Graphics.DrawText($"{moneteGiocatore}", 32, 33, 10, goldText);

        // Monete dealer (destra)
        DrawCoin(sw - 55, 32);
        Graphics.DrawText($"{moneteDealer}", sw - 43, 33, 10, luigiGreen);

        // Round (centro in alto)
        string roundStr = $"Round {roundCorrente}/{ROUND_TOTALI}";
        int rw = roundStr.Length * 5;
        Graphics.DrawText(roundStr, sw / 2 - rw / 2, 32, 8, grigioChiaro);
    }

    private void DrawCoin(int x, int y)
    {
        Graphics.DrawCircleV(new Vector2(x + 5, y + 5), 5, coinShadow);
        Graphics.DrawCircleV(new Vector2(x + 5, y + 4), 5, coinGold);
        Graphics.DrawCircleV(new Vector2(x + 5, y + 4), 3, new Color(255, 230, 100, 255));
    }

    private void DrawRoundIntro()
    {
        float t = roundIntroTimer / ROUND_INTRO_DURATA;
        float alpha = t < 0.2f ? t / 0.2f : t > 0.8f ? (1f - t) / 0.2f : 1f;
        float scale = 0.6f + 0.4f * EaseOutBack(Math.Min(1f, t * 2f));

        int pw = (int)(200 * scale);
        int ph = (int)(70 * scale);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.15f, 6,
            new Color(20, 60, 35, (byte)(230 * alpha)));
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.15f, 6, 2,
            new Color(255, 220, 80, (byte)(255 * alpha)));

        string title = $"Round {roundCorrente}";
        int tw = title.Length * 9;
        Graphics.DrawText(title, px + (pw - tw) / 2, py + 12, 18,
            new Color(255, 220, 80, (byte)(255 * alpha)));

        string sub = "Avvicinati a 21!";
        int sw2 = sub.Length * 5;
        Graphics.DrawText(sub, px + (pw - sw2) / 2, py + 38, 10,
            new Color(200, 200, 210, (byte)(200 * alpha)));
    }

    private void DrawConfronto()
    {
        float t = confrontoTimer / CONFRONTO_DURATA;
        float alpha = t < 0.15f ? t / 0.15f : t > 0.85f ? (1f - t) / 0.15f : 1f;

        int pw = 280;
        int ph = 65;
        int px = (sw - pw) / 2;
        int py = sh / 2 - ph / 2;

        Color bgCol = roundVinto
            ? new Color(30, 90, 45, (byte)(230 * alpha))
            : new Color(90, 35, 35, (byte)(230 * alpha));
        Color borderCol = roundVinto ? verdeChiaro : rosso;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.12f, 6, bgCol);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.12f, 6, 2,
            new Color(borderCol.R, borderCol.G, borderCol.B, (byte)(255 * alpha)));

        // Testo risultato
        int tw1 = risultatoRoundTesto.Length * 6;
        Graphics.DrawText(risultatoRoundTesto, px + (pw - tw1) / 2, py + 12, 12,
            new Color(255, 255, 255, (byte)(255 * alpha)));

        // Testo monete
        Color monCol = roundVinto
            ? new Color(100, 255, 120, (byte)(255 * alpha))
            : new Color(255, 120, 100, (byte)(255 * alpha));
        int tw2 = risultatoMoneteTesto.Length * 6;
        Graphics.DrawText(risultatoMoneteTesto, px + (pw - tw2) / 2, py + 35, 12, monCol);
    }

    private void DrawFinePartita()
    {
        float t = Math.Min(1f, finePartitaTimer / 0.5f);
        bool vinto = moneteGiocatore > moneteDealer;
        bool pareggio = moneteGiocatore == moneteDealer;

        int pw = 300;
        int ph = 100;
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        Color bgCol = vinto ? new Color(25, 80, 40, (byte)(240 * t)) : new Color(80, 30, 30, (byte)(240 * t));
        Color borderCol = vinto ? goldText : rosso;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.1f, 6, bgCol);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.1f, 6, 2,
            new Color(borderCol.R, borderCol.G, borderCol.B, (byte)(255 * t)));

        string title = pareggio ? "Pareggio!" : vinto ? "Hai Vinto!" : "Luigi Vince!";
        int tw = title.Length * 9;
        Graphics.DrawText(title, px + (pw - tw) / 2, py + 14, 18,
            new Color(255, 255, 255, (byte)(255 * t)));

        string score = $"Tu: {moneteGiocatore}  -  Luigi: {moneteDealer}";
        int sw2 = score.Length * 5;
        Graphics.DrawText(score, px + (pw - sw2) / 2, py + 42, 10,
            new Color(200, 200, 210, (byte)(220 * t)));

        int foglie = vinto ? 40 + moneteGiocatore : Math.Max(0, moneteGiocatore / 2);
        string foglieStr = $"+{foglie} Foglie";
        int fw = foglieStr.Length * 6;
        Color foglieCol = foglie > 0
            ? new Color(100, 255, 120, (byte)(255 * t))
            : new Color(160, 160, 160, (byte)(200 * t));
        Graphics.DrawText(foglieStr, px + (pw - fw) / 2, py + 65, 12, foglieCol);
    }

    private void DrawMano(List<Card> mano, int centerX, int startY)
    {
        int cardW = 38;
        int cardH = 52;
        int spacing = 5;
        int totalW = mano.Count > 0 ? mano.Count * (cardW + spacing) - spacing : 0;
        int sx = centerX - totalW / 2;

        for (int i = 0; i < mano.Count; i++)
        {
            var card = mano[i];
            float anim = EaseOutBack(card.AnimT);
            int cx = sx + i * (cardW + spacing);
            int drawW = (int)(cardW * anim);
            int drawH = (int)(cardH * anim);
            int drawX = cx + (cardW - drawW) / 2;
            int drawY = startY + (cardH - drawH) / 2;

            if (drawW < 4 || drawH < 4) continue;

            if (card.Coperta)
                DrawCardBack(drawX, drawY, drawW, drawH);
            else
                DrawCardFace(card, drawX, drawY, drawW, drawH);
        }
    }

    private void DrawCardBack(int x, int y, int w, int h)
    {
        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), 0.15f, 4, cardBack);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), 0.15f, 4, 1, new Color(25, 60, 30, 255));

        int m = 3;
        Graphics.DrawRectangleRounded(new Rectangle(x + m, y + m, w - m * 2, h - m * 2), 0.1f, 4, cardBackPattern);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x + m, y + m, w - m * 2, h - m * 2), 0.1f, 4, 1, new Color(25, 60, 30, 180));

        // Stella stile Mario
        int cx = x + w / 2;
        int cy = y + h / 2;
        int s = Math.Min(w, h) / 5;
        for (int i = 0; i < 5; i++)
        {
            float angle = -MathF.PI / 2 + i * MathF.PI * 2 / 5;
            float angle2 = angle + MathF.PI / 5;
            Vector2 outer = new Vector2(cx + MathF.Cos(angle) * s, cy + MathF.Sin(angle) * s);
            Vector2 inner = new Vector2(cx + MathF.Cos(angle2) * s * 0.4f, cy + MathF.Sin(angle2) * s * 0.4f);
            Graphics.DrawLineEx(new Vector2(cx, cy), outer, 1.5f, cardBackStar);
            Graphics.DrawLineEx(new Vector2(cx, cy), inner, 1f, cardBackStar);
        }
    }

    private void DrawCardFace(Card card, int x, int y, int w, int h)
    {
        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), 0.15f, 4, cardBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), 0.15f, 4, 1, cardBorder);

        bool isRed = card.Seme == 0 || card.Seme == 1;
        Color textCol = isRed ? cardRed : cardBlack;

        string valStr = card.Valore switch
        {
            1 => "A", 11 => "J", 12 => "Q", 13 => "K",
            _ => card.Valore.ToString()
        };

        string semeChar = card.Seme switch
        {
            0 => "H", 1 => "D", 2 => "C", 3 => "S", _ => "?"
        };

        if (w >= 18)
        {
            Graphics.DrawText(valStr, x + 3, y + 2, 10, textCol);
            Graphics.DrawText(semeChar, x + 3, y + 12, 7, textCol);
        }

        int cx = x + w / 2;
        int cy = y + h / 2 + 2;
        DrawSemeSymbol(card.Seme, cx, cy, Math.Min(w, h) / 4);
    }

    private void DrawSemeSymbol(int seme, int cx, int cy, int size)
    {
        bool isRed = seme == 0 || seme == 1;
        Color col = isRed ? cardRed : cardBlack;

        switch (seme)
        {
            case 0: // Cuori
                Graphics.DrawCircleV(new Vector2(cx - size / 3, cy - size / 4), size / 2.5f, col);
                Graphics.DrawCircleV(new Vector2(cx + size / 3, cy - size / 4), size / 2.5f, col);
                Graphics.DrawTriangle(
                    new Vector2(cx - size * 0.6f, cy - size / 6),
                    new Vector2(cx, cy + size * 0.6f),
                    new Vector2(cx + size * 0.6f, cy - size / 6), col);
                break;
            case 1: // Quadri
                Graphics.DrawTriangle(
                    new Vector2(cx, cy - size), new Vector2(cx - size * 0.6f, cy), new Vector2(cx + size * 0.6f, cy), col);
                Graphics.DrawTriangle(
                    new Vector2(cx - size * 0.6f, cy), new Vector2(cx, cy + size), new Vector2(cx + size * 0.6f, cy), col);
                break;
            case 2: // Fiori
                Graphics.DrawCircleV(new Vector2(cx, cy - size / 2.5f), size / 2.5f, col);
                Graphics.DrawCircleV(new Vector2(cx - size / 3, cy + size / 6), size / 2.5f, col);
                Graphics.DrawCircleV(new Vector2(cx + size / 3, cy + size / 6), size / 2.5f, col);
                Graphics.DrawRectangle(cx - 1, cy, 3, size / 2, col);
                break;
            case 3: // Picche
                Graphics.DrawTriangle(
                    new Vector2(cx, cy - size * 0.7f),
                    new Vector2(cx - size * 0.6f, cy + size * 0.2f),
                    new Vector2(cx + size * 0.6f, cy + size * 0.2f), col);
                Graphics.DrawCircleV(new Vector2(cx - size / 3, cy + size / 5), size / 3f, col);
                Graphics.DrawCircleV(new Vector2(cx + size / 3, cy + size / 5), size / 3f, col);
                Graphics.DrawRectangle(cx - 1, cy + size / 4, 3, size / 2, col);
                break;
        }
    }

    private void DrawButton(Rectangle rect, string text, bool hover, Color normal, Color hoverCol)
    {
        Color bg = hover ? hoverCol : normal;
        Graphics.DrawRectangleRounded(rect, 0.3f, 6, bg);
        Graphics.DrawRectangleRoundedLines(rect, 0.3f, 6, 1, new Color(255, 255, 255, hover ? (byte)80 : (byte)40));

        int tw = text.Length * 7;
        int tx = (int)rect.X + ((int)rect.Width - tw) / 2;
        int ty = (int)rect.Y + ((int)rect.Height - 14) / 2;
        Graphics.DrawText(text, tx, ty, 14, bianco);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

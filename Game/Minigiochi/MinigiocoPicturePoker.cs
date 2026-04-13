using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Plants;

public class MinigiocoPicturePoker : MinigiocoBase
{
    public override string Nome => "Picture Poker";
    public override string Descrizione => "Batti Luigi a poker! Scarta e pesca!";
    public override TipoMinigioco Tipo => TipoMinigioco.PicturePoker;

    // --- Tipi carta (6 tipi, dal peggiore al migliore) ---
    private enum PicType { Cloud, Mushroom, Flower, Luigi, Mario, Star }

    private static readonly Color[] picColors = {
        new Color(180, 180, 200, 255),  // Cloud - grigio
        new Color(220, 80, 80, 255),    // Mushroom - rosso
        new Color(255, 160, 40, 255),   // Flower - arancio
        new Color(80, 180, 80, 255),    // Luigi - verde
        new Color(220, 50, 50, 255),    // Mario - rosso
        new Color(255, 220, 50, 255),   // Star - giallo
    };

    private static readonly string[] picNames = { "Nuvola", "Fungo", "Fiore", "Luigi", "Mario", "Stella" };
    private static readonly string[] picSymbols = { "~", "M", "*", "L", "m", "S" };

    // Mano
    private struct PokerCard
    {
        public PicType Tipo;
        public float AnimT;
        public bool Coperta;
        public bool Selezionata; // per scarto
    }

    // --- Stato ---
    private enum FasePoker
    {
        Distribuzione,
        Selezione,      // giocatore seleziona carte da scartare
        Scarto,         // animazione scarto/pesca
        DealerScarto,   // Luigi scarta
        Reveal,         // mostra mani
        Risultato
    }
    private FasePoker fase;

    private List<PokerCard> manoGiocatore = new();
    private List<PokerCard> manoDealer = new();
    private List<PicType> mazzo = new();

    // Monete
    private int moneteGiocatore;
    private int scommessa;
    private const int SCOMMESSA_BASE = 1;
    private const int MONETE_INIZIALI = 20;

    // Round
    private int roundCorrente;
    private const int ROUND_TOTALI = 5;

    // Timers
    private float distribTimer;
    private int distribCount;
    private const float DISTRIB_DELAY = 0.2f;

    private float scartoTimer;
    private int scartoCount;
    private const float SCARTO_DELAY = 0.25f;

    private float dealerScartoTimer;
    private int dealerScartoCount;
    private List<int> dealerScarti = new();

    private float revealTimer;
    private const float REVEAL_DURATA = 1.5f;

    private float risultatoTimer;
    private const float RISULTATO_DURATA = 2.5f;

    private float roundIntroTimer;
    private bool showingRoundIntro;

    // Risultato
    private string risultatoTesto = "";
    private string manoTesto = "";
    private string vincitaTesto = "";
    private bool roundVinto;

    // Hover per selezione carte
    private int hoverCardIdx = -1;

    // Bottoni conferma
    private Rectangle btnConferma;
    private Rectangle btnRaddoppia;
    private bool hoverConferma;
    private bool hoverRaddoppia;

    // Colori
    private readonly Color feltPurple = new Color(50, 25, 70, 230);
    private readonly Color feltBorder = new Color(180, 140, 60, 255);
    private readonly Color cardBg = new Color(250, 248, 240, 255);
    private readonly Color cardBorder = new Color(70, 60, 80, 255);
    private readonly Color cardSelected = new Color(255, 255, 100, 120);
    private readonly Color cardBackCol = new Color(60, 30, 90, 255);
    private readonly Color cardBackPattern = new Color(80, 50, 110, 255);
    private readonly Color goldText = new Color(255, 220, 80, 255);
    private readonly Color coinGold = new Color(255, 200, 50, 255);
    private readonly Color coinShadow = new Color(180, 140, 30, 255);
    private readonly Color luigiGreen = new Color(60, 180, 80, 255);
    private readonly Color btnPurple = new Color(100, 60, 140, 255);
    private readonly Color btnPurpleHover = new Color(130, 80, 170, 255);
    private readonly Color btnGold = new Color(180, 150, 50, 255);
    private readonly Color btnGoldHover = new Color(210, 180, 70, 255);

    public MinigiocoPicturePoker() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 999f;
        punteggioMassimo = 100;

        moneteGiocatore = MONETE_INIZIALI;
        roundCorrente = 0;

        IniziaRound();
    }

    private void IniziaRound()
    {
        roundCorrente++;
        scommessa = SCOMMESSA_BASE;

        manoGiocatore.Clear();
        manoDealer.Clear();
        dealerScarti.Clear();

        CreaMazzo();

        fase = FasePoker.Distribuzione;
        distribTimer = 0f;
        distribCount = 0;
        scartoTimer = 0f;
        scartoCount = 0;
        dealerScartoTimer = 0f;
        dealerScartoCount = 0;
        revealTimer = 0f;
        risultatoTimer = 0f;
        risultatoTesto = "";
        manoTesto = "";
        vincitaTesto = "";
        hoverCardIdx = -1;
        hoverConferma = false;
        hoverRaddoppia = false;
        showingRoundIntro = true;
        roundIntroTimer = 0f;

        int btnW = 85;
        int btnH = 26;
        int btnY = sh - 52;
        btnConferma = new Rectangle(sw / 2 - btnW - 6, btnY, btnW, btnH);
        btnRaddoppia = new Rectangle(sw / 2 + 6, btnY, btnW, btnH);
    }

    private void CreaMazzo()
    {
        mazzo.Clear();
        // 30 carte: 5 per tipo
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
                mazzo.Add((PicType)i);

        var rng = new Random();
        for (int i = mazzo.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (mazzo[i], mazzo[j]) = (mazzo[j], mazzo[i]);
        }
    }

    private PokerCard PescaCarta(bool coperta = false)
    {
        if (mazzo.Count == 0) CreaMazzo();
        var tipo = mazzo[^1];
        mazzo.RemoveAt(mazzo.Count - 1);
        return new PokerCard { Tipo = tipo, AnimT = 0f, Coperta = coperta, Selezionata = false };
    }

    // --- Valutazione mano ---
    private enum HandRank { Niente, Coppia, DoppiaCoppia, Tris, FullHouse, Poker, Flush }

    private static readonly string[] handNames = {
        "Niente", "Coppia", "Doppia Coppia", "Tris", "Full House", "Poker", "5 Uguali!"
    };

    private static readonly int[] handMultipliers = { 0, 2, 3, 4, 6, 8, 16 };

    private (HandRank rank, PicType highCard) ValutaMano(List<PokerCard> mano)
    {
        var counts = new Dictionary<PicType, int>();
        foreach (var c in mano)
        {
            if (!counts.ContainsKey(c.Tipo)) counts[c.Tipo] = 0;
            counts[c.Tipo]++;
        }

        var sorted = counts.OrderByDescending(kv => kv.Value).ThenByDescending(kv => (int)kv.Key).ToList();
        int max = sorted[0].Value;
        PicType high = sorted[0].Key;

        if (max == 5) return (HandRank.Flush, high);
        if (max == 4) return (HandRank.Poker, high);
        if (max == 3 && sorted.Count >= 2 && sorted[1].Value == 2) return (HandRank.FullHouse, high);
        if (max == 3) return (HandRank.Tris, high);
        if (max == 2 && sorted.Count >= 2 && sorted[1].Value == 2) return (HandRank.DoppiaCoppia, high);
        if (max == 2) return (HandRank.Coppia, high);
        return (HandRank.Niente, high);
    }

    // --- AI dealer: decide quali carte scartare ---
    private List<int> DealerDecidiScarti()
    {
        var scarti = new List<int>();
        var counts = new Dictionary<PicType, int>();
        foreach (var c in manoDealer)
        {
            if (!counts.ContainsKey(c.Tipo)) counts[c.Tipo] = 0;
            counts[c.Tipo]++;
        }

        // Tieni le carte che formano coppie/tris, scarta i singleton
        var keep = new HashSet<PicType>();
        foreach (var kv in counts)
            if (kv.Value >= 2) keep.Add(kv.Key);

        // Se non ha niente, tieni le 2 carte piu' alte
        if (keep.Count == 0)
        {
            var sorted = manoDealer.Select((c, i) => (c, i)).OrderByDescending(x => (int)x.c.Tipo).ToList();
            for (int i = 2; i < sorted.Count; i++)
                scarti.Add(sorted[i].i);
            return scarti;
        }

        for (int i = 0; i < manoDealer.Count; i++)
            if (!keep.Contains(manoDealer[i].Tipo))
                scarti.Add(i);

        return scarti;
    }

    protected override void UpdateGioco(float dt)
    {
        // Anima carte
        AnimCards(manoGiocatore, dt);
        AnimCards(manoDealer, dt);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Round intro
        if (showingRoundIntro)
        {
            roundIntroTimer += dt;
            if (roundIntroTimer >= 1.3f) showingRoundIntro = false;
            return;
        }

        switch (fase)
        {
            case FasePoker.Distribuzione:
                distribTimer += dt;
                if (distribTimer >= DISTRIB_DELAY && distribCount < 10)
                {
                    distribTimer = 0f;
                    if (distribCount % 2 == 0)
                        manoGiocatore.Add(PescaCarta());
                    else
                        manoDealer.Add(PescaCarta(coperta: true));
                    distribCount++;
                }
                if (distribCount >= 10 && distribTimer >= DISTRIB_DELAY)
                    fase = FasePoker.Selezione;
                break;

            case FasePoker.Selezione:
                // Hover carte giocatore per selezionare
                UpdateCardHover(mx, my);

                if (Input.IsMouseButtonPressed(MouseButton.Left) && hoverCardIdx >= 0)
                {
                    var c = manoGiocatore[hoverCardIdx];
                    c.Selezionata = !c.Selezionata;
                    manoGiocatore[hoverCardIdx] = c;
                }

                hoverConferma = PointInRect(mx, my, btnConferma);
                hoverRaddoppia = PointInRect(mx, my, btnRaddoppia);

                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (hoverRaddoppia && moneteGiocatore >= scommessa * 2)
                    {
                        scommessa *= 2;
                    }
                    else if (hoverConferma)
                    {
                        // Scarta le carte selezionate
                        fase = FasePoker.Scarto;
                        scartoTimer = 0f;
                        scartoCount = 0;
                    }
                }
                break;

            case FasePoker.Scarto:
                scartoTimer += dt;
                if (scartoTimer >= SCARTO_DELAY)
                {
                    scartoTimer = 0f;
                    // Trova prossima carta da scartare
                    bool found = false;
                    for (int i = scartoCount; i < manoGiocatore.Count; i++)
                    {
                        if (manoGiocatore[i].Selezionata)
                        {
                            manoGiocatore[i] = PescaCarta();
                            scartoCount = i + 1;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // Dealer scarta
                        dealerScarti = DealerDecidiScarti();
                        dealerScartoCount = 0;
                        dealerScartoTimer = 0f;
                        fase = FasePoker.DealerScarto;
                    }
                }
                break;

            case FasePoker.DealerScarto:
                dealerScartoTimer += dt;
                if (dealerScartoTimer >= SCARTO_DELAY)
                {
                    dealerScartoTimer = 0f;
                    if (dealerScartoCount < dealerScarti.Count)
                    {
                        int idx = dealerScarti[dealerScartoCount];
                        manoDealer[idx] = PescaCarta();
                        dealerScartoCount++;
                    }
                    else
                    {
                        // Scopri carte dealer
                        for (int i = 0; i < manoDealer.Count; i++)
                        {
                            var c = manoDealer[i];
                            c.Coperta = false;
                            manoDealer[i] = c;
                        }
                        fase = FasePoker.Reveal;
                        revealTimer = 0f;
                    }
                }
                break;

            case FasePoker.Reveal:
                revealTimer += dt;
                if (revealTimer >= REVEAL_DURATA)
                {
                    RisolviRound();
                    fase = FasePoker.Risultato;
                    risultatoTimer = 0f;
                }
                break;

            case FasePoker.Risultato:
                risultatoTimer += dt;
                if (risultatoTimer >= RISULTATO_DURATA)
                {
                    if (moneteGiocatore <= 0 || roundCorrente >= ROUND_TOTALI)
                    {
                        bool vinto = moneteGiocatore > MONETE_INIZIALI;
                        punteggio = vinto ? Math.Min(100, 30 + moneteGiocatore * 2) : Math.Max(0, moneteGiocatore * 3);
                        Termina(vinto);
                    }
                    else
                    {
                        IniziaRound();
                    }
                }
                break;
        }
    }

    private void AnimCards(List<PokerCard> mano, float dt)
    {
        for (int i = 0; i < mano.Count; i++)
        {
            var c = mano[i];
            if (c.AnimT < 1f) { c.AnimT = Math.Min(1f, c.AnimT + dt * 6f); mano[i] = c; }
        }
    }

    private void UpdateCardHover(int mx, int my)
    {
        hoverCardIdx = -1;
        int cardW = 52;
        int cardH = 72;
        int spacing = 8;
        int totalW = 5 * (cardW + spacing) - spacing;
        int sx = sw / 2 - totalW / 2;
        int cy = sh / 2 + 22;

        for (int i = 0; i < manoGiocatore.Count; i++)
        {
            int cx = sx + i * (cardW + spacing);
            if (mx >= cx && mx <= cx + cardW && my >= cy && my <= cy + cardH)
            {
                hoverCardIdx = i;
                break;
            }
        }
    }

    private void RisolviRound()
    {
        var (rankG, highG) = ValutaMano(manoGiocatore);
        var (rankD, highD) = ValutaMano(manoDealer);

        manoTesto = $"Tu: {handNames[(int)rankG]}  -  Luigi: {handNames[(int)rankD]}";

        bool gVince;
        if (rankG != rankD)
            gVince = rankG > rankD;
        else
            gVince = highG > highD; // stessa mano: vince carta piu' alta

        bool pareggio = rankG == rankD && highG == highD;

        if (pareggio)
        {
            risultatoTesto = "Pareggio!";
            vincitaTesto = "Monete restituite";
            roundVinto = false;
        }
        else if (gVince)
        {
            int mult = handMultipliers[(int)rankG];
            int vinto = Math.Max(1, scommessa * mult);
            moneteGiocatore += vinto;
            risultatoTesto = "Vinci!";
            vincitaTesto = mult > 0 ? $"+{vinto} monete (x{mult})" : $"+{scommessa} monete";
            roundVinto = true;
        }
        else
        {
            moneteGiocatore -= scommessa;
            risultatoTesto = "Luigi vince!";
            vincitaTesto = $"-{scommessa} monete";
            roundVinto = false;
        }
    }

    private bool PointInRect(int px, int py, Rectangle r)
    {
        return px >= r.X && px <= r.X + r.Width && py >= r.Y && py <= r.Y + r.Height;
    }

    // ========== DRAW ==========

    protected override void DrawGioco()
    {
        // Tavolo viola stile casino Luigi
        int tm = 12;
        var tableRect = new Rectangle(tm, 28, sw - tm * 2, sh - 40);
        Graphics.DrawRectangleRounded(tableRect, 0.06f, 8, feltPurple);
        Graphics.DrawRectangleRoundedLines(tableRect, 0.06f, 8, 2, feltBorder);
        var innerRect = new Rectangle(tm + 5, 33, sw - tm * 2 - 10, sh - 50);
        Graphics.DrawRectangleRoundedLines(innerRect, 0.05f, 8, 1, new Color(180, 140, 60, 30));

        // HUD
        DrawCoin(18, 32);
        Graphics.DrawText($"{moneteGiocatore}", 30, 33, 10, goldText);
        string betStr = $"Puntata: {scommessa}";
        Graphics.DrawText(betStr, sw - 10 - betStr.Length * 5, 33, 8, goldText);
        string roundStr = $"Round {roundCorrente}/{ROUND_TOTALI}";
        int rw = roundStr.Length * 4;
        Graphics.DrawText(roundStr, sw / 2 - rw / 2, 32, 8, grigioChiaro);

        // Label dealer
        Graphics.DrawText("LUIGI", sw / 2 - 18, 46, 10, luigiGreen);

        // Carte dealer
        DrawHand(manoDealer, sw / 2, 62, false);

        // Separatore
        int sepY = sh / 2 - 5;
        Graphics.DrawText("VS", sw / 2 - 6, sepY - 4, 10, new Color(200, 180, 100, 120));

        // Carte giocatore
        DrawHand(manoGiocatore, sw / 2, sh / 2 + 22, true);

        // Fase selezione: indicazioni + bottoni
        if (fase == FasePoker.Selezione)
        {
            int selCount = manoGiocatore.Count(c => c.Selezionata);
            string hint = selCount > 0 ? $"Scarti: {selCount} carte" : "Clicca le carte da scartare";
            int hw = hint.Length * 5;
            Graphics.DrawText(hint, sw / 2 - hw / 2, sh / 2 + 10, 8, grigioChiaro);

            DrawButton(btnConferma, "GIOCA", hoverConferma, btnPurple, btnPurpleHover);
            if (moneteGiocatore >= scommessa * 2)
                DrawButton(btnRaddoppia, $"x2 ({scommessa * 2})", hoverRaddoppia, btnGold, btnGoldHover);
        }

        // Round intro
        if (showingRoundIntro)
            DrawRoundIntro();

        // Reveal: mostra nomi mani
        if (fase == FasePoker.Reveal || fase == FasePoker.Risultato)
        {
            int tw = manoTesto.Length * 5;
            Graphics.DrawText(manoTesto, sw / 2 - tw / 2, sepY + 5, 8, bianco);
        }

        // Risultato
        if (fase == FasePoker.Risultato)
            DrawRisultato();
    }

    private void DrawHand(List<PokerCard> mano, int centerX, int startY, bool isPlayer)
    {
        int cardW = 52;
        int cardH = 72;
        int spacing = 8;
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

            // Selezione offset
            if (isPlayer && card.Selezionata)
                drawY -= 8;

            if (drawW < 4 || drawH < 4) continue;

            if (card.Coperta)
            {
                DrawPicCardBack(drawX, drawY, drawW, drawH);
            }
            else
            {
                DrawPicCardFace(card, drawX, drawY, drawW, drawH);
            }

            // Hover highlight
            if (isPlayer && fase == FasePoker.Selezione && i == hoverCardIdx && !card.Selezionata)
            {
                Graphics.DrawRectangleRoundedLines(new Rectangle(drawX - 1, drawY - 1, drawW + 2, drawH + 2),
                    0.12f, 4, 2, goldText);
            }

            // Selection glow
            if (isPlayer && card.Selezionata)
            {
                Graphics.DrawRectangleRounded(new Rectangle(drawX, drawY, drawW, drawH), 0.12f, 4, cardSelected);
            }
        }
    }

    private void DrawPicCardBack(int x, int y, int w, int h)
    {
        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), 0.12f, 4, cardBackCol);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), 0.12f, 4, 1, new Color(40, 20, 60, 255));

        int m = 3;
        Graphics.DrawRectangleRounded(new Rectangle(x + m, y + m, w - m * 2, h - m * 2), 0.08f, 4, cardBackPattern);

        // Question mark
        int cx = x + w / 2;
        int cy = y + h / 2;
        Graphics.DrawText("?", cx - 5, cy - 7, 14, new Color(255, 255, 255, 60));
    }

    private void DrawPicCardFace(PokerCard card, int x, int y, int w, int h)
    {
        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), 0.12f, 4, cardBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), 0.12f, 4, 1, cardBorder);

        Color col = picColors[(int)card.Tipo];
        string name = picNames[(int)card.Tipo];

        // Nome in alto
        if (w >= 30)
            Graphics.DrawText(name, x + 3, y + 3, 7, col);

        // Simbolo grande al centro
        int cx = x + w / 2;
        int cy = y + h / 2 + 2;
        DrawPicSymbol(card.Tipo, cx, cy, Math.Min(w, h) / 3);
    }

    private void DrawPicSymbol(PicType tipo, int cx, int cy, int size)
    {
        Color col = picColors[(int)tipo];

        switch (tipo)
        {
            case PicType.Cloud:
                Graphics.DrawCircleV(new Vector2(cx - size / 3, cy), size / 2.5f, col);
                Graphics.DrawCircleV(new Vector2(cx + size / 3, cy), size / 2.5f, col);
                Graphics.DrawCircleV(new Vector2(cx, cy - size / 4), size / 2.2f, col);
                break;

            case PicType.Mushroom:
                // Cappello
                Graphics.DrawCircleV(new Vector2(cx, cy - size / 4), size / 1.8f, col);
                // Puntini
                Graphics.DrawCircleV(new Vector2(cx - size / 4, cy - size / 3), size / 6, cardBg);
                Graphics.DrawCircleV(new Vector2(cx + size / 4, cy - size / 3), size / 6, cardBg);
                // Gambo
                Graphics.DrawRectangle(cx - size / 4, cy, size / 2, size / 2, new Color(240, 230, 200, 255));
                break;

            case PicType.Flower:
                // Petali
                for (int i = 0; i < 5; i++)
                {
                    float angle = i * MathF.PI * 2 / 5 - MathF.PI / 2;
                    float px = cx + MathF.Cos(angle) * size / 3;
                    float py = cy + MathF.Sin(angle) * size / 3;
                    Graphics.DrawCircleV(new Vector2(px, py), size / 3.5f, col);
                }
                // Centro
                Graphics.DrawCircleV(new Vector2(cx, cy), size / 4, new Color(80, 60, 40, 255));
                break;

            case PicType.Luigi:
                // L verde
                Graphics.DrawRectangle(cx - size / 3, cy - size / 2, size / 4, size, col);
                Graphics.DrawRectangle(cx - size / 3, cy + size / 3, size * 2 / 3, size / 5, col);
                break;

            case PicType.Mario:
                // M rossa
                int mw = size * 2 / 3;
                Graphics.DrawRectangle(cx - mw / 2, cy - size / 2, size / 5, size, col);
                Graphics.DrawRectangle(cx + mw / 2 - size / 5, cy - size / 2, size / 5, size, col);
                Graphics.DrawTriangle(
                    new Vector2(cx - mw / 2 + size / 5, cy - size / 2),
                    new Vector2(cx, cy - size / 6),
                    new Vector2(cx, cy - size / 2), col);
                Graphics.DrawTriangle(
                    new Vector2(cx, cy - size / 2),
                    new Vector2(cx, cy - size / 6),
                    new Vector2(cx + mw / 2 - size / 5, cy - size / 2), col);
                break;

            case PicType.Star:
                // Stella a 5 punte
                for (int i = 0; i < 5; i++)
                {
                    float a1 = -MathF.PI / 2 + i * MathF.PI * 2 / 5;
                    float a2 = a1 + MathF.PI / 5;
                    Vector2 outer = new Vector2(cx + MathF.Cos(a1) * size / 1.8f, cy + MathF.Sin(a1) * size / 1.8f);
                    Vector2 inner = new Vector2(cx + MathF.Cos(a2) * size / 4f, cy + MathF.Sin(a2) * size / 4f);
                    float a3 = a1 + MathF.PI * 2 / 5;
                    Vector2 outer2 = new Vector2(cx + MathF.Cos(a3) * size / 1.8f, cy + MathF.Sin(a3) * size / 1.8f);
                    // No fill triangle fan — draw thick lines
                    Graphics.DrawTriangle(new Vector2(cx, cy), outer, inner, col);
                    Graphics.DrawTriangle(new Vector2(cx, cy), inner, outer2, col);
                }
                // Occhi
                Graphics.DrawCircleV(new Vector2(cx - size / 6, cy - size / 8), 2, new Color(30, 30, 30, 255));
                Graphics.DrawCircleV(new Vector2(cx + size / 6, cy - size / 8), 2, new Color(30, 30, 30, 255));
                break;
        }
    }

    private void DrawCoin(int x, int y)
    {
        Graphics.DrawCircleV(new Vector2(x + 5, y + 5), 5, coinShadow);
        Graphics.DrawCircleV(new Vector2(x + 5, y + 4), 5, coinGold);
        Graphics.DrawCircleV(new Vector2(x + 5, y + 4), 3, new Color(255, 230, 100, 255));
    }

    private void DrawRoundIntro()
    {
        float t = roundIntroTimer / 1.3f;
        float alpha = t < 0.2f ? t / 0.2f : t > 0.8f ? (1f - t) / 0.2f : 1f;
        float scale = 0.6f + 0.4f * EaseOutBack(Math.Min(1f, t * 2f));

        int pw = (int)(180 * scale);
        int ph = (int)(55 * scale);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.15f, 6,
            new Color(40, 20, 60, (byte)(230 * alpha)));
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.15f, 6, 2,
            new Color(255, 220, 80, (byte)(255 * alpha)));

        string title = $"Round {roundCorrente}";
        int tw = title.Length * 9;
        Graphics.DrawText(title, px + (pw - tw) / 2, py + 10, 18,
            new Color(255, 220, 80, (byte)(255 * alpha)));

        string sub = "Batti Luigi!";
        int sw2 = sub.Length * 5;
        Graphics.DrawText(sub, px + (pw - sw2) / 2, py + 32, 10,
            new Color(200, 200, 210, (byte)(200 * alpha)));
    }

    private void DrawRisultato()
    {
        float t = Math.Min(1f, risultatoTimer * 2.5f);

        int pw = 260;
        int ph = 55;
        int px = (sw - pw) / 2;
        int py = sh / 2 - 35;

        Color bgCol = roundVinto
            ? new Color(30, 70, 45, (byte)(230 * t))
            : new Color(70, 30, 40, (byte)(230 * t));
        Color borderCol = roundVinto ? verdeChiaro : rosso;

        Graphics.DrawRectangleRounded(new Rectangle(px, py, pw, ph), 0.12f, 6, bgCol);
        Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, pw, ph), 0.12f, 6, 2,
            new Color(borderCol.R, borderCol.G, borderCol.B, (byte)(255 * t)));

        int tw1 = risultatoTesto.Length * 7;
        Graphics.DrawText(risultatoTesto, px + (pw - tw1) / 2, py + 10, 14,
            new Color(255, 255, 255, (byte)(255 * t)));

        Color monCol = roundVinto
            ? new Color(100, 255, 120, (byte)(255 * t))
            : new Color(255, 120, 100, (byte)(255 * t));
        int tw2 = vincitaTesto.Length * 5;
        Graphics.DrawText(vincitaTesto, px + (pw - tw2) / 2, py + 32, 10, monCol);
    }

    private void DrawButton(Rectangle rect, string text, bool hover, Color normal, Color hoverCol)
    {
        Color bg = hover ? hoverCol : normal;
        Graphics.DrawRectangleRounded(rect, 0.3f, 6, bg);
        Graphics.DrawRectangleRoundedLines(rect, 0.3f, 6, 1, new Color(255, 255, 255, hover ? (byte)80 : (byte)40));

        int tw = text.Length * 6;
        int tx = (int)rect.X + ((int)rect.Width - tw) / 2;
        int ty = (int)rect.Y + ((int)rect.Height - 12) / 2;
        Graphics.DrawText(text, tx, ty, 12, bianco);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

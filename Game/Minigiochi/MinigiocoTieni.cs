using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

/// <summary>
/// Minigioco: tieni premuto il tasto sinistro dentro al cerchio per X secondi.
/// Il cerchio si sposta periodicamente - devi seguirlo e tenere premuto.
/// </summary>
public class MinigiocoTieni : MinigiocoBase
{
    public override string Nome => "Tieni Premuto";
    public override string Descrizione => "Tieni premuto nel cerchio per riempire la barra!";
    public override TipoMinigioco Tipo => TipoMinigioco.Tieni;

    private Vector2 cerchioPos;
    private float cerchioRaggio = 30f;
    private float tempoTenuto = 0f;
    private float tempoRichiesto = 5f;
    private float spostaTimer = 0f;
    private float spostaIntervallo = 3f;
    private bool isDentro = false;
    private float pulseTime = 0f;
    private float animCerchio = 0f;

    // Feedback
    private float shakeAmount = 0f;

    // Punteggi intermedi (ogni secondo tenuto = 2 punti)
    private float ultimoPuntoTempo = 0f;

    public MinigiocoTieni() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 20f;
        tempoTenuto = 0f;
        tempoRichiesto = 5f;
        spostaTimer = 0f;
        spostaIntervallo = 3f;
        punteggioMassimo = 10;
        punteggio = 0;
        ultimoPuntoTempo = 0f;
        SpostaCerchio();
    }

    protected override void UpdateGioco(float dt)
    {
        pulseTime += dt;
        animCerchio = Math.Min(1f, animCerchio + dt * 6f);

        // Timer spostamento
        spostaTimer += dt;
        if (spostaTimer >= spostaIntervallo)
        {
            spostaTimer = 0f;
            SpostaCerchio();
            // Accelera leggermente
            spostaIntervallo = Math.Max(1.5f, spostaIntervallo - 0.2f);
        }

        // Verifica se il mouse e' dentro e premuto
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        float dist = Vector2.Distance(new Vector2(mx, my), cerchioPos);
        bool mouseInside = dist <= cerchioRaggio;
        bool mouseDown = Input.IsMouseButtonDown(MouseButton.Left);

        isDentro = mouseInside && mouseDown;

        if (isDentro)
        {
            tempoTenuto += dt;
            shakeAmount = Math.Min(3f, shakeAmount + dt * 2f);

            // Assegna punti ogni mezzo secondo tenuto
            if (tempoTenuto - ultimoPuntoTempo >= 0.5f)
            {
                punteggio = Math.Min(punteggioMassimo, punteggio + 1);
                ultimoPuntoTempo = tempoTenuto;
            }

            if (tempoTenuto >= tempoRichiesto)
            {
                punteggio = punteggioMassimo;
                Termina(true);
                return;
            }
        }
        else
        {
            shakeAmount = Math.Max(0f, shakeAmount - dt * 6f);
        }
    }

    private void SpostaCerchio()
    {
        int marginX = 50;
        int marginTop = 50;
        int marginBottom = 40;
        int areaW = sw - marginX * 2;
        int areaH = sh - marginTop - marginBottom;

        cerchioPos = new Vector2(
            marginX + RandomHelper.Int((int)cerchioRaggio, areaW - (int)cerchioRaggio),
            marginTop + RandomHelper.Int((int)cerchioRaggio, areaH - (int)cerchioRaggio)
        );
        animCerchio = 0f;
    }

    protected override void DrawGioco()
    {
        float scale = EaseOutBack(animCerchio);
        float r = cerchioRaggio * scale;

        // Shake se si tiene premuto
        Vector2 drawPos = cerchioPos;
        if (shakeAmount > 0f)
        {
            drawPos.X += RandomHelper.Float(-shakeAmount, shakeAmount);
            drawPos.Y += RandomHelper.Float(-shakeAmount, shakeAmount);
        }

        // Alone che indica tempo rimanente prima dello spostamento
        float spostaPct = spostaTimer / spostaIntervallo;
        byte aloneAlpha = (byte)(30 + spostaPct * 60);
        Color aloneCol = spostaPct > 0.7f
            ? new Color(220, 150, 50, aloneAlpha)
            : new Color(100, 180, 100, aloneAlpha);
        Graphics.DrawCircleV(drawPos, r + 8, aloneCol);

        // Cerchio principale
        Color cerchioCol = isDentro
            ? new Color(80, 220, 80, 255)
            : new Color(100, 140, 180, 255);
        Graphics.DrawCircleV(drawPos, r, cerchioCol);

        // Bordo
        Color bordoCol = isDentro ? verdeChiaro : new Color(150, 180, 220, 200);
        Graphics.DrawCircleLinesV(drawPos, r, bordoCol);

        // Indicatore di riempimento (arco) dentro il cerchio
        float fillPct = tempoTenuto / tempoRichiesto;
        if (fillPct > 0f)
        {
            Color fillCol = new Color(60, 200, 60, 150);
            int segments = (int)(fillPct * 36);
            for (int i = 0; i < segments; i++)
            {
                float angle1 = -MathF.PI / 2f + (i / 36f) * MathF.PI * 2f;
                float angle2 = -MathF.PI / 2f + ((i + 1) / 36f) * MathF.PI * 2f;
                float innerR = r * 0.5f;

                Vector2 p1 = drawPos + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * innerR;
                Vector2 p2 = drawPos + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * innerR;
                Vector2 p3 = drawPos + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * (r - 4);
                Vector2 p4 = drawPos + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * (r - 4);

                Graphics.DrawTriangle(p1, p3, p2, fillCol);
                Graphics.DrawTriangle(p1, p4, p3, fillCol);
            }
        }

        // Testo al centro
        string pctText = $"{(int)(fillPct * 100)}%";
        int pctW = pctText.Length * 6;
        Graphics.DrawText(pctText, (int)drawPos.X - pctW / 2, (int)drawPos.Y - 5, 12, bianco);

        // Barra di progresso in basso
        int barW = sw - 80;
        int barH = 12;
        int barX = 40;
        int barY = sh - 35;

        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, barW, barH), 0.5f, 4, new Color(30, 30, 30, 200));
        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, (int)(barW * fillPct), barH), 0.5f, 4, verdeChiaro);
        Graphics.DrawRectangleRoundedLines(new Rectangle(barX, barY, barW, barH), 0.5f, 4, 1, new Color(80, 160, 80, 150));

        string barLabel = $"Tenuto: {tempoTenuto:0.1}s / {tempoRichiesto:0.0}s";
        int labelW = barLabel.Length * 5;
        Graphics.DrawText(barLabel, barX + (barW - labelW) / 2, barY - 14, 10, grigioChiaro);

        // Hint spostamento
        if (spostaPct > 0.7f)
        {
            float pulse = (MathF.Sin(pulseTime * 6f) + 1f) * 0.5f;
            byte warnA = (byte)(150 + pulse * 105);
            string warn = "Si sposta tra poco!";
            int warnW = warn.Length * 5;
            Graphics.DrawText(warn, (sw - warnW) / 2, 30, 10, new Color(220, 180, 50, warnA));
        }
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System;
using System.Numerics;

namespace Plants;

/// <summary>
/// Minigioco: clicca il cerchio che si sposta ogni volta che lo colpisci.
/// Ogni click corretto = +1 punto. Raggiungi il punteggio massimo per vincere.
/// </summary>
public class MinigiocoCerchio : MinigiocoBase
{
    public override string Nome => "Acchiappa il Cerchio";
    public override string Descrizione => "Clicca il cerchio prima che scada il tempo!";
    public override TipoMinigioco Tipo => TipoMinigioco.Cerchio;

    private Vector2 cerchioPos;
    private float cerchioRaggio = 22f;
    private float cerchioRaggioMin = 14f;
    private float animCerchio = 0f;
    private float pulseTime = 0f;
    private bool shrinking = false;

    // Area di gioco (margini dallo schermo)
    private int marginX = 40;
    private int marginTop = 40;
    private int marginBottom = 30;

    // Feedback visivo
    private Vector2 lastHitPos;
    private float hitAnimTimer = 0f;
    private string hitText = "";

    public MinigiocoCerchio() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 15f;
        punteggioMassimo = 10;
        cerchioRaggio = 22f;
        shrinking = false;
        SpostaCerchio();
    }

    protected override void UpdateGioco(float dt)
    {
        pulseTime += dt;
        hitAnimTimer = Math.Max(0f, hitAnimTimer - dt);

        // Animazione apparizione cerchio
        animCerchio = Math.Min(1f, animCerchio + dt * 8f);

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            float dist = Vector2.Distance(new Vector2(mx, my), cerchioPos);
            if (dist <= cerchioRaggio)
            {
                punteggio++;
                lastHitPos = cerchioPos;
                hitAnimTimer = 0.5f;
                hitText = $"+1";

                if (punteggio >= punteggioMassimo)
                {
                    Termina(true);
                    return;
                }

                // Dopo meta' punti, il cerchio si rimpicciolisce
                if (punteggio >= punteggioMassimo / 2 && !shrinking)
                {
                    shrinking = true;
                }

                if (shrinking)
                {
                    cerchioRaggio = Math.Max(cerchioRaggioMin,
                        cerchioRaggio - (22f - cerchioRaggioMin) / (punteggioMassimo / 2f));
                }

                SpostaCerchio();
            }
        }
    }

    private void SpostaCerchio()
    {
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
        // Area di gioco
        Graphics.DrawRectangleLines(marginX - 2, marginTop - 2,
            sw - marginX * 2 + 4, sh - marginTop - marginBottom + 4,
            new Color(60, 100, 60, 100));

        // Cerchio target
        float scale = EaseOutElastic(animCerchio);
        float r = cerchioRaggio * scale;
        float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;

        // Alone esterno
        byte aloneA = (byte)(40 + pulse * 30);
        Graphics.DrawCircleV(cerchioPos, r + 6, new Color(100, 220, 100, aloneA));

        // Cerchio principale
        Color cerchioCol = new Color(
            (byte)(80 + pulse * 30),
            (byte)(180 + pulse * 40),
            80, 255);
        Graphics.DrawCircleV(cerchioPos, r, cerchioCol);

        // Cerchio interno
        Graphics.DrawCircleV(cerchioPos, r * 0.4f, new Color(60, 140, 60, 200));

        // Bordo
        Graphics.DrawCircleLinesV(cerchioPos, r, new Color(200, 255, 200, 200));

        // Hit feedback
        if (hitAnimTimer > 0f)
        {
            float a = hitAnimTimer / 0.5f;
            float yOff = (1f - a) * 20f;
            byte textA = (byte)(255 * a);
            Graphics.DrawText(hitText,
                (int)lastHitPos.X - 8,
                (int)(lastHitPos.Y - 20 - yOff),
                14, new Color(100, 255, 100, textA));
        }
    }

    private float EaseOutElastic(float x)
    {
        if (x <= 0f) return 0f;
        if (x >= 1f) return 1f;
        float c4 = (2f * MathF.PI) / 3f;
        return MathF.Pow(2f, -10f * x) * MathF.Sin((x * 10f - 0.75f) * c4) + 1f;
    }
}

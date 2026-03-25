using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Numerics;

namespace Plants;

public class MinigiocoResta : MinigiocoBase
{
    public override string Nome => "Resta nel Cerchio";
    public override string Descrizione => "Tieni il mouse nel cerchio che si muove!";
    public override TipoMinigioco Tipo => TipoMinigioco.Resta;

    private Vector2 cerchioPos;
    private float cerchioRaggio = 50f;
    private float percentuale = 0f;
    private Vector2 velocita;
    private float cambioDirezioneTimer = 0f;
    private float cambioDirezioneIntervallo = 1.5f;
    private bool mouseDentro = false;
    private float pulseTime = 0f;
    private float animCerchio = 0f;

    private int marginX = 60;
    private int marginTop = 60;
    private int marginBottom = 50;

    public MinigiocoResta() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 15f;
        punteggioMassimo = 100;
        percentuale = 0f;
        cambioDirezioneTimer = 0f;
        cambioDirezioneIntervallo = 1.5f;
        animCerchio = 0f;

        int areaW = sw - marginX * 2;
        int areaH = sh - marginTop - marginBottom;
        cerchioPos = new Vector2(
            marginX + RandomHelper.Int((int)cerchioRaggio, areaW - (int)cerchioRaggio),
            marginTop + RandomHelper.Int((int)cerchioRaggio, areaH - (int)cerchioRaggio)
        );
        CambiaDirezione();
    }

    private void CambiaDirezione()
    {
        float speed = 180f;
        float angle = RandomHelper.Float(0, MathF.PI * 2);
        velocita = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
    }

    protected override void UpdateGioco(float dt)
    {
        pulseTime += dt;
        animCerchio = Math.Min(1f, animCerchio + dt * 4f);

        cambioDirezioneTimer += dt;
        if (cambioDirezioneTimer >= cambioDirezioneIntervallo)
        {
            cambioDirezioneTimer = 0f;
            CambiaDirezione();
            cambioDirezioneIntervallo = Math.Max(0.8f, cambioDirezioneIntervallo - 0.05f);
        }

        cerchioPos.X += velocita.X * dt;
        cerchioPos.Y += velocita.Y * dt;

        int areaW = sw - marginX * 2;
        int areaH = sh - marginTop - marginBottom;
        float minX = marginX + cerchioRaggio;
        float maxX = marginX + areaW - cerchioRaggio;
        float minY = marginTop + cerchioRaggio;
        float maxY = marginTop + areaH - cerchioRaggio;

        if (cerchioPos.X <= minX || cerchioPos.X >= maxX)
        {
            velocita.X *= -1;
            cerchioPos.X = Math.Clamp(cerchioPos.X, minX, maxX);
        }
        if (cerchioPos.Y <= minY || cerchioPos.Y >= maxY)
        {
            velocita.Y *= -1;
            cerchioPos.Y = Math.Clamp(cerchioPos.Y, minY, maxY);
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        float dist = Vector2.Distance(new Vector2(mx, my), cerchioPos);
        mouseDentro = dist <= cerchioRaggio;

        if (mouseDentro)
        {
            percentuale += dt * 15f;
            if (percentuale >= 100f)
            {
                percentuale = 100f;
                punteggio = 100;
                Termina(true);
                return;
            }
        }
        else
        {
            percentuale = Math.Max(0f, percentuale - dt * 8f);
        }

        punteggio = (int)percentuale;
    }

    protected override void DrawGioco()
    {
        float scale = EaseOutBack(animCerchio);
        float r = cerchioRaggio * scale;
        float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;

        Graphics.DrawRectangleLines(marginX - 2, marginTop - 2,
            sw - marginX * 2 + 4, sh - marginTop - marginBottom + 4,
            new Color(60, 80, 60, 80));

        byte aloneA = mouseDentro ? (byte)(60 + pulse * 40) : (byte)(30 + pulse * 20);
        Color aloneCol = mouseDentro ? new Color(100, 220, 100, aloneA) : new Color(220, 100, 100, aloneA);
        Graphics.DrawCircleV(cerchioPos, r + 10, aloneCol);

        Color cerchioCol = mouseDentro
            ? new Color(80, 200, 80, 255)
            : new Color(180, 100, 100, 220);
        Graphics.DrawCircleV(cerchioPos, r, cerchioCol);

        Color bordoCol = mouseDentro ? verdeChiaro : rosso;
        Graphics.DrawCircleLinesV(cerchioPos, r, new Color(bordoCol.R, bordoCol.G, bordoCol.B, 200));

        float fillPct = percentuale / 100f;
        if (fillPct > 0f)
        {
            Color fillCol = new Color(60, 180, 60, 120);
            int segments = (int)(fillPct * 24);
            for (int i = 0; i < segments; i++)
            {
                float angle1 = -MathF.PI / 2f + (i / 24f) * MathF.PI * 2f;
                float angle2 = -MathF.PI / 2f + ((i + 1) / 24f) * MathF.PI * 2f;
                float innerR = r * 0.4f;

                Vector2 p1 = cerchioPos + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * innerR;
                Vector2 p2 = cerchioPos + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * innerR;
                Vector2 p3 = cerchioPos + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * (r - 5);
                Vector2 p4 = cerchioPos + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * (r - 5);

                Graphics.DrawTriangle(p1, p3, p2, fillCol);
                Graphics.DrawTriangle(p1, p4, p3, fillCol);
            }
        }

        string pctText = $"{(int)percentuale}%";
        int pctW = pctText.Length * 7;
        Color textCol = mouseDentro ? bianco : new Color(255, 220, 220, 255);
        Graphics.DrawText(pctText, (int)cerchioPos.X - pctW / 2, (int)cerchioPos.Y - 8, 14, textCol);

        if (!mouseDentro)
        {
            float warnPulse = (MathF.Sin(pulseTime * 8f) + 1f) * 0.5f;
            byte warnA = (byte)(150 + warnPulse * 100);
            string warn = "Resta dentro!";
            int warnW = warn.Length * 5;
            Graphics.DrawText(warn, (int)cerchioPos.X - warnW / 2, (int)(cerchioPos.Y - r - 15), 10, new Color(255, 150, 150, warnA));
        }

        int barW = sw - 80;
        int barH = 14;
        int barX = 40;
        int barY = sh - 40;

        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, barW, barH), 0.5f, 4, new Color(30, 30, 30, 200));

        Color progressCol = fillPct > 0.5f ? verdeChiaro : fillPct > 0.25f ? new Color(220, 180, 50, 255) : rosso;
        Graphics.DrawRectangleRounded(new Rectangle(barX, barY, (int)(barW * fillPct), barH), 0.5f, 4, progressCol);
        Graphics.DrawRectangleRoundedLines(new Rectangle(barX, barY, barW, barH), 0.5f, 4, 1, new Color(80, 160, 80, 150));

        string barLabel = $"Percentuale: {percentuale:0.0}%";
        int labelW = barLabel.Length * 5;
        Graphics.DrawText(barLabel, barX + (barW - labelW) / 2, barY - 14, 10, grigioChiaro);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

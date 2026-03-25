using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

/// <summary>
/// GUI per conferma, countdown di sopravvivenza e barra di rewind del recupero seme.
/// </summary>
public class Obj_GuiSeedRecovery : GameElement
{
    private float animProgress = 0f;
    private float pulseTime = 0f;

    // Hover pulsanti conferma
    private bool confirmHovered = false;
    private bool cancelHovered = false;

    // Hover pulsante annulla countdown
    private bool cancelCountdownHovered = false;

    // Colori
    private readonly Color panelBg = new Color(25, 28, 22, 245);
    private readonly Color panelBorder = new Color(100, 180, 100, 255);
    private readonly Color headerBg = new Color(40, 55, 35, 255);
    private readonly Color greenBtn = new Color(60, 140, 60, 255);
    private readonly Color greenBtnHover = new Color(80, 180, 80, 255);
    private readonly Color redBtn = new Color(140, 50, 50, 255);
    private readonly Color redBtnHover = new Color(180, 70, 70, 255);
    private readonly Color countdownBg = new Color(20, 25, 18, 220);
    private readonly Color countdownBorder = new Color(120, 160, 60, 255);
    private readonly Color rewindBg = new Color(20, 25, 18, 220);
    private readonly Color rewindFill = new Color(80, 200, 80, 255);
    private readonly Color rewindBorder = new Color(60, 130, 60, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiSeedRecovery()
    {
        this.guiLayer = true;
        this.depth = -2500;
        this.persistent = true;
    }

    public void ShowConfirmation()
    {
        SeedRecoverySystem.IsConfirming = true;
        animProgress = 0f;
        pulseTime = 0f;
    }

    public override void Update()
    {
        float dt = Time.GetFrameTime();
        pulseTime += dt;

        if (SeedRecoverySystem.IsConfirming)
        {
            animProgress = Math.Min(1f, animProgress + dt * 6f);
            if (animProgress > 0.9f)
                HandleConfirmationInput();
        }

        if (SeedRecoverySystem.IsCountdown)
        {
            SeedRecoverySystem.Update(dt);
            HandleCountdownInput();
        }

        if (SeedRecoverySystem.IsRewinding)
        {
            SeedRecoverySystem.Update(dt);
        }
    }

    private void HandleConfirmationInput()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        int pw = Math.Min(260, sw - 30);
        int ph = 150;
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        int btnW = 90;
        int btnH = 26;
        int btnY = py + ph - 40;
        int confirmX = px + pw / 2 - btnW - 8;
        int cancelX = px + pw / 2 + 8;

        confirmHovered = mx >= confirmX && mx <= confirmX + btnW && my >= btnY && my <= btnY + btnH;
        cancelHovered = mx >= cancelX && mx <= cancelX + btnW && my >= btnY && my <= btnY + btnH;

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (confirmHovered)
                SeedRecoverySystem.StartRecovery();
            else if (cancelHovered)
                SeedRecoverySystem.IsConfirming = false;
        }

        if (Input.IsKeyPressed(KeyboardKey.Escape))
            SeedRecoverySystem.IsConfirming = false;
    }

    private void HandleCountdownInput()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Pulsante annulla nel banner countdown
        int barW = sw - 30;
        int barX = 15;
        int barY = 8;
        int btnW = 60;
        int btnH = 16;
        int btnX = barX + barW - btnW - 6;
        int btnBY = barY + 6;

        cancelCountdownHovered = mx >= btnX && mx <= btnX + btnW && my >= btnBY && my <= btnBY + btnH;

        if (cancelCountdownHovered && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            SeedRecoverySystem.CancelRecovery();
        }
    }

    public override void Draw()
    {
        if (SeedRecoverySystem.IsConfirming)
            DrawConfirmation();
        else if (SeedRecoverySystem.IsCountdown)
            DrawCountdown();
        else if (SeedRecoverySystem.IsRewinding)
            DrawRewind();
    }

    // ========== DIALOGO DI CONFERMA ==========

    private void DrawConfirmation()
    {
        if (animProgress < 0.05f) return;

        byte overlayA = (byte)(180 * animProgress);
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, overlayA));

        float eased = EaseOutBack(animProgress);
        int pw = (int)(Math.Min(260, sw - 30) * eased);
        int ph = (int)(150 * eased);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        if (pw < 50) return;

        Graphics.DrawRectangleRounded(
            new Rectangle(px, py, pw, ph), 0.08f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(px, py, pw, ph), 0.08f, 8, 2, panelBorder);

        if (animProgress < 0.5f) return;

        // Header
        Graphics.DrawRectangleRounded(
            new Rectangle(px + 6, py + 6, pw - 12, 35), 0.12f, 6, headerBg);
        Graphics.DrawCircle(px + 22, py + 23, 7, new Color(180, 140, 60, 255));
        Graphics.DrawCircle(px + 22, py + 20, 5, new Color(200, 170, 80, 255));
        Graphics.DrawText("Recupero Seme", px + 38, py + 14, 13, new Color(220, 240, 200, 255));

        // Testo
        string msg1 = "Vuoi recuperare il seme?";
        int msg1W = msg1.Length * 5;
        Graphics.DrawText(msg1, px + (pw - msg1W) / 2, py + 50, 10, new Color(220, 220, 220, 255));

        string msg2 = "Tieni viva la pianta per il tempo";
        int msg2W = msg2.Length * 4;
        Graphics.DrawText(msg2, px + (pw - msg2W) / 2, py + 65, 8, new Color(160, 200, 160, 255));

        string msg3 = "indicato per recuperare il seme.";
        int msg3W = msg3.Length * 4;
        Graphics.DrawText(msg3, px + (pw - msg3W) / 2, py + 77, 8, new Color(160, 200, 160, 255));

        // Durata
        SeedRarity rarity = Seed.GetRarityFromType(Game.pianta.TipoSeme);
        float duration = SeedRecoverySystem.GetDuration(rarity);
        int mins = (int)(duration / 60);
        int secs = (int)(duration % 60);
        string durText = $"Sopravvivi: {mins}:{secs:D2}";
        int durW = durText.Length * 5;
        Graphics.DrawText(durText, px + (pw - durW) / 2, py + 95, 10, new Color(255, 220, 100, 255));

        // Pulsanti
        int btnW = 90;
        int btnH = 26;
        int btnY = py + ph - 40;
        int confirmX = px + pw / 2 - btnW - 8;
        int cancelX = px + pw / 2 + 8;

        Color confirmBg = confirmHovered ? greenBtnHover : greenBtn;
        Graphics.DrawRectangleRounded(
            new Rectangle(confirmX, btnY, btnW, btnH), 0.25f, 6, confirmBg);
        string confLabel = "Conferma";
        int confW = confLabel.Length * 6;
        Graphics.DrawText(confLabel, confirmX + (btnW - confW) / 2, btnY + 7, 11, Color.White);

        Color cancelBg = cancelHovered ? redBtnHover : redBtn;
        Graphics.DrawRectangleRounded(
            new Rectangle(cancelX, btnY, btnW, btnH), 0.25f, 6, cancelBg);
        string cancLabel = "Annulla";
        int cancW = cancLabel.Length * 6;
        Graphics.DrawText(cancLabel, cancelX + (btnW - cancW) / 2, btnY + 7, 11, Color.White);
    }

    // ========== COUNTDOWN DI SOPRAVVIVENZA ==========

    private void DrawCountdown()
    {
        int barMargin = 15;
        int barH = 28;
        int barX = barMargin;
        int barY = 8;
        int barW = sw - barMargin * 2;

        // Sfondo
        Graphics.DrawRectangleRounded(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, countdownBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, 2, countdownBorder);

        // Barra progresso countdown
        float progress = SeedRecoverySystem.CountdownProgress;
        int fillW = (int)((barW - 8) * progress);
        if (fillW > 4)
        {
            // Colore che vira dal giallo al verde man mano che il countdown avanza
            byte r = (byte)(200 - progress * 140);
            byte g = (byte)(180 + progress * 40);
            byte b = 60;
            Graphics.DrawRectangleRounded(
                new Rectangle(barX + 4, barY + 4, fillW, barH - 8), 0.15f, 4,
                new Color(r, g, b, 255));
        }

        // Timer rimanente (grande al centro)
        float remaining = SeedRecoverySystem.CountdownRemaining;
        int mins = (int)(remaining / 60);
        int secs = (int)(remaining % 60);
        string timeText = $"{mins}:{secs:D2}";
        int timeW = timeText.Length * 7;
        Graphics.DrawText(timeText, barX + (barW - timeW) / 2, barY + 4, 12, Color.White);

        // Label
        string label = "Sopravvivi!";
        Graphics.DrawText(label, barX + 8, barY + 8, 9, new Color(255, 220, 100, 255));

        // Pulsante annulla (piccolo, a destra)
        int btnW = 60;
        int btnH = 16;
        int btnX = barX + barW - btnW - 6;
        int btnBY = barY + 6;

        Color btnBg = cancelCountdownHovered
            ? new Color(180, 70, 70, 255)
            : new Color(120, 50, 50, 200);
        Graphics.DrawRectangleRounded(
            new Rectangle(btnX, btnBY, btnW, btnH), 0.3f, 4, btnBg);
        string cancText = "Annulla";
        int cancW = cancText.Length * 5;
        Graphics.DrawText(cancText, btnX + (btnW - cancW) / 2, btnBY + 3, 8, new Color(220, 200, 200, 255));

        // Pulsa il bordo se il tempo sta scadendo (ultimi 30 secondi)
        if (remaining < 30f)
        {
            float pulse = (MathF.Sin(pulseTime * 6f) + 1f) * 0.5f;
            byte borderA = (byte)(100 + pulse * 155);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(barX, barY, barW, barH), 0.2f, 6, 2,
                new Color(100, 255, 100, borderA));
        }
    }

    // ========== REWIND VISIVO ==========

    private void DrawRewind()
    {
        int barMargin = 15;
        int barH = 28;
        int barX = barMargin;
        int barY = 8;
        int barW = sw - barMargin * 2;

        Graphics.DrawRectangleRounded(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, rewindBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, 2, rewindBorder);

        // Barra progresso rewind
        float progress = SeedRecoverySystem.RewindProgress;
        int fillW = (int)((barW - 8) * progress);
        if (fillW > 4)
        {
            float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;
            Color fill = new Color(
                (byte)(60 + pulse * 30),
                (byte)(180 + pulse * 40),
                (byte)(60 + pulse * 30),
                255);
            Graphics.DrawRectangleRounded(
                new Rectangle(barX + 4, barY + 4, fillW, barH - 8), 0.15f, 4, fill);
        }

        // Testo
        string label = "Recupero in corso...";
        Graphics.DrawText(label, barX + 8, barY + 8, 9, new Color(220, 240, 200, 255));

        string pctText = $"{(int)(progress * 100)}%";
        int pctW = pctText.Length * 7;
        Graphics.DrawText(pctText, barX + barW - pctW - 8, barY + 8, 9, new Color(200, 200, 200, 255));

        // Hint input bloccato
        float pulse2 = (MathF.Sin(pulseTime * 2f) + 1f) * 0.5f;
        byte hintA = (byte)(100 + pulse2 * 80);
        string hint = "Input bloccato";
        int hintW = hint.Length * 4;
        Graphics.DrawText(hint, (sw - hintW) / 2, barY + barH + 4, 7, new Color(180, 150, 100, hintA));
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

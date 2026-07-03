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
            {
                InputGate.ConsumeMouse();
                if (Input.IsKeyPressed(KeyboardKey.Escape))
                    SeedRecoverySystem.IsConfirming = false;
            }
        }

        if (SeedRecoverySystem.IsCountdown)
            SeedRecoverySystem.Update(dt);

        if (SeedRecoverySystem.IsRewinding)
            SeedRecoverySystem.Update(dt);
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
        Hud.Overlay(new Color(0, 0, 0, overlayA));

        float eased = EaseOutBack(animProgress);
        int pw = (int)(Math.Min(260, sw - 30) * eased);
        int ph = (int)(150 * eased);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        if (pw < 50) return;

        Hud.Panel(px, py, pw, ph, new Hud.PanelOptions
        {
            Bg = panelBg, Border = panelBorder, Roundness = 0.08f
        });

        if (animProgress < 0.5f) return;

        Hud.Panel(px + 6, py + 6, pw - 12, 35, new Hud.PanelOptions
        {
            Bg = headerBg, Roundness = 0.12f, BorderThickness = 0
        });
        Graphics.DrawCircle(px + 22, py + 23, 7, new Color(180, 140, 60, 255));
        Graphics.DrawCircle(px + 22, py + 20, 5, new Color(200, 170, 80, 255));
        GuiTheme.DrawText("Recupero Seme", px + 38, py + 14, 13, new Color(220, 240, 200, 255));

        string msg1 = "Vuoi recuperare il seme?";
        int msg1W = GuiTheme.MeasureText(msg1);
        GuiTheme.DrawText(msg1, px + (pw - msg1W) / 2, py + 50, 10, new Color(220, 220, 220, 255));

        string msg2 = "Tieni viva la pianta per il tempo";
        int msg2W = GuiTheme.MeasureText(msg2, 8);
        GuiTheme.DrawText(msg2, px + (pw - msg2W) / 2, py + 65, 8, new Color(160, 200, 160, 255));

        string msg3 = "indicato per recuperare il seme.";
        int msg3W = GuiTheme.MeasureText(msg3, 8);
        GuiTheme.DrawText(msg3, px + (pw - msg3W) / 2, py + 77, 8, new Color(160, 200, 160, 255));

        SeedRarity rarity = Seed.GetRarityFromType(Game.pianta.TipoSeme);
        float duration = SeedRecoverySystem.GetDuration(rarity);
        int mins = (int)(duration / 60);
        int secs = (int)(duration % 60);
        string durText = $"Sopravvivi: {mins}:{secs:D2}";
        int durW = GuiTheme.MeasureText(durText);
        GuiTheme.DrawText(durText, px + (pw - durW) / 2, py + 95, 10, new Color(255, 220, 100, 255));

        int btnW = 90;
        int btnH = 26;
        int btnY = py + ph - 40;
        int confirmX = px + pw / 2 - btnW - 8;
        int cancelX = px + pw / 2 + 8;

        Hud.Button(confirmX, btnY, btnW, btnH, "Conferma",
            () => SeedRecoverySystem.StartRecovery(),
            new Hud.ButtonOptions
            {
                Bg = greenBtn, HoverBg = greenBtnHover,
                TextColor = Color.White, FontSize = 11, Roundness = 0.25f,
                BorderThickness = 0
            });

        Hud.Button(cancelX, btnY, btnW, btnH, "Annulla",
            () => { SeedRecoverySystem.IsConfirming = false; },
            new Hud.ButtonOptions
            {
                Bg = redBtn, HoverBg = redBtnHover,
                TextColor = Color.White, FontSize = 11, Roundness = 0.25f,
                BorderThickness = 0
            });
    }

    // ========== COUNTDOWN DI SOPRAVVIVENZA ==========

    private void DrawCountdown()
    {
        int barMargin = 15;
        int barH = 28;
        int barX = barMargin;
        int barY = 8;
        int barW = sw - barMargin * 2;

        Graphics.DrawRectangleRounded(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, countdownBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(barX, barY, barW, barH), 0.2f, 6, 2, countdownBorder);

        float progress = SeedRecoverySystem.CountdownProgress;
        int fillW = (int)((barW - 8) * progress);
        if (fillW > 4)
        {
            byte r = (byte)(200 - progress * 140);
            byte g = (byte)(180 + progress * 40);
            byte b = 60;
            Graphics.DrawRectangleRounded(
                new Rectangle(barX + 4, barY + 4, fillW, barH - 8), 0.15f, 4,
                new Color(r, g, b, 255));
        }

        float remaining = SeedRecoverySystem.CountdownRemaining;
        int mins = (int)(remaining / 60);
        int secs = (int)(remaining % 60);
        string timeText = $"{mins}:{secs:D2}";
        int timeW = GuiTheme.MeasureText(timeText, 12);
        GuiTheme.DrawText(timeText, barX + (barW - timeW) / 2, barY + 4, 12, Color.White);

        string label = "Sopravvivi!";
        GuiTheme.DrawText(label, barX + 8, barY + 8, 9, new Color(255, 220, 100, 255));

        int btnW = 60;
        int btnH = 16;
        int btnX = barX + barW - btnW - 6;
        int btnBY = barY + 6;

        Hud.Button(btnX, btnBY, btnW, btnH, "Annulla",
            () => SeedRecoverySystem.CancelRecovery(),
            new Hud.ButtonOptions
            {
                Bg = new Color(120, 50, 50, 200),
                HoverBg = new Color(180, 70, 70, 255),
                TextColor = new Color(220, 200, 200, 255),
                FontSize = 8, Roundness = 0.3f,
                BorderThickness = 0
            });

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

        string label = "Recupero in corso...";
        GuiTheme.DrawText(label, barX + 8, barY + 8, 9, new Color(220, 240, 200, 255));

        string pctText = $"{(int)(progress * 100)}%";
        int pctW = GuiTheme.MeasureText(pctText, 9);
        GuiTheme.DrawText(pctText, barX + barW - pctW - 8, barY + 8, 9, new Color(200, 200, 200, 255));

        float pulse2 = (MathF.Sin(pulseTime * 2f) + 1f) * 0.5f;
        byte hintA = (byte)(100 + pulse2 * 80);
        string hint = "Input bloccato";
        int hintW = GuiTheme.MeasureText(hint, 7);
        GuiTheme.DrawText(hint, (sw - hintW) / 2, barY + barH + 4, 7, new Color(180, 150, 100, hintA));
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

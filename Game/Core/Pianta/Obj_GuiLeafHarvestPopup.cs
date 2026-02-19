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
/// Popup che mostra il riepilogo della raccolta foglie
/// </summary>
public class Obj_GuiLeafHarvestPopup : GameElement
{
    private bool isVisible = false;
    private HarvestResult currentResult = null;

    // Animazione
    private float animProgress = 0f;
    private const float ANIM_SPEED = 6f;

    // Scroll lista foglie
    private int scrollOffset = 0;
    private const int LEAVES_PER_PAGE = 8;

    // Contatore animato foglie integre
    private float animatedLeaves = 0f;
    private float leavesAnimSpeed = 0f;

    // Particelle
    private List<HarvestParticle> particles = new();
    private float particleTimer = 0f;

    // Hover pulsante
    private bool confirmHovered = false;
    private float pulseTime = 0f;

    // Colori
    private readonly Color panelBg = new Color(25, 30, 20, 245);
    private readonly Color panelBorder = new Color(80, 160, 80, 255);
    private readonly Color headerBg = new Color(40, 60, 35, 255);
    private readonly Color intactColor = new Color(100, 220, 100, 255);
    private readonly Color brokenColor = new Color(200, 80, 80, 255);
    private readonly Color scrollBg = new Color(20, 25, 15, 200);

    private struct HarvestParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    public Obj_GuiLeafHarvestPopup()
    {
        this.guiLayer = true;
        this.depth = -2000; // sempre sopra tutto
        this.persistent = true;
    }

    public void Show(HarvestResult result)
    {
        if (result == null || result.TotalLeaves == 0) return;

        currentResult = result;
        isVisible = true;
        animProgress = 0f;
        scrollOffset = 0;
        animatedLeaves = 0f;
        leavesAnimSpeed = result.IntactLeaves / 1.5f; // arriva in ~1.5 secondi
        particles.Clear();
        particleTimer = 0f;
        pulseTime = 0f;
    }

    public void Hide()
    {
        isVisible = false;
        currentResult = null;
    }

    private void SpawnLeafParticle(int panelX, int panelY, int panelW, int panelH)
    {
        int x = panelX + RandomHelper.Int(10, panelW - 10);
        int y = panelY + RandomHelper.Int(10, panelH - 10);

        bool isGold = RandomHelper.Chance(0.3f);
        Color col = isGold
            ? new Color(255, 210, 80, 255)
            : new Color((byte)(80 + RandomHelper.Int(120)), (byte)(180 + RandomHelper.Int(60)), 80, 255);

        particles.Add(new HarvestParticle
        {
            Position = new Vector2(x, y),
            Velocity = new Vector2(RandomHelper.Float(-20f, 20f), RandomHelper.Float(-40f, -10f)),
            Color = col,
            Life = RandomHelper.Float(0.8f, 1.5f),
            MaxLife = 1.5f,
            Size = RandomHelper.Float(2f, 5f)
        });
    }

    public override void Update()
    {
        if (!isVisible) return;

        float dt = Time.GetFrameTime();
        pulseTime += dt;

        // Animazione apertura
        float target = isVisible ? 1f : 0f;
        animProgress += (target - animProgress) * dt * ANIM_SPEED;
        animProgress = Math.Clamp(animProgress, 0f, 1f);

        // Contatore foglie integre animato
        if (currentResult != null && animatedLeaves < currentResult.IntactLeaves)
        {
            animatedLeaves = Math.Min(currentResult.IntactLeaves, animatedLeaves + leavesAnimSpeed * dt);
        }

        // Particelle occasionali se ci sono foglie integre da mostrare
        if (currentResult != null && currentResult.IntactLeaves > 0 && animProgress > 0.8f)
        {
            particleTimer += dt;
            if (particleTimer > 0.15f)
            {
                particleTimer = 0f;
                int sw = Rendering.camera.screenWidth;
                int sh = Rendering.camera.screenHeight;
                int pw = 320;
                int ph = GetPanelHeight();
                SpawnLeafParticle((sw - pw) / 2, (sh - ph) / 2, pw, ph);
            }
        }

        // Aggiorna particelle
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Life -= dt;
            if (p.Life <= 0) { particles.RemoveAt(i); continue; }
            p.Position += p.Velocity * dt;
            p.Velocity.Y -= 30f * dt;
            particles[i] = p;
        }

        // Input
        if (animProgress > 0.9f)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        int sw = Rendering.camera.screenWidth;
        int sh = Rendering.camera.screenHeight;
        int pw = 320;
        int ph = GetPanelHeight();
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Scroll
        float wheel = Input.GetMouseWheelMove();
        if (wheel != 0 && currentResult != null)
        {
            scrollOffset = Math.Clamp(
                scrollOffset - (int)wheel,
                0,
                Math.Max(0, currentResult.TotalLeaves - LEAVES_PER_PAGE)
            );
        }

        // Pulsante conferma
        int btnW = 120;
        int btnH = 30;
        int btnX = px + (pw - btnW) / 2;
        int btnY = py + ph - 45;

        confirmHovered = mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH;

        if (confirmHovered && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Hide();
        }

        // ESC per chiudere
        if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Enter))
        {
            Hide();
        }
    }

    private int GetPanelHeight()
    {
        // Altezza dinamica basata sul contenuto, ma limitata
        return Math.Min(480, 180 + Math.Min(LEAVES_PER_PAGE, currentResult?.TotalLeaves ?? 0) * 22 + 60);
    }

    public override void Draw()
    {
        if (!isVisible || currentResult == null) return;
        if (animProgress < 0.05f) return;

        int sw = Rendering.camera.screenWidth;
        int sh = Rendering.camera.screenHeight;

        // Overlay scuro
        byte overlayA = (byte)(160 * animProgress);
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, overlayA));

        // Pannello
        float eased = EaseOutBack(animProgress);
        int pw = (int)(320 * eased);
        int ph = (int)(GetPanelHeight() * eased);
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        if (pw < 60) return;

        // Sfondo pannello
        Graphics.DrawRectangleRounded(
            new Rectangle(px, py, pw, ph),
            0.08f, 8, panelBg
        );
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(px, py, pw, ph),
            0.08f, 8, 3, panelBorder
        );

        if (animProgress < 0.5f) return;

        DrawHeader(px, py, pw);
        DrawSummary(px, py + 65, pw);
        DrawLeafList(px, py + 130, pw, ph - 200);
        DrawConfirmButton(px, py + ph - 45, pw);

        // Particelle
        foreach (var p in particles)
        {
            float a = p.Life / p.MaxLife;
            Color col = new Color(p.Color.R, p.Color.G, p.Color.B, (byte)(p.Color.A * a));
            Graphics.DrawCircleV(p.Position, p.Size * a, col);
        }
    }

    private void DrawHeader(int px, int py, int pw)
    {
        // Header verde
        Graphics.DrawRectangleRounded(
            new Rectangle(px + 8, py + 8, pw - 16, 50),
            0.15f, 6, headerBg
        );

        // Icona foglia (emoji-like semplice)
        Graphics.DrawCircle(px + 28, py + 33, 10, new Color(60, 180, 60, 255));
        Graphics.DrawCircle(px + 36, py + 27, 7, new Color(80, 200, 80, 255));

        // Titolo
        string title = currentResult.TriggerReason == "Pianta morta"
            ? "Raccolta d'Emergenza"
            : "Raccolta Foglie";
        Graphics.DrawText(title, px + 55, py + 18, 15, new Color(200, 255, 200, 255));

        // Sottotitolo (motivo)
        string sub = currentResult.TriggerReason;
        Graphics.DrawText(sub, px + 55, py + 37, 9, new Color(150, 200, 150, 255));
    }

    private void DrawSummary(int px, int py, int pw)
    {
        // Box riepilogo con 3 colonne
        Graphics.DrawRectangleRounded(
            new Rectangle(px + 8, py, pw - 16, 52),
            0.12f, 6, new Color(30, 40, 25, 220)
        );

        int colW = (pw - 16) / 3;

        // Totale
        DrawStatBox(px + 8, py + 4, colW, "TOTALI", currentResult.TotalLeaves.ToString(),
            new Color(200, 200, 200, 255));

        // Integre
        DrawStatBox(px + 8 + colW, py + 4, colW, "INTEGRE", currentResult.IntactLeaves.ToString(),
            intactColor);

        // Rotte
        DrawStatBox(px + 8 + colW * 2, py + 4, colW, "ROTTE", currentResult.BrokenLeaves.ToString(),
            brokenColor);

        // Foglie guadagnate (sotto)
        int leavesY = py + 38;
        string leavesStr = $"+{(int)animatedLeaves} Foglie";
        int leavesW = leavesStr.Length * 7 + 10;
        int leavesX = px + (pw - leavesW) / 2;

        // Pulsa
        float pulse = (MathF.Sin(pulseTime * 4f) + 1f) * 0.5f;
        byte leafAlpha = (byte)(200 + pulse * 55);

        Color leafGlow = new Color(100, 220, 100, leafAlpha);
        Graphics.DrawText(leavesStr, leavesX + 5, leavesY, 12, leafGlow);

        // Piccola icona foglia
        DrawLeafIcon(leavesX + leavesW - 8, leavesY + 6, 5);
    }

    private void DrawStatBox(int x, int y, int w, string label, string value, Color valueColor)
    {
        int textW = label.Length * 5;
        Graphics.DrawText(label, x + (w - textW) / 2, y + 2, 8, new Color(160, 160, 160, 255));

        int valW = value.Length * 8;
        Graphics.DrawText(value, x + (w - valW) / 2, y + 14, 14, valueColor);
    }

    private void DrawLeafList(int px, int py, int pw, int maxH)
    {
        if (currentResult.TotalLeaves == 0) return;

        // Titolo lista
        Graphics.DrawText("Dettaglio foglie:", px + 12, py, 9, new Color(150, 180, 150, 255));

        int listY = py + 14;
        int listH = maxH - 14;

        // Area scroll
        Graphics.DrawRectangle(px + 8, listY, pw - 16, listH, scrollBg);

        int endIdx = Math.Min(scrollOffset + LEAVES_PER_PAGE, currentResult.TotalLeaves);
        int rowH = 22;

        for (int i = scrollOffset; i < endIdx; i++)
        {
            var leaf = currentResult.Leaves[i];
            int rowY = listY + (i - scrollOffset) * rowH;

            // Sfondo alternato
            if (i % 2 == 0)
            {
                Graphics.DrawRectangle(px + 8, rowY, pw - 16, rowH,
                    new Color(35, 45, 28, 150));
            }

            // Indicatore intatta/rotta
            Color dotColor = leaf.IsIntact ? intactColor : brokenColor;
            Graphics.DrawCircle(px + 20, rowY + rowH / 2, 4, dotColor);

            // Numero foglia
            Graphics.DrawText($"#{i + 1}", px + 30, rowY + 5, 9, new Color(150, 150, 150, 255));

            if (leaf.IsIntact)
            {
                // Barra qualità
                int barX = px + 58;
                int barW = 80;
                int barY = rowY + 8;
                int barH = 6;

                Graphics.DrawRectangle(barX, barY, barW, barH, new Color(30, 40, 25, 255));

                Color barColor = leaf.Quality > 0.7f
                    ? new Color(80, 230, 80, 255)
                    : leaf.Quality > 0.4f
                        ? new Color(200, 200, 60, 255)
                        : new Color(200, 130, 60, 255);

                Graphics.DrawRectangle(barX, barY, (int)(barW * leaf.Quality), barH, barColor);

                Graphics.DrawText("integra", px + pw - 55, rowY + 5, 8, intactColor);
            }
            else
            {
                // Motivo del danno
                string reason = leaf.DamageReason;
                Graphics.DrawText(reason, px + 58, rowY + 5, 8, new Color(160, 100, 100, 255));

                Graphics.DrawText("rotta", px + pw - 45, rowY + 5, 8, brokenColor);
            }
        }

        // Indicatore scroll se ci sono più foglie
        if (currentResult.TotalLeaves > LEAVES_PER_PAGE)
        {
            string scrollText = $"{scrollOffset + 1}-{endIdx} / {currentResult.TotalLeaves}  (scroll)";
            int scrollTextW = scrollText.Length * 5;
            Graphics.DrawText(scrollText, px + (pw - scrollTextW) / 2, listY + listH - 11, 8,
                new Color(120, 120, 120, 255));
        }
    }

    private void DrawConfirmButton(int px, int py, int pw)
    {
        int btnW = 120;
        int btnH = 30;
        int btnX = px + (pw - btnW) / 2;

        float pulse = (MathF.Sin(pulseTime * 3f) + 1f) * 0.5f;
        Color bg = confirmHovered
            ? new Color(80, 180, 80, 255)
            : new Color((byte)(55 + pulse * 15), (byte)(130 + pulse * 20), 55, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(btnX, py, btnW, btnH),
            0.3f, 8, bg
        );
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(btnX, py, btnW, btnH),
            0.3f, 8, 2,
            confirmHovered ? Color.White : new Color(120, 220, 120, 255)
        );

        string label = "Continua";
        int lw = label.Length * 7;
        Graphics.DrawText(label, btnX + (btnW - lw) / 2, py + 8, 13, Color.White);
    }

    private void DrawLeafIcon(int x, int y, int size)
    {
        float pulse = (MathF.Sin(pulseTime * 3f) + 1f) * 0.5f;
        byte a = (byte)(180 + pulse * 75);
        Color col = new Color(80, (byte)(200 + pulse * 55), 80, a);

        // Foglia stilizzata come ellisse ruotata
        Graphics.DrawEllipse(x, y, size, size / 2, col);
        Graphics.DrawLine(x - size, y, x + size, y, new Color(col.R, col.G, col.B, (byte)(a / 2)));
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}

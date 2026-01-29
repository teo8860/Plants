using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Tools;

namespace Plants;

public enum PackOpeningPhase
{
    Idle,
    Spinning,
    Slowing,
    Stopping,
    Reveal,
    WaitingForClick
}

public class Obj_GuiPackOpeningAnimation : GameElement
{
    private PackOpeningPhase currentPhase = PackOpeningPhase.Idle;
    private Seed resultSeed;
    private Obj_Seed visualSeed;

    // Timing
    private float phaseTimer = 0f;
    private const float SPIN_DURATION = 0.5f;
    private float slowDuration = 1.0f; // Will vary by rarity
    private const float STOP_DURATION = 0.5f;
    private float revealDuration = 1.5f; // Will vary by rarity

    // Slot machine effect
    private List<SlotBar> slotBars = new();
    private float scrollSpeed = 0f;
    private float targetScrollSpeed = 0f;
    private const float MAX_SCROLL_SPEED = 2000f;

    // Particles
    private List<RevealParticle> particles = new();
    private const int MAX_PARTICLES = 200;

    // Rarity colors
    private Dictionary<SeedRarity, Color> rarityColors = new()
    {
        { SeedRarity.Comune, new Color(200, 200, 200, 255) },
        { SeedRarity.NonComune, new Color(80, 200, 80, 255) },
        { SeedRarity.Raro, new Color(80, 150, 255, 255) },
        { SeedRarity.Epico, new Color(180, 80, 255, 255) },
        { SeedRarity.Leggendario, new Color(255, 180, 50, 255) }
    };

    // UI
    private bool canSkip = false;
    private float skipButtonAlpha = 0f;
    private bool clickPromptVisible = false;
    private float clickPromptPulse = 0f;

    // Animation state
    private float revealScale = 0f;
    private float revealGlowIntensity = 0f;
    private float rayRotation = 0f;

    private struct SlotBar
    {
        public float Y;
        public Color Color;
        public float Width;
        public float Height;
    }

    private struct RevealParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Size;
        public float Rotation;
        public float RotationSpeed;
    }

    public Obj_GuiPackOpeningAnimation()
    {
        this.guiLayer = true;
        this.depth = -1000; // Top layer
        this.active = false;
        this.roomId = Game.room_compost.id;
    }

    /// <summary>
    /// Call this method to start the pack opening animation
    /// </summary>
    public void StartAnimation(Seed seed)
    {
        resultSeed = seed;
        currentPhase = PackOpeningPhase.Spinning;
        phaseTimer = 0f;
        canSkip = false;
        skipButtonAlpha = 0f;
        clickPromptVisible = false;
        revealScale = 0f;
        revealGlowIntensity = 0f;
        rayRotation = 0f;
        particles.Clear();
        slotBars.Clear();

        // Configure animation duration based on rarity
        switch (resultSeed.rarity)
        {
            case SeedRarity.Comune:
                slowDuration = 1.0f;
                revealDuration = 1.0f;
                break;
            case SeedRarity.NonComune:
                slowDuration = 1.2f;
                revealDuration = 1.5f;
                break;
            case SeedRarity.Raro:
                slowDuration = 1.5f;
                revealDuration = 2.0f;
                break;
            case SeedRarity.Epico:
                slowDuration = 2.0f;
                revealDuration = 2.5f;
                break;
            case SeedRarity.Leggendario:
                slowDuration = 2.5f;
                revealDuration = 3.0f;
                break;
        }

        // Create visual seed
        visualSeed = new Obj_Seed(resultSeed)
        {
            scale = 8f,
            position = new Vector2(
                Rendering.camera.screenWidth / 2,
                Rendering.camera.screenHeight / 2
            )
        };

        // Set seed color based on type
        if (resultSeed.type == SeedType.Glaciale)
            visualSeed.color = new Vector3(1f, 1f, 1f);
        else if (resultSeed.type == SeedType.Magmatico)
            visualSeed.color = new Vector3(1f, 0f, 0f);
        else if (resultSeed.type == SeedType.Cosmico)
            visualSeed.color = new Vector3(0.1f, 0.1f, 0.1f);

        // Initialize slot bars with random colors
        for (int i = 0; i < 20; i++)
        {
            slotBars.Add(new SlotBar
            {
                Y = i * 80f - 400f,
                Color = GetRandomRarityColor(),
                Width = 300f,
                Height = 60f
            });
        }

        scrollSpeed = MAX_SCROLL_SPEED;
        targetScrollSpeed = MAX_SCROLL_SPEED;

        this.active = true;
    }

    private Color GetRandomRarityColor()
    {
        var rarities = new[] {
            SeedRarity.Comune,
            SeedRarity.NonComune,
            SeedRarity.Raro,
            SeedRarity.Epico,
            SeedRarity.Leggendario
        };
        return rarityColors[rarities[RandomHelper.Int(0, rarities.Length)]];
    }

    public override void Update()
    {
        if (currentPhase == PackOpeningPhase.Idle) return;

        float deltaTime = Time.GetFrameTime();
        phaseTimer += deltaTime;
        clickPromptPulse += deltaTime;

        // Skip functionality
        if (canSkip && Input.IsKeyPressed(KeyboardKey.Space))
        {
            SkipToReveal();
        }

        // Enable skip after 0.5 seconds
        if (phaseTimer > 0.5f && !canSkip)
        {
            canSkip = true;
        }

        if (canSkip)
        {
            skipButtonAlpha = Math.Min(255f, skipButtonAlpha + deltaTime * 500f);
        }

        switch (currentPhase)
        {
            case PackOpeningPhase.Spinning:
                UpdateSpinning(deltaTime);
                break;
            case PackOpeningPhase.Slowing:
                UpdateSlowing(deltaTime);
                break;
            case PackOpeningPhase.Stopping:
                UpdateStopping(deltaTime);
                break;
            case PackOpeningPhase.Reveal:
                UpdateReveal(deltaTime);
                break;
            case PackOpeningPhase.WaitingForClick:
                UpdateWaitingForClick(deltaTime);
                break;
        }

        // Update visual seed
        if (visualSeed != null)
        {
            visualSeed.Update();
        }

        // Update particles
        UpdateParticles(deltaTime);
    }

    private void UpdateSpinning(float deltaTime)
    {
        UpdateSlotBars(deltaTime);

        if (phaseTimer >= SPIN_DURATION)
        {
            currentPhase = PackOpeningPhase.Slowing;
            phaseTimer = 0f;
            targetScrollSpeed = 0f;
        }
    }

    private void UpdateSlowing(float deltaTime)
    {
        // Gradually slow down
        float progress = phaseTimer / slowDuration;
        scrollSpeed = MathHelper.Lerp(MAX_SCROLL_SPEED, 100f, progress);

        UpdateSlotBars(deltaTime);

        if (phaseTimer >= slowDuration)
        {
            currentPhase = PackOpeningPhase.Stopping;
            phaseTimer = 0f;
            scrollSpeed = 100f;
        }
    }

    private void UpdateStopping(float deltaTime)
    {
        // Final slowdown
        float progress = phaseTimer / STOP_DURATION;
        scrollSpeed = MathHelper.Lerp(100f, 0f, EaseOutCubic(progress));

        UpdateSlotBars(deltaTime);

        if (phaseTimer >= STOP_DURATION)
        {
            currentPhase = PackOpeningPhase.Reveal;
            phaseTimer = 0f;
            scrollSpeed = 0f;
            slotBars.Clear();
            SpawnRevealParticles();
        }
    }

    private void UpdateReveal(float deltaTime)
    {
        float progress = phaseTimer / revealDuration;

        // Scale up seed
        revealScale = EaseOutBack(Math.Min(1f, progress * 1.5f));

        // Glow intensity
        revealGlowIntensity = MathF.Sin(progress * MathF.PI);

        // Ray rotation
        rayRotation += deltaTime * 50f;

        // Spawn particles throughout reveal
        if (resultSeed.rarity >= SeedRarity.Raro && RandomHelper.Chance(0.3f))
        {
            SpawnRevealParticles(3);
        }

        if (phaseTimer >= revealDuration)
        {
            currentPhase = PackOpeningPhase.WaitingForClick;
            phaseTimer = 0f;
            clickPromptVisible = true;
            revealScale = 1f;
        }
    }

    private void UpdateWaitingForClick(float deltaTime)
    {
        rayRotation += deltaTime * 30f;

        // Check for click to exit
        if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyboardKey.Space))
        {
            CloseAnimation();
        }
    }

    private void UpdateSlotBars(float deltaTime)
    {
        int screenHeight = Rendering.camera.screenHeight;

        // Scroll bars down
        for (int i = slotBars.Count - 1; i >= 0; i--)
        {
            var bar = slotBars[i];
            bar.Y += scrollSpeed * deltaTime;

            // If bar goes off bottom, wrap to top with new color
            if (bar.Y > screenHeight + bar.Height)
            {
                bar.Y = -bar.Height;
                bar.Color = GetRandomRarityColor();
            }

            slotBars[i] = bar;
        }
    }

    private void UpdateParticles(float deltaTime)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Life -= deltaTime;

            if (p.Life <= 0)
            {
                particles.RemoveAt(i);
                continue;
            }

            p.Position += p.Velocity * deltaTime;
            p.Velocity.Y += 100f * deltaTime; // Gravity
            p.Rotation += p.RotationSpeed * deltaTime;

            particles[i] = p;
        }
    }

    private void SpawnRevealParticles(int count = -1)
    {
        int particleCount = count > 0 ? count : GetParticleCountForRarity();
        Vector2 center = new Vector2(
            Rendering.camera.screenWidth / 2,
            Rendering.camera.screenHeight / 2
        );

        Color rarityColor = rarityColors[resultSeed.rarity];

        for (int i = 0; i < particleCount && particles.Count < MAX_PARTICLES; i++)
        {
            float angle = RandomHelper.Float(0, MathF.PI * 2);
            float speed = RandomHelper.Float(100f, 300f);

            particles.Add(new RevealParticle
            {
                Position = center + RandomHelper.InsideCircle(20f),
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = new Color(
                    (byte)Math.Min(255, rarityColor.R + RandomHelper.Int(-30, 30)),
                    (byte)Math.Min(255, rarityColor.G + RandomHelper.Int(-30, 30)),
                    (byte)Math.Min(255, rarityColor.B + RandomHelper.Int(-30, 30)),
                    255
                ),
                Life = RandomHelper.Float(0.5f, 1.5f),
                MaxLife = 1.5f,
                Size = RandomHelper.Float(2f, 6f),
                Rotation = RandomHelper.Float(0, 360f),
                RotationSpeed = RandomHelper.Float(-200f, 200f)
            });
        }
    }

    private int GetParticleCountForRarity()
    {
        return resultSeed.rarity switch
        {
            SeedRarity.Comune => 20,
            SeedRarity.NonComune => 40,
            SeedRarity.Raro => 60,
            SeedRarity.Epico => 80,
            SeedRarity.Leggendario => 120,
            _ => 20
        };
    }

    private void SkipToReveal()
    {
        currentPhase = PackOpeningPhase.Reveal;
        phaseTimer = revealDuration - 0.5f; // Skip to near end
        slotBars.Clear();
        scrollSpeed = 0f;
        SpawnRevealParticles();
    }

    private void CloseAnimation()
    {
        currentPhase = PackOpeningPhase.Idle;
        this.active = false;
        particles.Clear();
        slotBars.Clear();
        visualSeed = null;
    }

    public override void Draw()
    {
        if (currentPhase == PackOpeningPhase.Idle) return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        // Draw based on phase
        if (currentPhase == PackOpeningPhase.Spinning ||
            currentPhase == PackOpeningPhase.Slowing ||
            currentPhase == PackOpeningPhase.Stopping)
        {
            DrawSlotMachine();
        }
        else if (currentPhase == PackOpeningPhase.Reveal ||
                 currentPhase == PackOpeningPhase.WaitingForClick)
        {
            DrawReveal();
        }

        // Draw particles
        DrawParticles();

        // Draw skip button
        if (canSkip && currentPhase != PackOpeningPhase.WaitingForClick)
        {
            DrawSkipButton();
        }

        // Draw click prompt
        if (clickPromptVisible)
        {
            DrawClickPrompt();
        }
    }

    private void DrawSlotMachine()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        int centerX = screenW / 2;

        // Draw slot bars
        foreach (var bar in slotBars)
        {
            int barX = (int)(centerX - bar.Width / 2);
            int barY = (int)bar.Y;

            // Glow effect
            Color glowColor = new Color(bar.Color.R, bar.Color.G, bar.Color.B, 100);
            Graphics.DrawRectangle(barX - 10, barY - 5, (int)bar.Width + 20, (int)bar.Height + 10, glowColor);

            // Main bar
            Graphics.DrawRectangle(barX, barY, (int)bar.Width, (int)bar.Height, bar.Color);

            // Highlight
            Color highlightColor = new Color(255, 255, 255, 100);
            Graphics.DrawRectangle(barX, barY, (int)bar.Width, 10, highlightColor);
        }

        // Draw center frame (focus area)
        int frameY = screenH / 2 - 40;
        int frameHeight = 80;
        Color frameColor = new Color(255, 255, 255, 150);
        Graphics.DrawRectangleLines(centerX - 160, frameY, 320, frameHeight, frameColor);
        Graphics.DrawRectangleLines(centerX - 161, frameY - 1, 322, frameHeight + 2, frameColor);
    }

    private void DrawReveal()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        Vector2 center = new Vector2(screenW / 2, screenH / 2);

        Color rarityColor = rarityColors[resultSeed.rarity];

        // Draw rays for Epic/Legendary
        if (resultSeed.rarity >= SeedRarity.Epico)
        {
            DrawRays(center, rarityColor);
        }

        // Draw glow circle
        float glowSize = 100f + revealGlowIntensity * 50f;
        byte glowAlpha = (byte)(revealGlowIntensity * 150f);
        Color glowColor = new Color(rarityColor.R, rarityColor.G, rarityColor.B, glowAlpha);

        Graphics.DrawCircleGradient(
            (int)center.X, (int)center.Y, glowSize,
            glowColor,
            new Color(rarityColor.R, rarityColor.G, rarityColor.B, 0)
        );

        // Draw inner bright glow
        Graphics.DrawCircleGradient(
            (int)center.X, (int)center.Y, 60f * revealScale,
            new Color(255, 255, 255, (byte)(revealGlowIntensity * 200f)),
            new Color(rarityColor.R, rarityColor.G, rarityColor.B, 0)
        );

        // Draw seed with scale
        if (visualSeed != null && revealScale > 0.1f)
        {
            visualSeed.scale = 8f * revealScale;
            visualSeed.Draw();
        }

        // Draw seed name and rarity
        if (revealScale > 0.7f)
        {
            string seedName = SeedDataType.GetName(resultSeed.type);
            string rarityName = resultSeed.rarity.ToString();

            int nameWidth = seedName.Length * 8;
            int rarityWidth = rarityName.Length * 6;

            Graphics.DrawText(seedName, screenW / 2 - nameWidth / 2, screenH / 2 + 100, 16, Color.White);
            Graphics.DrawText(rarityName, screenW / 2 - rarityWidth / 2, screenH / 2 + 120, 12, rarityColor);
        }
    }

    private void DrawRays(Vector2 center, Color color)
    {
        int rayCount = resultSeed.rarity == SeedRarity.Leggendario ? 12 : 8;
        float rayLength = 150f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (rayRotation + i * (360f / rayCount)) * MathF.PI / 180f;
            Vector2 end = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * rayLength;

            // Draw ray as thick line with gradient
            Color rayColor = new Color(color.R, color.G, color.B, (byte)(revealGlowIntensity * 200f));
            Graphics.DrawLineEx(center, end, 3f, rayColor);
        }
    }

    private void DrawParticles()
    {
        foreach (var p in particles)
        {
            float alpha = p.Life / p.MaxLife;
            Color col = new Color(
                p.Color.R, p.Color.G, p.Color.B,
                (byte)(p.Color.A * alpha)
            );

            // Draw as small rotating rectangle
            Vector2[] points = new Vector2[4];
            float halfSize = p.Size / 2f;
            float cos = MathF.Cos(p.Rotation * MathF.PI / 180f);
            float sin = MathF.Sin(p.Rotation * MathF.PI / 180f);

            points[0] = p.Position + new Vector2(-halfSize * cos + halfSize * sin, -halfSize * sin - halfSize * cos);
            points[1] = p.Position + new Vector2(halfSize * cos + halfSize * sin, halfSize * sin - halfSize * cos);
            points[2] = p.Position + new Vector2(halfSize * cos - halfSize * sin, halfSize * sin + halfSize * cos);
            points[3] = p.Position + new Vector2(-halfSize * cos - halfSize * sin, -halfSize * sin + halfSize * cos);

            Graphics.DrawTriangle(points[0], points[1], points[2], col);
            Graphics.DrawTriangle(points[0], points[2], points[3], col);
        }
    }

    private void DrawSkipButton()
    {
        int screenW = Rendering.camera.screenWidth;

        byte alpha = (byte)skipButtonAlpha;
        Color bgColor = new Color(50, 50, 60, (byte)(alpha * 0.8f));
        Color textColor = new Color(255, 255, 255, alpha);

        string text = "SPACE to skip";
        int textWidth = text.Length * 6;
        int x = screenW / 2 - textWidth / 2 - 10;
        int y = 20;

        Graphics.DrawRectangleRounded(
            new Rectangle(x, y, textWidth + 20, 25),
            0.3f, 6, bgColor
        );

        Graphics.DrawText(text, x + 10, y + 6, 10, textColor);
    }

    private void DrawClickPrompt()
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        float pulse = (MathF.Sin(clickPromptPulse * 3f) + 1f) * 0.5f;
        byte alpha = (byte)(200 + pulse * 55);

        string text = "Click to continue";
        int textWidth = text.Length * 7;

        Graphics.DrawText(text, screenW / 2 - textWidth / 2, screenH - 60, 14, new Color(255, 255, 255, alpha));
    }

    private float EaseOutCubic(float x)
    {
        return 1f - MathF.Pow(1f - x, 3f);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}
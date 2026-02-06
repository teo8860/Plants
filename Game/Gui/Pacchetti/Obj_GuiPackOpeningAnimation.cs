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

public enum PackOpeningPhase
{
    Idle,
    PackAppear,
    PackShake,
    PackOpen,
    RarityRoulette,
    SeedReveal,
    WaitingForClick
}

public class Obj_GuiPackOpeningAnimation : GameElement
{
    private PackOpeningPhase currentPhase = PackOpeningPhase.Idle;
    private Seed resultSeed;
    private Obj_Seed visualSeed;

    // Timing
    private float phaseTimer = 0f;
    private const float APPEAR_DURATION = 0.6f;
    private const float SHAKE_DURATION = 0.8f;
    private const float OPEN_DURATION = 0.5f;
    private const float ROULETTE_SPIN_DURATION = 1.5f;
    private const float ROULETTE_SLOW_DURATION = 2.0f;
    private const float ROULETTE_STOP_DURATION = 1.0f;
    private float revealDuration = 2.0f;

    // Pack animation
    private Vector2 packPosition;
    private float packScale = 0f;
    private float packRotation = 0f;
    private float shakeIntensity = 0f;
    private Vector2 packOffset;
    private SeedPackageRarity packageRarity;

    // Seed card animation
    private float cardRevealProgress = 0f;
    private float cardRotation = 0f;
    private float cardGlowPulse = 0f;

    // Rarity roulette
    private float rouletteSpeed = 0f;
    private float currentRarityIndex = 0f;
    private SeedRarity[] raritySequence = {
        SeedRarity.Comune,
        SeedRarity.NonComune,
        SeedRarity.Raro,
        SeedRarity.Epico,
        SeedRarity.Leggendario
    };
    private Color currentRouletteColor;
    private float rouletteGlowIntensity = 0f;
    private bool isStopPhaseInitialized = false;
    private float stopStartRarityIndex;
    private float stopTargetRarityIndex;


    // Particles
    private List<RevealParticle> particles = new();
    private List<StarParticle> starParticles = new();
    private List<EnergyWave> energyWaves = new();
    private const int MAX_PARTICLES = 300;

    // Rarity colors
    private Dictionary<SeedRarity, Color> rarityColors = new()
    {
        { SeedRarity.Comune, new Color(200, 200, 200, 255) },
        { SeedRarity.NonComune, new Color(80, 200, 80, 255) },
        { SeedRarity.Raro, new Color(80, 150, 255, 255) },
        { SeedRarity.Epico, new Color(180, 80, 255, 255) },
        { SeedRarity.Leggendario, new Color(255, 180, 50, 255) }
    };

    // Rarity Package colors
    private Dictionary<SeedPackageRarity, Color> rarityPackageColors = new()
    {
        { SeedPackageRarity.Common, new Color(200, 200, 200, 255) },
        { SeedPackageRarity.Uncommon, new Color(80, 200, 80, 255) },
        { SeedPackageRarity.Rare, new Color(80, 150, 255, 255) },
        { SeedPackageRarity.Epic, new Color(180, 80, 255, 255) },
        { SeedPackageRarity.Legendary, new Color(255, 180, 50, 255) }
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
    private float backgroundPulse = 0f;
    private float energyWaveProgress = 0f;
    private float screenShake = 0f;

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

    private struct StarParticle
    {
        public Vector2 Position;
        public float Life;
        public float MaxLife;
        public float Size;
        public Color Color;
        public float TwinklePhase;
    }

    private struct EnergyWave
    {
        public float Radius;
        public float MaxRadius;
        public float Alpha;
        public Color Color;
    }

    public Obj_GuiPackOpeningAnimation()
    {
        this.guiLayer = true;
        this.depth = -1000;
        this.active = false;
        this.roomId = Game.room_compost.id;
    }

    public void StartAnimation(Seed seed, SeedPackageRarity packageRarity = SeedPackageRarity.Common)
    {
        resultSeed = seed;
        this.packageRarity = packageRarity;
        currentPhase = PackOpeningPhase.PackAppear;
        phaseTimer = 0f;
        canSkip = false;
        skipButtonAlpha = 0f;
        clickPromptVisible = false;
        revealScale = 0f;
        revealGlowIntensity = 0f;
        rayRotation = 0f;
        packScale = 0f;
        packRotation = 0f;
        shakeIntensity = 0f;
        cardRevealProgress = 0f;
        cardRotation = 0f;
        cardGlowPulse = 0f;
        backgroundPulse = 0f;
        energyWaveProgress = 0f;
        screenShake = 0f;
        rouletteSpeed = 0f;
        currentRarityIndex = 0f;
        currentRouletteColor = rarityColors[SeedRarity.Comune];
        rouletteGlowIntensity = 0f;

        particles.Clear();
        starParticles.Clear();
        energyWaves.Clear();

        packPosition = new Vector2(
            Rendering.camera.screenWidth / 2,
            Rendering.camera.screenHeight / 2
        );

        switch (resultSeed.rarity)
        {
            case SeedRarity.Comune:
                revealDuration = 1.0f;
                break;
            case SeedRarity.NonComune:
                revealDuration = 1.5f;
                break;
            case SeedRarity.Raro:
                revealDuration = 2.0f;
                break;
            case SeedRarity.Epico:
                revealDuration = 2.5f;
                break;
            case SeedRarity.Leggendario:
                revealDuration = 3.0f;
                break;
        }

        visualSeed = new Obj_Seed(resultSeed)
        {
            scale = 8f,
            position = new Vector2(
                Rendering.camera.screenWidth / 2,
                Rendering.camera.screenHeight / 2
            )
        };

        this.active = true;
    }

    public override void Update()
    {
        if (currentPhase == PackOpeningPhase.Idle) return;

        float deltaTime = Time.GetFrameTime();
        phaseTimer += deltaTime;
        clickPromptPulse += deltaTime;
        backgroundPulse += deltaTime;
        rayRotation += deltaTime * 30f;
        cardGlowPulse += deltaTime * 2f;

        if (canSkip && Input.IsKeyPressed(KeyboardKey.Space))
        {
            SkipToReveal();
        }

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
            case PackOpeningPhase.PackAppear:
                UpdatePackAppear(deltaTime);
                break;
            case PackOpeningPhase.PackShake:
                UpdatePackShake(deltaTime);
                break;
            case PackOpeningPhase.PackOpen:
                UpdatePackOpen(deltaTime);
                break;
            case PackOpeningPhase.RarityRoulette:
                UpdateRarityRoulette(deltaTime);
                break;
            case PackOpeningPhase.SeedReveal:
                UpdateSeedReveal(deltaTime);
                break;
            case PackOpeningPhase.WaitingForClick:
                UpdateWaitingForClick(deltaTime);
                break;
        }

        if (visualSeed != null)
        {
            visualSeed.Update();
        }

        UpdateParticles(deltaTime);
        UpdateStarParticles(deltaTime);
        UpdateEnergyWaves(deltaTime);

        if (screenShake > 0)
        {
            screenShake = Math.Max(0, screenShake - deltaTime * 10f);
        }
    }

    private void UpdatePackAppear(float deltaTime)
    {
        float progress = phaseTimer / APPEAR_DURATION;
        packScale = EaseOutBack(Math.Min(1f, progress));

        if (phaseTimer >= APPEAR_DURATION)
        {
            currentPhase = PackOpeningPhase.PackShake;
            phaseTimer = 0f;
            packScale = 1f;
        }
    }

    private void UpdatePackShake(float deltaTime)
    {
        float progress = phaseTimer / SHAKE_DURATION;
        shakeIntensity = MathF.Sin(progress * MathF.PI * 12f) * (1f - progress) * 15f;

        packOffset = new Vector2(
            MathF.Sin(phaseTimer * 30f) * shakeIntensity,
            MathF.Cos(phaseTimer * 25f) * shakeIntensity * 0.5f
        );

        if (resultSeed.rarity >= SeedRarity.Raro && RandomHelper.Chance(0.1f))
        {
            SpawnAnticipationParticles();
        }

        if (phaseTimer >= SHAKE_DURATION)
        {
            currentPhase = PackOpeningPhase.PackOpen;
            phaseTimer = 0f;
            shakeIntensity = 0f;
            packOffset = Vector2.Zero;

            SpawnExplosionParticles();
            SpawnStarBurst();

            if (resultSeed.rarity >= SeedRarity.Epico)
            {
                screenShake = 5f;
            }
        }
    }

    int chosenRarity = 0;
    private void UpdatePackOpen(float deltaTime)
    {
        float progress = phaseTimer / OPEN_DURATION;

        packRotation = EaseInCubic(progress) * 45f;
        packScale = 1f + EaseOutCubic(progress) * 0.3f;

        float splitOffset = EaseOutCubic(progress) * 100f;

        if (phaseTimer >= OPEN_DURATION)
        {
            currentPhase = PackOpeningPhase.RarityRoulette;
            chosenRarity = Random.Shared.Next(10000);
            Console.WriteLine(chosenRarity);
            phaseTimer = 0f;
            rouletteSpeed = 30f; 
            currentRarityIndex = 0f;

            for (int i = 0; i < 3; i++)
            {
                energyWaves.Add(new EnergyWave
                {
                    Radius = i * 30f,
                    MaxRadius = 300f,
                    Alpha = 1f,
                    Color = rarityColors[SeedRarity.Comune]
                });
            }
        }
    }

    private void UpdateRarityRoulette(float deltaTime)
    {
        float totalDuration = ROULETTE_SPIN_DURATION + ROULETTE_SLOW_DURATION + ROULETTE_STOP_DURATION;
        float virtualIndex = currentRarityIndex;

        if (phaseTimer == 0) isStopPhaseInitialized = false;

        if (phaseTimer < ROULETTE_SPIN_DURATION)
        {
            rouletteSpeed = 30f;
            rouletteGlowIntensity = 0.6f;
            virtualIndex += rouletteSpeed * deltaTime;
        }
        else if (phaseTimer < ROULETTE_SPIN_DURATION + ROULETTE_SLOW_DURATION)
        {
            float slowProgress = (phaseTimer - ROULETTE_SPIN_DURATION) / ROULETTE_SLOW_DURATION;
            rouletteSpeed = MathHelper.Lerp(30f, 10f, EaseInCubic(slowProgress));
            rouletteGlowIntensity = 0.8f + MathF.Sin(phaseTimer * 10f) * 0.2f;
            virtualIndex += rouletteSpeed * deltaTime;
        }
        else if (phaseTimer < totalDuration)
        {
            if (!isStopPhaseInitialized)
            {
                isStopPhaseInitialized = true;
                stopStartRarityIndex = currentRarityIndex;

                int targetIndexRaw = GetRarityIndex(resultSeed.rarity);
                float currentMod = currentRarityIndex % raritySequence.Length;
                float distanceToTarget = targetIndexRaw - currentMod;

                if (distanceToTarget <= 0) distanceToTarget += raritySequence.Length;

                distanceToTarget += raritySequence.Length * 2.0f;
                stopTargetRarityIndex = currentRarityIndex + distanceToTarget;
            }

            float stopProgress = (phaseTimer - ROULETTE_SPIN_DURATION - ROULETTE_SLOW_DURATION) / ROULETTE_STOP_DURATION;
            float t = EaseOutCubic(stopProgress);

            float prevVirtual = virtualIndex;
            virtualIndex = MathHelper.Lerp(stopStartRarityIndex, stopTargetRarityIndex, t);

            if (deltaTime > 0) rouletteSpeed = (virtualIndex - prevVirtual) / deltaTime;
            rouletteGlowIntensity = 1f;
        }
        else
        {
            currentRarityIndex = GetRarityIndex(resultSeed.rarity);
            currentRouletteColor = rarityColors[resultSeed.rarity]; 
            isStopPhaseInitialized = false;
            currentPhase = PackOpeningPhase.SeedReveal;
            phaseTimer = 0f;
            rouletteSpeed = 0f;

            SpawnExplosionParticles();
            if (resultSeed.rarity >= SeedRarity.Epico) { SpawnStarBurst(); screenShake = 5f; }

            for (int i = 0; i < energyWaves.Count; i++)
            {
                var wave = energyWaves[i];
                wave.Color = rarityColors[resultSeed.rarity];
                energyWaves[i] = wave;
            }
            return;
        }

        currentRarityIndex = virtualIndex;

        int displayIndex = (int)MathF.Round(currentRarityIndex) % raritySequence.Length;
        currentRouletteColor = rarityColors[raritySequence[displayIndex]];

        if (rouletteSpeed > 5f && RandomHelper.Chance(0.2f)) SpawnRouletteParticles();
    }

    private int GetRarityIndex(SeedRarity rarity)
    {
        for (int i = 0; i < raritySequence.Length; i++)
        {
            if (raritySequence[i] == rarity)
                return i;
        }
        return 0;
    }

    private void SpawnRouletteParticles()
    {
        Vector2 center = packPosition;

        for (int i = 0; i < 2 && particles.Count < MAX_PARTICLES; i++)
        {
            float angle = RandomHelper.Float(0, MathF.PI * 2);

            particles.Add(new RevealParticle
            {
                Position = center + RandomHelper.InsideCircle(60f),
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * RandomHelper.Float(30f, 80f),
                Color = currentRouletteColor,
                Life = RandomHelper.Float(0.3f, 0.6f),
                MaxLife = 0.6f,
                Size = RandomHelper.Float(2f, 4f),
                Rotation = RandomHelper.Float(0, 360f),
                RotationSpeed = RandomHelper.Float(-150f, 150f)
            });
        }
    }

    private void UpdateSeedReveal(float deltaTime)
    {
        float progress = phaseTimer / revealDuration;

        cardRevealProgress = EaseOutBack(Math.Min(1f, progress * 1.2f));

        cardRotation = (1f - EaseOutCubic(Math.Min(1f, progress * 1.5f))) * 180f;

        revealScale = EaseOutBack(Math.Min(1f, progress * 1.5f));

        revealGlowIntensity = MathF.Sin(progress * MathF.PI);

        if (resultSeed.rarity >= SeedRarity.Raro)
        {
            if (RandomHelper.Chance(0.3f))
            {
                SpawnRevealParticles(2);
            }

            if (resultSeed.rarity == SeedRarity.Leggendario && RandomHelper.Chance(0.2f))
            {
                SpawnStarParticles();
            }
        }

        if (phaseTimer >= revealDuration)
        {
            currentPhase = PackOpeningPhase.WaitingForClick;
            phaseTimer = 0f;
            clickPromptVisible = true;
            revealScale = 1f;
            cardRevealProgress = 1f;
            cardRotation = 0f;
        }
    }

    private void UpdateWaitingForClick(float deltaTime)
    {
        float floatOffset = MathF.Sin(phaseTimer * 2f) * 5f;

        if (resultSeed.rarity == SeedRarity.Leggendario && RandomHelper.Chance(0.1f))
        {
            SpawnStarParticles();
        }

        if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyboardKey.Space))
        {
            CloseAnimation();
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
            p.Velocity.Y += 150f * deltaTime; 
            p.Velocity *= 0.98f;
            p.Rotation += p.RotationSpeed * deltaTime;

            particles[i] = p;
        }
    }

    private void UpdateStarParticles(float deltaTime)
    {
        for (int i = starParticles.Count - 1; i >= 0; i--)
        {
            var p = starParticles[i];
            p.Life -= deltaTime;
            p.TwinklePhase += deltaTime * 8f;

            if (p.Life <= 0)
            {
                starParticles.RemoveAt(i);
                continue;
            }

            starParticles[i] = p;
        }
    }

    private void UpdateEnergyWaves(float deltaTime)
    {
        for (int i = energyWaves.Count - 1; i >= 0; i--)
        {
            var wave = energyWaves[i];
            wave.Radius += deltaTime * 200f;
            wave.Alpha = Math.Max(0, wave.Alpha - deltaTime * 1.5f);

            if (wave.Radius >= wave.MaxRadius || wave.Alpha <= 0)
            {
                energyWaves.RemoveAt(i);
                continue;
            }

            energyWaves[i] = wave;
        }
    }

    private void SpawnAnticipationParticles()
    {
        Vector2 center = packPosition;
        Color rarityColor = rarityColors[resultSeed.rarity];

        for (int i = 0; i < 3 && particles.Count < MAX_PARTICLES; i++)
        {
            float angle = RandomHelper.Float(0, MathF.PI * 2);

            particles.Add(new RevealParticle
            {
                Position = center + RandomHelper.InsideCircle(40f),
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * RandomHelper.Float(20f, 50f),
                Color = rarityColor,
                Life = RandomHelper.Float(0.3f, 0.6f),
                MaxLife = 0.6f,
                Size = RandomHelper.Float(2f, 4f),
                Rotation = RandomHelper.Float(0, 360f),
                RotationSpeed = RandomHelper.Float(-100f, 100f)
            });
        }
    }

    private void SpawnExplosionParticles()
    {
        Vector2 center = packPosition;
        Color rarityColor = rarityColors[resultSeed.rarity];
        int particleCount = GetParticleCountForRarity() * 2;

        for (int i = 0; i < particleCount && particles.Count < MAX_PARTICLES; i++)
        {
            float angle = RandomHelper.Float(0, MathF.PI * 2);
            float speed = RandomHelper.Float(200f, 500f);

            particles.Add(new RevealParticle
            {
                Position = center,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = new Color(
                    (byte)Math.Min(255, rarityColor.R + RandomHelper.Int(-30, 30)),
                    (byte)Math.Min(255, rarityColor.G + RandomHelper.Int(-30, 30)),
                    (byte)Math.Min(255, rarityColor.B + RandomHelper.Int(-30, 30)),
                    255
                ),
                Life = RandomHelper.Float(0.8f, 1.5f),
                MaxLife = 1.5f,
                Size = RandomHelper.Float(3f, 8f),
                Rotation = RandomHelper.Float(0, 360f),
                RotationSpeed = RandomHelper.Float(-300f, 300f)
            });
        }
    }

    private void SpawnRevealParticles(int count = 5)
    {
        Vector2 center = packPosition;
        Color rarityColor = rarityColors[resultSeed.rarity];

        for (int i = 0; i < count && particles.Count < MAX_PARTICLES; i++)
        {
            float angle = RandomHelper.Float(0, MathF.PI * 2);
            float speed = RandomHelper.Float(50f, 150f);

            particles.Add(new RevealParticle
            {
                Position = center + RandomHelper.InsideCircle(30f),
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = rarityColor,
                Life = RandomHelper.Float(0.5f, 1.2f),
                MaxLife = 1.2f,
                Size = RandomHelper.Float(2f, 5f),
                Rotation = RandomHelper.Float(0, 360f),
                RotationSpeed = RandomHelper.Float(-200f, 200f)
            });
        }
    }

    private void SpawnStarParticles()
    {
        Vector2 center = packPosition;

        for (int i = 0; i < 5 && starParticles.Count < 50; i++)
        {
            starParticles.Add(new StarParticle
            {
                Position = center + RandomHelper.InsideCircle(100f),
                Life = RandomHelper.Float(1f, 2f),
                MaxLife = 2f,
                Size = RandomHelper.Float(3f, 8f),
                Color = new Color(255, 255, 200, 255),
                TwinklePhase = RandomHelper.Float(0, MathF.PI * 2)
            });
        }
    }

    private void SpawnStarBurst()
    {
        if (resultSeed.rarity < SeedRarity.Epico) return;

        Vector2 center = packPosition;
        int starCount = resultSeed.rarity == SeedRarity.Leggendario ? 12 : 8;

        for (int i = 0; i < starCount; i++)
        {
            float angle = (i / (float)starCount) * MathF.PI * 2;

            starParticles.Add(new StarParticle
            {
                Position = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 50f,
                Life = 1.5f,
                MaxLife = 1.5f,
                Size = 6f,
                Color = Color.White,
                TwinklePhase = 0f
            });
        }
    }

    private int GetParticleCountForRarity()
    {
        return resultSeed.rarity switch
        {
            SeedRarity.Comune => 15,
            SeedRarity.NonComune => 25,
            SeedRarity.Raro => 40,
            SeedRarity.Epico => 60,
            SeedRarity.Leggendario => 80,
            _ => 15
        };
    }

    private void SkipToReveal()
    {
        currentPhase = PackOpeningPhase.SeedReveal;
        phaseTimer = revealDuration - 0.5f;
        currentRarityIndex = GetRarityIndex(resultSeed.rarity);
        currentRouletteColor = rarityColors[resultSeed.rarity];
        SpawnExplosionParticles();
        SpawnStarBurst();
    }

    private void CloseAnimation()
    {
        currentPhase = PackOpeningPhase.Idle;
        this.active = false;
        particles.Clear();
        starParticles.Clear();
        energyWaves.Clear();
        visualSeed = null;
    }

    public override void Draw()
    {
        if (currentPhase == PackOpeningPhase.Idle) return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        Vector2 shakeOffset = Vector2.Zero;
        if (screenShake > 0)
        {
            shakeOffset = new Vector2(
                RandomHelper.Float(-screenShake, screenShake),
                RandomHelper.Float(-screenShake, screenShake)
            );
        }

        byte bgAlpha = (byte)(180 + MathF.Sin(backgroundPulse) * 30);
        Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, bgAlpha));

        DrawEnergyWaves(shakeOffset);

        if (currentPhase == PackOpeningPhase.PackAppear ||
            currentPhase == PackOpeningPhase.PackShake ||
            currentPhase == PackOpeningPhase.PackOpen)
        {
            DrawPack(shakeOffset);
        }

        if (currentPhase == PackOpeningPhase.RarityRoulette)
        {
            DrawRarityRoulette(shakeOffset);
        }

        if (currentPhase == PackOpeningPhase.SeedReveal ||
            currentPhase == PackOpeningPhase.WaitingForClick)
        {
            DrawCardReveal(shakeOffset);
        }

        DrawStarParticles(shakeOffset);
        DrawParticles(shakeOffset);

        if (canSkip && currentPhase != PackOpeningPhase.WaitingForClick)
        {
            DrawSkipButton();
        }

        if (clickPromptVisible)
        {
            DrawClickPrompt();
        }
    }

    private void DrawPack(Vector2 shakeOffset)
    {
        Vector2 pos = packPosition + packOffset + shakeOffset;
        Color rarityColor = rarityPackageColors[packageRarity];

        if (currentPhase == PackOpeningPhase.PackShake)
        {
            float glowSize = 120f + MathF.Sin(phaseTimer * 15f) * 20f;
            byte glowAlpha = (byte)(100 + MathF.Sin(phaseTimer * 15f) * 50);

            Graphics.DrawCircleGradient(
                (int)pos.X, (int)pos.Y, glowSize,
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, glowAlpha),
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, 0)
            );
        }

        int packSize = (int)(80 * packScale);

        Vector2 center = pos;

        Graphics.DrawRectangle(
            (int)(center.X - packSize / 2 + 5),
            (int)(center.Y - packSize / 2 + 5),
            packSize, packSize,
            new Color(0, 0, 0, 100)
        );

        Color packColor = new Color(
            (byte)(rarityColor.R * 0.7f),
            (byte)(rarityColor.G * 0.7f),
            (byte)(rarityColor.B * 0.7f),
            255
        );

        Graphics.DrawRectangle(
            (int)(center.X - packSize / 2),
            (int)(center.Y - packSize / 2),
            packSize, packSize,
            packColor
        );

        Graphics.DrawRectangleLines(
            (int)(center.X - packSize / 2),
            (int)(center.Y - packSize / 2),
            packSize, packSize,
            rarityColor
        );

        Graphics.DrawRectangle(
            (int)(center.X - packSize / 2 + 5),
            (int)(center.Y - packSize / 2 + 5),
            packSize - 10, 15,
            new Color(255, 255, 255, 80)
        );
    }

    private void DrawRarityRoulette(Vector2 shakeOffset)
    {
        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;
        Vector2 center = new Vector2(screenW / 2, screenH / 2) + shakeOffset;

        int displayWidth = 280;
        int displayHeight = 100;
        int rarityBoxSize = 70;
        int spacing = rarityBoxSize + 10;

        float glowSize = 150f + MathF.Sin(phaseTimer * 8f) * 20f;
        byte glowAlpha = (byte)(rouletteGlowIntensity * 120);

        Graphics.DrawCircleGradient(
            (int)center.X, (int)center.Y, glowSize,
            new Color(currentRouletteColor.R, currentRouletteColor.G, currentRouletteColor.B, glowAlpha),
            new Color(currentRouletteColor.R, currentRouletteColor.G, currentRouletteColor.B, 0)
        );

        Graphics.DrawRectangleRounded(
            new Rectangle(center.X - displayWidth / 2, center.Y - displayHeight / 2, displayWidth, displayHeight),
            0.15f, 8, new Color(20, 20, 30, 240)
        );

        Graphics.BeginScissorMode(
            (int)(center.X - displayWidth / 2),
            (int)(center.Y - displayHeight / 2),
            displayWidth,
            displayHeight
        );

        for (int i = -2; i <= 2; i++)
        {
            float scrollOffset = (currentRarityIndex - MathF.Floor(currentRarityIndex)) * spacing;
            int index = ((int)MathF.Floor(currentRarityIndex) + i + (raritySequence.Length * 100)) % raritySequence.Length;

            SeedRarity rarity = raritySequence[index];
            Color color = rarityColors[rarity];

            int xPos = (int)(center.X - rarityBoxSize / 2 + (i * spacing) - scrollOffset);
            int yPos = (int)(center.Y - rarityBoxSize / 2);


            float distanceFromCenter = Math.Abs((i * spacing) - scrollOffset);
            float scale = 1f - (distanceFromCenter / (spacing * 1.5f)) * 0.4f;
            byte alpha = (byte)Math.Clamp(255 - (distanceFromCenter / spacing) * 180, 0, 255);

            int scaledSize = (int)(rarityBoxSize * scale);
            int scaledX = (int)(xPos + (rarityBoxSize - scaledSize) / 2);
            int scaledY = (int)(yPos + (rarityBoxSize - scaledSize) / 2);

            Graphics.DrawRectangleRounded(
                new Rectangle(scaledX, scaledY, scaledSize, scaledSize),
                0.2f, 6, new Color((byte)(color.R * 0.3f), (byte)(color.G * 0.3f), (byte)(color.B * 0.3f), alpha)
            );

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(scaledX, scaledY, scaledSize, scaledSize),
                0.2f, 6, 3, new Color(color.R, color.G, color.B, alpha)
            );

            if (visualSeed != null && alpha > 50)
            {
                var possibleTypes = Enum.GetValues<SeedType>()
                .Cast<SeedType>()
                .Where(t => new Seed(t).rarity == rarity)
                .ToList();

                SeedType chosenType = possibleTypes[chosenRarity % possibleTypes.Count];                

                Vector2 boxCenter = new Vector2(scaledX + scaledSize / 2, scaledY + scaledSize / 2);

                visualSeed.position = boxCenter;
                visualSeed.scale = 3.0f * scale;

                float totalDuration = ROULETTE_SPIN_DURATION + ROULETTE_SLOW_DURATION + ROULETTE_STOP_DURATION;

                if (phaseTimer >= totalDuration)
                    visualSeed.color = resultSeed.color;
                else
                    visualSeed.color = new Seed(chosenType).color;
				visualSeed.Draw();
            }
        }

        Graphics.EndScissorMode();

        float borderPulse = (MathF.Sin(phaseTimer * 12f) + 1f) * 0.5f;
        byte borderAlpha = (byte)(150 + borderPulse * 105);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(center.X - displayWidth / 2, center.Y - displayHeight / 2, displayWidth, displayHeight),
            0.15f, 8, 4, new Color(currentRouletteColor.R, currentRouletteColor.G, currentRouletteColor.B, borderAlpha)
        );

        int indicatorSize = 12;
        Vector2 indicatorTop = center + new Vector2(0, -displayHeight / 2 + 5);
        Graphics.DrawTriangle(
            indicatorTop,
            indicatorTop + new Vector2(-indicatorSize, -indicatorSize),
            indicatorTop + new Vector2(indicatorSize, -indicatorSize),
            currentRouletteColor
        );
    }


    private void DrawCardReveal(Vector2 shakeOffset)
    {
        Vector2 center = packPosition + shakeOffset;
        Color rarityColor = rarityColors[resultSeed.rarity];

        if (resultSeed.rarity >= SeedRarity.Epico)
        {
            DrawRays(center, rarityColor);
        }

        int cardWidth = (int)(200 * cardRevealProgress);
        int cardHeight = (int)(280 * cardRevealProgress);

        if (cardWidth > 20 && cardHeight > 20)
        {

            float glowPulse = (MathF.Sin(cardGlowPulse) + 1f) * 0.5f;
            byte glowAlpha = (byte)(revealGlowIntensity * 150 * glowPulse);
            Graphics.DrawCircleGradient(
                (int)center.X, (int)center.Y, 150f,
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, glowAlpha),
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, 0)
            );

            Graphics.DrawRectangleRounded(
                new Rectangle(
                    center.X - cardWidth / 2,
                    center.Y - cardHeight / 2,
                    cardWidth, cardHeight
                ),
                0.1f, 8,
                new Color(40, 40, 50, 240)
            );

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(
                    center.X - cardWidth / 2,
                    center.Y - cardHeight / 2,
                    cardWidth, cardHeight
                ),
                0.1f, 8, 4,
                rarityColor
            );

            Graphics.DrawRectangleRounded(
                new Rectangle(
                    center.X - cardWidth / 2 + 10,
                    center.Y - cardHeight / 2 + 10,
                    cardWidth - 20, 30
                ),
                0.2f, 6,
                new Color(rarityColor.R, rarityColor.G, rarityColor.B, 200)
            );

            if (revealScale > 0.5f)
            {
                string rarityText = resultSeed.rarity.ToString();
                int textWidth = rarityText.Length * 7;
                Graphics.DrawText(
                    rarityText,
                    (int)(center.X - textWidth / 2),
                    (int)(center.Y - cardHeight / 2 + 18),
                    14, Color.White
                );
            }
        }

        if (visualSeed != null && revealScale > 0.3f)
        {
            visualSeed.scale = 8f * revealScale;
            visualSeed.position = center;
            visualSeed.color = resultSeed.color;
            visualSeed.Draw();
        }

        if (revealScale > 0.7f)
        {
            string seedName = SeedDataType.GetName(resultSeed.type);
            int nameWidth = seedName.Length * 8;
            Graphics.DrawText(
                seedName,
                (int)(center.X - nameWidth / 2),
                (int)(center.Y + 100),
                16, Color.White
            );
        }
    }

    private void DrawRays(Vector2 center, Color color)
    {
        int rayCount = resultSeed.rarity == SeedRarity.Leggendario ? 16 : 12;
        float rayLength = 180f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (rayRotation + i * (360f / rayCount)) * MathF.PI / 180f;
            Vector2 end = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * rayLength;

            byte alpha = (byte)(revealGlowIntensity * 180f);
            Color rayColor = new Color(color.R, color.G, color.B, alpha);
            Graphics.DrawLineEx(center, end, 4f, rayColor);

            Vector2 innerEnd = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (rayLength * 0.6f);
            Graphics.DrawLineEx(center, innerEnd, 2f, new Color(255, 255, 255, (byte)(alpha * 0.7f)));
        }
    }

    private void DrawEnergyWaves(Vector2 shakeOffset)
    {
        Vector2 center = packPosition + shakeOffset;

        foreach (var wave in energyWaves)
        {
            byte alpha = (byte)(wave.Alpha * 200);
            Color waveColor = new Color(wave.Color.R, wave.Color.G, wave.Color.B, alpha);

            Graphics.DrawCircleLines((int)center.X, (int)center.Y, wave.Radius, waveColor);
            Graphics.DrawCircleLines((int)center.X, (int)center.Y, wave.Radius + 2, waveColor);
        }
    }

    private void DrawParticles(Vector2 shakeOffset)
    {
        foreach (var p in particles)
        {
            float alpha = p.Life / p.MaxLife;
            Color col = new Color(
                p.Color.R, p.Color.G, p.Color.B,
                (byte)(p.Color.A * alpha)
            );

            Vector2 pos = p.Position + shakeOffset;

            float halfSize = p.Size / 2f;
            float cos = MathF.Cos(p.Rotation * MathF.PI / 180f);
            float sin = MathF.Sin(p.Rotation * MathF.PI / 180f);

            Vector2[] points = new Vector2[4];
            points[0] = pos + new Vector2(-halfSize * cos + halfSize * sin, -halfSize * sin - halfSize * cos);
            points[1] = pos + new Vector2(halfSize * cos + halfSize * sin, halfSize * sin - halfSize * cos);
            points[2] = pos + new Vector2(halfSize * cos - halfSize * sin, halfSize * sin + halfSize * cos);
            points[3] = pos + new Vector2(-halfSize * cos - halfSize * sin, -halfSize * sin + halfSize * cos);

            Graphics.DrawTriangle(points[0], points[1], points[2], col);
            Graphics.DrawTriangle(points[0], points[2], points[3], col);
        }
    }

    private void DrawStarParticles(Vector2 shakeOffset)
    {
        foreach (var star in starParticles)
        {
            float alpha = (star.Life / star.MaxLife) * (MathF.Sin(star.TwinklePhase) * 0.3f + 0.7f);
            byte a = (byte)(alpha * 255);
            Color col = new Color(star.Color.R, star.Color.G, star.Color.B, a);

            Vector2 pos = star.Position + shakeOffset;

            float size = star.Size;
            Graphics.DrawLineEx(
                pos + new Vector2(0, -size),
                pos + new Vector2(0, size),
                2f, col
            );
            Graphics.DrawLineEx(
                pos + new Vector2(-size, 0),
                pos + new Vector2(size, 0),
                2f, col
            );

            Graphics.DrawCircle((int)pos.X, (int)pos.Y, size * 0.3f, col);
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

    private float EaseInCubic(float x)
    {
        return x * x * x;
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }
}
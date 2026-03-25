using System;

namespace Plants;

/// <summary>
/// Sistema centralizzato per il recupero del seme.
/// Fase 1 (Countdown): il giocatore deve sopravvivere per la durata. La pianta cresce e prende danni normalmente.
/// Fase 2 (Rewind): la pianta smette di crescere/prendere danni e si riavvolge visivamente.
/// </summary>
public static class SeedRecoverySystem
{
    public static bool IsConfirming { get; set; } = false;
    public static bool IsCountdown { get; private set; } = false;
    public static bool IsRewinding { get; private set; } = false;
    public static bool IsRecovering => IsCountdown || IsRewinding;

    // Seme da recuperare
    private static SeedType recoveredSeedType;
    private static SeedStats recoveredSeedStats;
    private static SeedRarity recoveredSeedRarity;

    // Countdown (fase sopravvivenza)
    private static float countdownDuration;
    private static float countdownElapsed;

    // Rewind (fase visiva)
    private static float rewindDuration;
    private static float rewindElapsed;
    private static float rewindTimePerStep;
    private static float rewindStepAccumulator;
    private static int initialSplineCount;
    private static int initialBranchCount;
    private static int initialRootCount;
    private static int initialLeafCount;

    // Progresso
    public static float CountdownProgress => countdownDuration > 0 ? Math.Clamp(countdownElapsed / countdownDuration, 0f, 1f) : 0f;
    public static float CountdownRemaining => Math.Max(0, countdownDuration - countdownElapsed);
    public static float RewindProgress => rewindDuration > 0 ? Math.Clamp(rewindElapsed / rewindDuration, 0f, 1f) : 0f;

    private const float REWIND_DURATION = 8f;

    /// <summary>
    /// Durata del countdown in base alla rarita' (piu' raro = meno tempo da sopravvivere)
    /// </summary>
    public static float GetDuration(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune => 300f,       // 5 min
        SeedRarity.NonComune => 270f,    // 4:30
        SeedRarity.Raro => 240f,         // 4 min
        SeedRarity.Esotico => 210f,      // 3:30
        SeedRarity.Epico => 180f,        // 3 min
        SeedRarity.Leggendario => 150f,  // 2:30
        SeedRarity.Mitico => 120f,       // 2 min
        _ => 300f
    };

    /// <summary>
    /// Avvia il countdown di sopravvivenza
    /// </summary>
    public static void StartRecovery()
    {
        var pianta = Game.pianta;
        if (pianta == null || IsRecovering) return;

        // Cattura i dati del seme
        recoveredSeedType = pianta.TipoSeme;
        recoveredSeedStats = pianta.seedBonus;
        recoveredSeedRarity = Seed.GetRarityFromType(recoveredSeedType);

        countdownDuration = GetDuration(recoveredSeedRarity);
        countdownElapsed = 0f;

        IsCountdown = true;
        IsConfirming = false;

        Console.WriteLine($"Countdown recupero avviato: {recoveredSeedType} ({recoveredSeedRarity}) - Sopravvivi per {countdownDuration}s");
    }

    /// <summary>
    /// Aggiorna il sistema (chiamato ogni frame)
    /// </summary>
    public static void Update(float dt)
    {
        if (IsCountdown)
            UpdateCountdown(dt);
        else if (IsRewinding)
            UpdateRewind(dt);
    }

    private static void UpdateCountdown(float dt)
    {
        // Pianta morta durante il countdown = recupero fallito
        if (Game.guiMorte != null && Game.guiMorte.active)
        {
            CancelRecovery();
            return;
        }

        countdownElapsed += dt;

        // Countdown completato: avvia il rewind visivo
        if (countdownElapsed >= countdownDuration)
        {
            StartRewind();
        }
    }

    /// <summary>
    /// Avvia la fase di rewind visivo (pianta non puo' piu' prendere danni)
    /// </summary>
    private static void StartRewind()
    {
        IsCountdown = false;
        IsRewinding = true;

        var pianta = Game.pianta;
        initialSplineCount = pianta.GetSplineCount();
        initialBranchCount = pianta.GetBranchCount();
        initialRootCount = pianta.GetRootCount();
        initialLeafCount = pianta.Stats.FoglieAttuali;

        rewindDuration = REWIND_DURATION;
        rewindElapsed = 0f;
        int totalSteps = Math.Max(1, initialSplineCount - 3);
        rewindTimePerStep = rewindDuration / totalSteps;
        rewindStepAccumulator = 0f;

        Console.WriteLine($"Rewind avviato: {initialSplineCount} punti in {rewindDuration}s");
    }

    private static void UpdateRewind(float dt)
    {
        rewindElapsed += dt;
        rewindStepAccumulator += dt;

        // Rimuovi punti spline in base al tempo
        while (rewindStepAccumulator >= rewindTimePerStep && Game.pianta.GetSplineCount() > 3)
        {
            rewindStepAccumulator -= rewindTimePerStep;
            Game.pianta.RewindStep(initialSplineCount, initialBranchCount, initialRootCount, initialLeafCount);
        }

        if (Game.pianta.GetSplineCount() <= 3 || rewindElapsed >= rewindDuration)
        {
            CompleteRecovery();
        }
    }

    private static void CompleteRecovery()
    {
        IsRewinding = false;

        // Ricostruisci il seme con le stats originali
        var seed = new Seed();
        seed.type = recoveredSeedType;
        seed.stats = recoveredSeedStats;

        Inventario.get().AddSeed(seed);

        Console.WriteLine($"Seme recuperato: {seed.name} restituito all'inventario");

        GameSave.DeleteSaveFile();

        WorldManager.SetCurrentWorld(WorldType.Terra);
        Game.pianta.SetNaturalColors(WorldType.Terra);
        Rendering.camera.position.Y = 0;
        Game.controller.targetScrollY = 0;

        Game.EntraModalitaPiantaggio();
    }

    /// <summary>
    /// Annulla il recupero (pianta morta o annullamento manuale)
    /// </summary>
    public static void CancelRecovery()
    {
        IsCountdown = false;
        IsRewinding = false;
        Console.WriteLine("Recupero seme annullato");
    }
}

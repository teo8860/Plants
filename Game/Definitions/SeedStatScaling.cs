using System;

namespace Plants;

/// <summary>
/// Scala delle statistiche dei semi (0-99) e conversione ai moltiplicatori di gioco.
/// Le statistiche primarie (vitalita, idratazione, metabolismo, vegetazione) sono
/// "moltiplicatori" rispetto al riferimento <see cref="PrimaryNeutralReference"/>.
/// Le resistenze (freddo, caldo, parassiti, vuoto) sono frazioni rispetto a
/// <see cref="ResistanceFullReference"/>.
/// Lo stage corrente (loop mondi) rende i valori meno efficaci man mano che si sale.
/// </summary>
public static class SeedStatScaling
{
    public const float StatMin = 0f;
    public const float StatMax = 99f;

    public const float PrimaryNeutralReference = 10f;
    public const float ResistanceFullReference = 100f;

    /// <summary>Quanto la difficolta' stage richiede stat piu' alte.</summary>
    public const float PrimaryStageScale = 0.7f;
    public const float ResistanceStageScale = 1.0f;

    public const float VitalitaMin   = 5f;
    public const float IdratazioneMin = 3f;
    public const float MetabolismoMin = 5f;
    public const float VegetazioneMin = 5f;

    public const float ResistanceEffectiveCap = 0.95f;

    public static float StagePrimaryRequirement(int stage)
        => PrimaryNeutralReference + Math.Max(0, stage - 1) * PrimaryStageScale;

    public static float StageResistanceRequirement(int stage)
        => ResistanceFullReference + Math.Max(0, stage - 1) * ResistanceStageScale;

    /// <summary>Stat primaria → moltiplicatore efficace (1.0 = neutro al suo stage di riferimento).</summary>
    public static float EffectiveMultiplier(float stat, int stage)
        => stat / StagePrimaryRequirement(stage);

    /// <summary>Stat di resistenza → frazione di resistenza [0, 0.95].</summary>
    public static float EffectiveResistance(float stat, int stage)
        => Math.Clamp(stat / StageResistanceRequirement(stage), 0f, ResistanceEffectiveCap);

    public static float ClampPrimary(float value, float minValue)
        => Math.Clamp(value, minValue, StatMax);

    public static float ClampResistance(float value)
        => Math.Clamp(value, StatMin, StatMax);

    /// <summary>
    /// Euristica: le stats in vecchio formato (scala ~0-2.5 primarie, 0-1 resistenze)
    /// hanno vitalita < 3. Il nuovo baseline e' 10 (clampato a 5 minimo).
    /// </summary>
    public static bool NeedsLegacyMigration(SeedStats s)
        => s != null && s.vitalita > 0f && s.vitalita < 3f;

    /// <summary>
    /// Migra stats dal vecchio formato (primarie ×10, resistenze ×100) al nuovo 0-99.
    /// Idempotente: controlla <see cref="NeedsLegacyMigration"/> prima di applicare.
    /// </summary>
    public static void MigrateLegacyStats(SeedStats s)
    {
        if (!NeedsLegacyMigration(s)) return;
        s.vitalita    = ClampPrimary(s.vitalita    * 10f, VitalitaMin);
        s.idratazione = ClampPrimary(s.idratazione * 10f, IdratazioneMin);
        s.metabolismo = ClampPrimary(s.metabolismo * 10f, MetabolismoMin);
        s.vegetazione = ClampPrimary(s.vegetazione * 10f, VegetazioneMin);
        s.resistenzaFreddo    = ClampResistance(s.resistenzaFreddo    * 100f);
        s.resistenzaCaldo     = ClampResistance(s.resistenzaCaldo     * 100f);
        s.resistenzaParassiti = ClampResistance(s.resistenzaParassiti * 100f);
        s.resistenzaVuoto     = ClampResistance(s.resistenzaVuoto     * 100f);
    }
}

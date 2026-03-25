using System;

namespace Plants;

public class OfflineSimulationResult
{
    public TimeSpan TimeSimulated { get; set; }
    public int TicksSimulated { get; set; }

    // Plant stats before/after
    public float HealthBefore { get; set; }
    public float HealthAfter { get; set; }
    public float HydrationBefore { get; set; }
    public float HydrationAfter { get; set; }
    public float HeightBefore { get; set; }
    public float HeightAfter { get; set; }
    public int LeavesBefore { get; set; }
    public int LeavesAfter { get; set; }

    // Events
    public int WeatherChanges { get; set; }
    public float WaterBefore { get; set; }
    public float WaterAfter { get; set; }

    public string GetSummary()
    {
        var s = "=== Simulazione Offline ===\n";
        s += $"Tempo offline: {TimeSimulated.Days}g {TimeSimulated.Hours}h {TimeSimulated.Minutes}m\n";
        s += $"Tick simulati: {TicksSimulated}\n";
        s += $"Salute: {HealthBefore:P0} -> {HealthAfter:P0}\n";
        s += $"Idratazione: {HydrationBefore:P0} -> {HydrationAfter:P0}\n";
        s += $"Altezza: {HeightBefore:F0} -> {HeightAfter:F0}\n";
        s += $"Foglie: {LeavesBefore} -> {LeavesAfter}\n";
        s += $"Acqua: {WaterBefore:F0} -> {WaterAfter:F0}\n";
        s += $"Cambi meteo: {WeatherChanges}\n";
        return s;
    }
}

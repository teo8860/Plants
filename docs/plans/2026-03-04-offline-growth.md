# Offline Plant Growth System - Implementation Plan

**Date:** 2026-03-04
**Goal:** Fix the existing buggy offline growth system to properly simulate time-based plant growth with accurate weather, day/night cycles, water mechanics, and death detection.

**Current bugs in `SimulateOfflineGrowth()`:**
1. Uses `FaseGiorno.GetCurrentPhase()` (real-time) instead of simulated phase
2. Uses `WeatherManager.GetCurrentWeather()` (real-time) instead of simulated weather
3. Uses `WorldManager.GetCurrentModifiers()` which randomizes `IsMeteoOn` every call
4. No water consumption simulation (only passive recharge)
5. No death check during simulation
6. No player feedback after simulation

**Architecture:** Replace `SimulateOfflineGrowth` with a proper tick-based simulator.
- **Environment tick = 10 real minutes** (resolves weather + day phase per tick)
- **Plant tick = 1 second** (600 sub-ticks per environment tick, matching online rate)
- **Simulation override system** lets managers return simulated values instead of real-time values, ensuring all internal code sees correct simulated state
- **No max offline time cap**

**Performance:** Worst case 30 days offline = 4320 env ticks * 600 sub-ticks = 2,592,000 AggiornaTutto calls. Each is pure math (~50μs), total ~130s. For typical offline (1-3 days) = <15s. Acceptable.

---

## Task 1: Fix WorldManager.GetCurrentModifiers() IsMeteoOn Bug

**Files:** `Game/Core/Mondo/WorldManager.cs`

**Problem:** Line ~395: `IsMeteoOn = RandomHelper.Int(0,2)==1` randomizes weather toggle every call. Should use the world's actual `IsMeteoOn` from the modifiers dictionary.

**Steps:**
1. In `GetCurrentModifiers()`, replace `IsMeteoOn = RandomHelper.Int(0,2)==1` with `IsMeteoOn = m.IsMeteoOn` (where `m` is the modifier from dictionary)
2. Build: `dotnet build`

**Verification:** Build succeeds. `GetCurrentModifiers()` returns stable `IsMeteoOn` value.

---

## Task 2: Add Simulation Override System

**Files:**
- `Game/Core/Game.cs` — add `IsOfflineSimulation` flag
- `Game/Core/Mondo/WorldManager.cs` — add override field
- `Game/Core/Mondo/FaseGiorno.cs` — add override field
- `Game/Core/Mondo/WeatherManager.cs` — add override field
- `Game/Core/Pianta/Obj_Plant.cs` — guard auto-scroll + isPaused
- `Game/Core/GameLogic.cs` — guard events + harvest UI

**Context:** During offline simulation, all code calling `GetCurrentPhase()`, `GetCurrentWeather()`, `GetCurrentModifiers()` must see simulated values. Internal code (ControlloCrescita, Crescita, AggiornaTutto) calls these methods directly, so we can't just pass parameters — we need global overrides.

### Step 1: Add `IsOfflineSimulation` to Game class

In `Game/Core/Game.cs`, add field near `isPaused` (line 30):
```csharp
public static bool IsOfflineSimulation = false;
```

### Step 2: Add override to WorldManager

In `Game/Core/Mondo/WorldManager.cs`, add at class level:
```csharp
private static WorldModifier? simulationOverride = null;

public static void SetSimulationOverride(WorldModifier? mod)
{
    simulationOverride = mod;
}
```

Modify `GetCurrentModifiers()` — add check at top:
```csharp
public static WorldModifier GetCurrentModifiers()
{
    if (simulationOverride.HasValue) return simulationOverride.Value;
    // ... existing code (with IsMeteoOn fix from Task 1)
}
```

### Step 3: Add override to FaseGiorno

In `Game/Core/Mondo/FaseGiorno.cs`, add at class level:
```csharp
private static DayPhase? phaseOverride = null;

public static void SetSimulationOverride(DayPhase? phase)
{
    phaseOverride = phase;
}
```

Modify `GetCurrentPhase()`:
```csharp
public static DayPhase GetCurrentPhase()
{
    if (phaseOverride.HasValue) return phaseOverride.Value;
    return GetPhaseFromTime(DateTime.Now);
}
```

### Step 4: Add override to WeatherManager

In `Game/Core/Mondo/WeatherManager.cs`, add at class level:
```csharp
private static Weather? weatherOverride = null;

public static void SetSimulationOverride(Weather? weather)
{
    weatherOverride = weather;
}
```

Modify `GetCurrentWeather()`:
```csharp
public static Weather GetCurrentWeather()
{
    if (weatherOverride.HasValue) return weatherOverride.Value;
    UpdateWeatherIfNeeded();
    return currentWeather;
}
```

### Step 5: Guard Obj_Plant during offline simulation

In `ControlloCrescita()` (line 234-236 of Obj_Plant.cs):
```csharp
// BEFORE: if (Game.isPaused) return;
// AFTER:
if (Game.isPaused && !Game.IsOfflineSimulation) return;
```

In `Crescita()` (line ~207-210), guard auto-scroll:
```csharp
// BEFORE:
if (Game.controller.targetScrollY <= Stats.AltezzaMassima * ... && Game.controller.autoscroll == true)
{
    Game.controller.targetScrollY += incrementoFinale;
}
// AFTER:
if (!Game.IsOfflineSimulation && Game.controller.targetScrollY <= Stats.AltezzaMassima * ... && Game.controller.autoscroll == true)
{
    Game.controller.targetScrollY += incrementoFinale;
}
```

### Step 6: Guard GameLogic during offline simulation

In `AggiornaTutto()` in `Game/Core/GameLogic.cs`:

Guard death harvest UI:
```csharp
// BEFORE:
if (!IsViva)
{
    if (stats.FoglieAttuali > 0)
    {
        LeafHarvestSystem.HarvestAndShow("Pianta morta");
    }
    return;
}
// AFTER:
if (!IsViva)
{
    if (stats.FoglieAttuali > 0 && !Game.IsOfflineSimulation)
    {
        LeafHarvestSystem.HarvestAndShow("Pianta morta");
    }
    return;
}
```

Guard events:
```csharp
// BEFORE:
EventSystem?.CheckAndFireEvents();
// AFTER:
if (!Game.IsOfflineSimulation) EventSystem?.CheckAndFireEvents();
```

### Step 7: Build and verify

Run: `dotnet build`
Expected: Build succeeds. All override methods compile.

---

## Task 3: Extract Weather Markov Transition to Public Method

**Files:** `Game/Core/Mondo/WeatherManager.cs`

**Context:** `ChangeWeatherRandomly()` is private. For offline simulation, we need to advance weather deterministically using a seeded Random. Extract the Markov transition logic.

### Step 1: Add public `GetNextWeather` method

Add to WeatherManager:
```csharp
/// <summary>
/// Markov chain weather transition. Pass a seeded Random for deterministic offline simulation.
/// </summary>
public static Weather GetNextWeather(Weather current, Random rng)
{
    int roll = rng.Next(100);
    return current switch
    {
        Weather.Sunny => roll < 60 ? Weather.Sunny : roll < 80 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Snowy : Weather.Foggy,
        Weather.Cloudy => roll < 40 ? Weather.Sunny : roll < 60 ? Weather.Cloudy : roll < 85 ? Weather.Rainy : roll < 95 ? Weather.Stormy : Weather.Foggy,
        Weather.Rainy => roll < 50 ? Weather.Rainy : roll < 75 ? Weather.Cloudy : roll < 90 ? Weather.Stormy : Weather.Snowy,
        Weather.Stormy => roll < 40 ? Weather.Stormy : roll < 80 ? Weather.Rainy : roll < 90 ? Weather.Snowy : Weather.Cloudy,
        Weather.Foggy => roll < 40 ? Weather.Foggy : roll < 70 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Stormy : Weather.Snowy,
        Weather.Snowy => roll < 50 ? Weather.Snowy : roll < 80 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : roll < 95 ? Weather.Foggy : Weather.Sunny,
        _ => Weather.Sunny
    };
}
```

NOTE: Copy exact transition percentages from existing `ChangeWeatherRandomly()`. The percentages above match the analysis but MUST be verified against actual code.

### Step 2: Add accessors for weather state

```csharp
public static DateTime GetLastWeatherChange() => lastWeatherChange;
public static void SetLastWeatherChange(DateTime time) { lastWeatherChange = time; }
public static void SetWeatherDirect(Weather weather) { currentWeather = weather; }
```

### Step 3: Refactor `ChangeWeatherRandomly()` to use `GetNextWeather`

```csharp
private static void ChangeWeatherRandomly()
{
    Weather newWeather = GetNextWeather(currentWeather, random);
    if (newWeather != currentWeather)
    {
        currentWeather = newWeather;
        lastWeatherChange = DateTime.Now;
    }
}
```

### Step 4: Build and verify

Run: `dotnet build`
Expected: Build succeeds. Weather transition behavior unchanged.

---

## Task 4: Save Weather Timing in GameSaveData

**Files:** `Game/Core/Dati/GameSave.cs`

**Context:** To simulate weather progression offline, we need to know when the last weather change happened. Currently only `CurrentWeather` is saved, not the timing.

### Step 1: Add field to GameSaveData

```csharp
public DateTime LastWeatherChange { get; set; }
```

### Step 2: Wire into Save()

After the existing `CurrentWeather` save:
```csharp
data.LastWeatherChange = WeatherManager.GetLastWeatherChange();
```

### Step 3: Wire into Load()

After restoring weather:
```csharp
if (saveData.LastWeatherChange != default(DateTime))
    WeatherManager.SetLastWeatherChange(saveData.LastWeatherChange);
```

### Step 4: Build and verify

Run: `dotnet build`
Expected: Build succeeds. Existing saves still load (new field defaults to `default(DateTime)`).

---

## Task 5: Create OfflineSimulationResult

**Files:** Create `Game/Core/Dati/OfflineSimulationResult.cs`

```csharp
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
    public bool PlantDied { get; set; }
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
        if (PlantDied) s += "!! LA PIANTA E' MORTA DURANTE L'ASSENZA!\n";
        return s;
    }
}
```

---

## Task 6: Create OfflineSimulator

**Files:** Create `Game/Core/Dati/OfflineSimulator.cs`

**This is the core of the feature.** It simulates offline time by:
1. Dividing time into 10-minute environment ticks
2. Per environment tick: resolving DayPhase from timestamp, advancing weather via Markov chain
3. Per environment tick: running 600 sub-ticks (1/sec) of `AggiornaTutto()` with correct parameters
4. Handling water, death detection, and stats tracking

```csharp
using System;

namespace Plants;

public static class OfflineSimulator
{
    private const int TICK_MINUTES = 10;
    private const int SUB_TICKS_PER_TICK = TICK_MINUTES * 60; // 600 (1 per second)
    private const int WEATHER_DURATION_MINUTES = 30;

    public static OfflineSimulationResult Simulate(
        DateTime closeTime,
        DateTime openTime,
        Weather startWeather,
        DateTime lastWeatherChange)
    {
        var result = new OfflineSimulationResult();
        TimeSpan offlineTime = openTime - closeTime;

        if (offlineTime.TotalMinutes < 1 || Game.pianta == null)
        {
            result.TimeSimulated = offlineTime;
            return result;
        }

        var plant = Game.pianta;
        var logic = plant.proprieta;

        // Snapshot before
        result.TimeSimulated = offlineTime;
        result.HealthBefore = plant.Stats.Salute;
        result.HydrationBefore = plant.Stats.Idratazione;
        result.HeightBefore = plant.Stats.Altezza;
        result.LeavesBefore = plant.Stats.FoglieAttuali;
        result.WaterBefore = WaterSystem.Current;

        int totalTicks = (int)(offlineTime.TotalMinutes / TICK_MINUTES);
        if (totalTicks < 1) totalTicks = 1;

        // Weather simulation state
        Weather simWeather = startWeather;
        DateTime nextWeatherChange = lastWeatherChange.AddMinutes(WEATHER_DURATION_MINUTES);
        Random weatherRng = new Random(closeTime.GetHashCode());
        int weatherChanges = 0;

        // Enter offline simulation mode
        Game.IsOfflineSimulation = true;

        try
        {
            bool plantDied = false;

            for (int tick = 0; tick < totalTicks && !plantDied; tick++)
            {
                DateTime tickTime = closeTime.AddMinutes((tick + 1) * TICK_MINUTES);

                // === Advance weather ===
                while (tickTime >= nextWeatherChange)
                {
                    Weather newWeather = WeatherManager.GetNextWeather(simWeather, weatherRng);
                    if (newWeather != simWeather)
                    {
                        simWeather = newWeather;
                        weatherChanges++;
                    }
                    nextWeatherChange = nextWeatherChange.AddMinutes(WEATHER_DURATION_MINUTES);
                }

                // === Resolve environment ===
                DayPhase phase = FaseGiorno.GetPhaseFromTime(tickTime);
                WorldModifier worldMod = WorldManager.GetCurrentModifiers();

                // Set overrides so ALL internal code sees simulated values
                FaseGiorno.SetSimulationOverride(phase);
                WeatherManager.SetSimulationOverride(simWeather);
                WorldManager.SetSimulationOverride(worldMod);

                // === Run sub-ticks (plant simulation at 1/sec rate) ===
                for (int sub = 0; sub < SUB_TICKS_PER_TICK; sub++)
                {
                    logic.AggiornaTutto(phase, simWeather, worldMod);

                    // Check death
                    if (plant.Stats.Salute <= 0)
                    {
                        plantDied = true;
                        break;
                    }
                }

                result.TicksSimulated = tick + 1;
            }

            result.PlantDied = plantDied;
        }
        finally
        {
            // ALWAYS clear simulation mode
            Game.IsOfflineSimulation = false;
            FaseGiorno.SetSimulationOverride(null);
            WeatherManager.SetSimulationOverride(null);
            WorldManager.SetSimulationOverride(null);

            // Update live weather state to where simulation ended
            WeatherManager.SetWeatherDirect(simWeather);
            WeatherManager.SetLastWeatherChange(
                nextWeatherChange.AddMinutes(-WEATHER_DURATION_MINUTES));
        }

        // Snapshot after
        result.HealthAfter = plant.Stats.Salute;
        result.HydrationAfter = plant.Stats.Idratazione;
        result.HeightAfter = plant.Stats.Altezza;
        result.LeavesAfter = plant.Stats.FoglieAttuali;

        // Water: passive recharge for entire offline period
        WaterSystem.AddOfflineRecharge((float)offlineTime.TotalSeconds);
        result.WaterAfter = WaterSystem.Current;

        result.WeatherChanges = weatherChanges;

        return result;
    }
}
```

**IMPORTANT NOTES for implementer:**
- `Game.pianta.proprieta` is the `GameLogicPianta` instance — verify exact field name
- `plant.Stats` is the `PlantStats` instance — verify exact accessor
- `WaterSystem.AddOfflineRecharge` already exists — reuse it for passive recharge
- Verify `AggiornaTutto` signature: `(DayPhase fase, Weather meteo, WorldModifier worldMod)`
- `GetPhaseFromTime(DateTime)` already exists in FaseGiorno — no need to create

---

## Task 7: Replace GameSave Offline Methods

**Files:** `Game/Core/Dati/GameSave.cs`

**Context:** Replace `CalculateOfflineGrowth()` and `SimulateOfflineGrowth()` with the new `OfflineSimulator`.

### Step 1: Replace `CalculateOfflineGrowth()`

```csharp
private void CalculateOfflineGrowth()
{
    if (saveData == null || saveData.SaveTime == default(DateTime)) return;

    TimeSpan timeOffline = DateTime.Now - saveData.SaveTime;
    if (timeOffline.TotalMinutes < 1) return;

    // Resolve starting weather state from save
    Weather startWeather = saveData.CurrentWeather;
    DateTime lastWeatherChange = saveData.LastWeatherChange != default(DateTime)
        ? saveData.LastWeatherChange
        : saveData.SaveTime; // Fallback: assume weather changed at save time

    // Run simulation
    var result = OfflineSimulator.Simulate(
        saveData.SaveTime,
        DateTime.Now,
        startWeather,
        lastWeatherChange);

    // Log summary
    Console.WriteLine(result.GetSummary());
}
```

### Step 2: Remove or comment out `SimulateOfflineGrowth()`

The old method is no longer called. Remove it or mark it `[Obsolete]`.

### Step 3: Remove old `WaterSystem.AddOfflineRecharge` call

The old `CalculateOfflineGrowth` calls `WaterSystem.AddOfflineRecharge` separately. The new code handles it inside `OfflineSimulator.Simulate()`. Remove the old call to avoid double-recharging.

### Step 4: Build and verify

Run: `dotnet build`
Expected: Build succeeds. Old buggy methods replaced.

---

## Task 8: Build & Manual Verification

### Step 1: Full build

```powershell
dotnet build
```

Expected: 0 errors, 0 warnings (or only pre-existing warnings).

### Step 2: Manual test flow

1. Run the game: `dotnet run`
2. Wait for a plant to be growing
3. Close the game (triggers Save with DateTime.Now)
4. Wait 2+ minutes
5. Reopen the game
6. Check console output for simulation summary
7. Verify plant stats changed (health, hydration, height, leaves)
8. Verify weather state is different from saved weather (if enough time passed)
9. Verify no crashes or visual glitches

### Step 3: Edge case tests (manual)

- Close and reopen immediately (<1 min) → should skip simulation
- Close with dying plant (low health) → should die during offline
- Close during night phase → verify night temperature effects simulated

---

## File Summary

| Action | File | Changes |
|--------|------|---------|
| Modify | `Game/Core/Game.cs` | Add `IsOfflineSimulation` field |
| Modify | `Game/Core/Mondo/WorldManager.cs` | Fix IsMeteoOn bug, add simulation override |
| Modify | `Game/Core/Mondo/FaseGiorno.cs` | Add simulation override |
| Modify | `Game/Core/Mondo/WeatherManager.cs` | Add override, public Markov method, accessors |
| Modify | `Game/Core/Pianta/Obj_Plant.cs` | Guard isPaused + auto-scroll for offline |
| Modify | `Game/Core/GameLogic.cs` | Guard events + harvest UI for offline |
| Modify | `Game/Core/Dati/GameSave.cs` | Add LastWeatherChange field, replace offline methods |
| Create | `Game/Core/Dati/OfflineSimulationResult.cs` | Result data class |
| Create | `Game/Core/Dati/OfflineSimulator.cs` | Core simulation engine |

## Execution Order

Tasks 1-4 are independent and can be parallelized.
Task 5-6 depend on Tasks 1-4 (use the override system).
Task 7 depends on Tasks 5-6 (wires simulator into GameSave).
Task 8 depends on all previous tasks.

Recommended: Execute Task 1+2+3+4 in parallel, then 5+6, then 7, then 8.

# CLAUDE.md

Guidance for Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Plants** = plant simulation game. C# .NET 9.0, custom engine on Raylib-CSharp. Windows-only desktop app, system tray. UI fully rendered via Raylib (custom panels, sprites, text) — no ImGui despite NuGet ref. All text Italian.

## Build & Run

```bash
# Run in development (with console window for debug output)
dotnet run --project Plants.csproj

# Publish single-file executable to ./dist (hides console, copies Assets/)
./build.bat
# Equivalent: dotnet publish -c Release -r win-x64 --self-contained true -p:OutputType=WinExe -o ./dist

# Clean
dotnet clean
```

No automated tests. Verify changes by running game manually.

**Trimming**: Toggled via `<PublishTrimmed>` in `Plants.csproj` — user switches between `true` and `false` for testing, so code must work with both. When `true`, `TrimMode=partial` is used and `linker.xml` preserves all types in `Plants` namespace. `GameElement.Create<T>()` uses `new T()` with a `new()` constraint (not reflection), so it is trim-safe without extra annotations. JSON (`System.Text.Json`) still relies on runtime reflection — kept working via `<JsonSerializerIsReflectionEnabledByDefault>true</JsonSerializerIsReflectionEnabledByDefault>`. If you add types constructed dynamically (e.g. `Activator.CreateInstance(Type)`, JSON polymorphism), verify `linker.xml` still covers them.

**Standalone minigame mode**: `Plants.exe --minigioco <TipoMinigioco>` launches single minigame in separate window.

## Architecture

### Entry Point & Main Loop
`Program.Main()` → `Game.Init()` → `Rendering.Init()` (infinite `while(true)` loop at 60 FPS)

Main loop in `Rendering.cs`:
1. Updates all active GameElements
2. Splits into `layerBase` (world, `guiLayer=false`) and `layerGui` (UI, `guiLayer=true`)
3. Sorts each layer by `depth` (higher = drawn on top)
4. Renders world via `PixelCamera` (BeginWorldMode → draw → EndWorldMode → DrawWorld)
5. Renders GUI layer directly on screen
6. Draws `DebugConsole` overlay last

Virtual resolution: 100x125, upscaled 4x to 400x500 window. `PixelCamera` handles world-to-screen projection via `RenderTexture2D`.

Window never truly closes — minimize/close hides to system tray (`ConfigFlags.HiddenWindow`). Exit only via tray icon context menu.

### Object System
- **All game objects** inherit from `GameElement` (in `Engine/GameGeneral/`)
- Factory: `GameElement.Create<T>(depth, room)` — constructs via `new T()` (requires `where T : GameElement, new()`). Trim-safe, no reflection
- Constructor auto-registers in static `GameElement.elementList` (thread-safe with `_elementLock`)
- Call `Destroy()` to remove; finalizer also removes from list
- `Obj_` prefix for game objects, `Obj_Gui` prefix for UI objects
- `persistent = true` keeps element active across room switches

### Room System
- `Room` manages scene activation; one room active at a time
- 6 rooms: `room_main`, `room_inventory`, `room_options`, `room_compost`, `room_upgrade`, plus `room_minigioco` (created by `ManagerMinigames`)
- `room.SetActiveRoom()` activates its elements, deactivates all others (except `persistent`)
- Elements auto-bind to active room at creation unless room passed to `Create<T>()`

### Static Singletons in `Game.cs`
All major objects referenced as static fields: `Game.pianta`, `Game.controller`, `Game.room_main`, `Game.statsPanel`, `Game.toolbar`, etc. `Game.Init()` creates everything in order: rooms → `AssetLoader.LoadAll()` → `ItemRegistry.Init()` → `NotificationManager.Initialize()` → game objects → GUI → inventory → composter → upgrades → minigames → load save.

### Timers (System.Timers.Timer, run on thread pool)
| Timer | Interval | Purpose |
|-------|----------|---------|
| `Timer` | 1s | Plant logic tick (`GameLogicPianta.AggiornaTutto`) |
| `TimerSave` | 10s | Auto-save (`GameSave.get().Save()`) |
| `TimerFase` | 1h | Day phase update |

All timers pause during `IsModalitaPiantaggio` or `isPaused`. 1s timer = core simulation tick — **all plant damage, growth, weather effects happen here**, not in render loop.

### Data Persistence
- `GameSave` singleton — JSON to `%APPDATA%/Plants/savegame.json` via `SaveHelper`
- `Inventario` singleton — JSON to `%APPDATA%/Plants/inventario.json` (seed collection)
- `ItemInventory` singleton — JSON to `%APPDATA%/Plants/items.json` (equipped items)
- `SaveHelper` uses `System.Text.Json` with `WriteIndented`, `camelCase` naming, `IncludeFields`
- Always call `GameSave.get().Save()` on exit; auto-save runs every 10s
- `PlantDead` flag in save triggers piantaggio mode on next launch (preserves leaves, essence, upgrades)

### Assets
All PNG/shader files in `Assets/` copied to output (configured in .csproj `CopyToOutputDirectory`). Loaded once in `AssetLoader.LoadAll()` during `Game.Init()`. Sprites wrap `Texture2D` with scale and origin. **Never load assets in render loop.**

Shaders: `base.vert/frag`, `recolor.frag` (plant coloring), `seed.frag` (seed rendering).

## Game Systems

### Plant Growth (`Game/Core/Pianta/`)
- `Obj_Plant` holds `PlantStats`, `SeedStats` (bonus from seed type), geometry (spline points, branches, roots, ivy)
- `GameLogicPianta` (in `GameLogic.cs`) runs every 1s tick: temperature → damage → hydration → oxygen → metabolism → parasites → leaves → storms → item hooks → clamp → events
- Plant geometry: trunk = spline points, branches = `Obj_Ramo` (with leaves), roots = `Obj_Radice`, ivy = `Obj_RamoEdera`
- Growth limited by `Stats.EffectiveMaxHeight` (= `AltezzaMassima * WorldModifier.LimitMultiplier`)
- Death: when `Salute <= 0`, sets `PlantDead=true` in save, shows death screen, enters piantaggio

### Seed System (`Game/Definitions/`)
- 10 types: Normale, Poderoso, Fluviale, Glaciale, Magmatico, Puro, Florido, Rapido, Antico, Cosmico
- 7 rarities (non-sequential enum!): Comune(1), NonComune(2), Raro(3), Esotico(100), Epico(4), Leggendario(5), Mitico(101)
- **Use `SeedDefinitions.RarityOrder[]` and `GetRarityRank()` for ordering** — never compare enum int values directly
- `SeedDefinitions` = single source of truth for all seed data: names, colors, bonuses, generation ranges, rarity multipliers
- Breeding via `SeedFusionManager`: 70/30 stat inheritance + mutation, max 4 fusions per seed, Mitico not reachable via fusion
- Complementary pairs get compatibility bonus: Glaciale↔Magmatico, Fluviale↔Florido, Rapido↔Poderoso, Puro↔Antico

### Seed Stat Scaling (`Game/Definitions/SeedStatScaling.cs`)
- Stored in real 0-99 numbers (no separate display layer). Display = stored, rounded to int in UI bars
- Primarie (vitalita, idratazione, metabolismo, vegetazione): baseline 10 = neutro, mins 5/3/5/5, max 99
- Resistenze (freddo, caldo, parassiti, vuoto): baseline 0 = nessun bonus, max 99, cap efficace 0.95
- Stage-based requirement (loop mondi): `primary_req = 10 + (stage-1)*0.7`, `resistance_req = 100 + (stage-1)*1.0`. Stat più alto dello stage corrente = vantaggio; più basso = svantaggio. Un seme 10 regge finché stage < ~10
- `EffectiveMultiplier(stat, stage) = stat / StagePrimaryRequirement(stage)` usato da `GameLogicPianta` per applicare bonus allo scaling
- `EffectiveResistance(stat, stage) = clamp(stat / StageResistanceRequirement(stage), 0, 0.95)` usato come frazione di resistenza additiva
- Fusione: 70/30 media pesata + mutazione ±15% + bonus complementarietà. Output mai sproporzionato rispetto ai parent (proporzionale alla loro somma)
- `NeedsLegacyMigration(s)` euristica: vitalita<3 = vecchio formato (0-2.5). `MigrateLegacyStats` applica ×10 primarie, ×100 resistenze. Chiamata in `GameSave.Load` e `Inventario.Load` (idempotente)

### World System (`Game/Core/Mondo/`, `Game/Definitions/WorldDefinitions.cs`)
- 10 worlds: Serra(tutorial) → Terra → Luna → Marte → Europa → Venere → Titano → ReameMistico → GiardinoMistico → Origine
- `WorldModifier` struct has ~20 float multipliers applied to plant simulation
- `WorldManager.GetCurrentModifiers()` applies stage-based difficulty scaling on top of base modifiers
- Stage progression: `currentStage++` increases difficulty multiplier (`1.0 + stage/10 * 0.25`)
- Weather: 7 types (Sunny, Cloudy, Rainy, Stormy, Windy, Foggy, Snowy), changes every 30 min
- Day phases: Night(0-5), Dawn(6-7), Morning(8-11), Afternoon(12-17), Dusk(18-19), Evening(20-23) — real clock

### Climate Constants (`Game/Definitions/ClimateDefinitions.cs`)
All balancing constants for temperature thresholds, resource consumption rates, parasite chances, photosynthesis energy per day phase, weather modifiers live here. `GameLogic.cs` re-exports them as aliases for compatibility.

### Item System (`Game/Items/`)
- `ItemDefinition` abstract base with lifecycle hooks: `OnStart`, `OnEnd`, `OnGrow`, `OnBranchNew`, `OnWeatherRain`, etc.
- 6 items registered in `ItemRegistry.Init()`: fertilizzante, scudo_gelo, acceleratore_rami, parapioggia, radicatore, fotosintesi
- 3 equip slots per seed (`Seed.MAX_ITEM_SLOTS = 3`)
- `ItemHookCaller` invokes hooks on equipped items during game events

### Upgrade System (`Game/Core/UpgradeSystem.cs`)
- 3 upgrade types: Innaffiatoio (water tank), Inventario (seed slots), SpazioPacchetti (pack slots)
- Max level 5 each, costs: 50, 150, 400, 800, 1500 leaves
- Currency: `FoglieAccumulate` (accumulated leaves)

### Water System (`Game/Core/WaterSystem.cs`)
- Finite water tank (base 100, +50 per Innaffiatoio upgrade level)
- Consumes 10/sec when watering, recharges passively at ~0.033/sec

### Minigames (`Game/Minigiochi/`)
- 7 types: Cerchio, Tieni, Resta, Semi, Treni, Blackjack, PicturePoker
- All extend `MinigiocoBase` (states: Intro → InCorso → Vittoria/Sconfitta)
- Triggered by clicking golden leaves on branches
- Standalone via `--minigioco` command-line arg

### Seed Selection (Piantaggio)
- `Game.EntraModalitaPiantaggio()` / `Game.EsciModalitaPiantaggio()` toggle mode
- During `IsModalitaPiantaggio`: game logic paused, plant hidden, timers skip
- `Obj_GuiPiantaggio` shows seed grid; `isFalling` flag blocks all input during fall animation
- Death screen (`Obj_GuiMorte`) — do NOT call `GameSave.DeleteSaveFile()` elsewhere

### Seed Recovery
- `SeedRecoverySystem`: countdown (survival) → rewind (visual) → seed returned to inventory
- During `IsRewinding`: no damage/growth applied (checked in timer callback)

### Mail System (`Game/Core/Posta/`)
- `MailSystem` with daily recurring mails, `MailTemplates` for content

### Notification System
- `NotificationMonitor` checks plant events every frame, plant status every 30s
- Suppressed when window focused or during piantaggio
- 5-min cooldown between same-type notifications
- Uses `Microsoft.Toolkit.Uwp.Notifications` for Windows toast

### Offline Simulation
- `OfflineSimulator` simulates elapsed time on game load (from `SaveTime` to now)
- Uses `Game.IsOfflineSimulation` flag to suppress UI-only operations during sim

### Debug Console
- Toggle with backtick key (`` ` ``)
- Commands: `reset all/inventory`, `add seed/leaf/essence`, `weather set`, `world set`, `stage set`, `plant grow`, `tick set/reset`, `kill`, `godmode`, `minigame start/end`, `plant info`, `help`
- `DebugConsole.GodMode` skips all damage, maxes stats, only grows
- Has command history, autocomplete, picker UI

## Key Conventions

### Naming
- Classes/Methods: PascalCase (`Obj_Plant`, `LoadTexture()`)
- Variables: camelCase (`currentPhase`, `worldState`)
- Game objects: `Obj_` prefix; GUI objects: `Obj_Gui` prefix
- Italian vocabulary: Annaffia (water), Ramo (branch), Radice (root), Fase (phase), Pianta (plant), Seme (seed), Inventario (inventory), Foglie (leaves), Salute (health), Parassiti (parasites), Innaffiatoio (watering can), Compost (compost), Piantaggio (planting), Morte (death), Nascondere/Mostrare (hide/show)

### Rules
- Do NOT use Raylib directly in GUI code — use Engine wrappers if avaiable
- Do NOT modify window handles without validation
- Do NOT skip `GameSave.Save()` on exit
- Do NOT load assets in render loop
- Always `Destroy()` child GameElements before removing parent
- Inheritance only — no interfaces in this codebase
- Thread safety: timer callbacks run on thread pool — `GameElement.elementList` access uses `_elementLock`
- Never compare `SeedRarity` enum values directly for ordering — use `SeedDefinitions.GetRarityRank()`

## Key Dependencies

| Package | Purpose |
|---------|---------|
| Raylib-CSharp 4.0.1 | Graphics/window/input |
| System.Drawing.Common 10.0.0 | Icon loading for tray |
| Microsoft.Toolkit.Uwp.Notifications 7.1.3 | Windows toast notifications |
| NotificationIconSharp 1.0.1 | System tray icon |

## Limits & Constants

| Constant | Value | Location |
|----------|-------|----------|
| MAX_GAME_ELEMENTS | 500 | GameProperties |
| MAX_PARTICLES | 3000 | GameProperties |
| MAX_SPLINE_POINTS | 1000 | GameProperties |
| MAX_ITEM_SLOTS | 3 | Seed |
| MAX_FUSIONS | 4 | Seed |
| MAX_BREEDING_RANK | 5 (Leggendario) | SeedDefinitions |
| TOTAL_WORLDS | 10 | WorldDefinitions |
| UpgradeSystem.MaxLevel | 5 | UpgradeSystem |
| WEATHER_DURATION_MINUTES | 30 | ClimateDefinitions |
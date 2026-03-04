# PROJECT KNOWLEDGE BASE

**Generated:** 2026-03-04
**Commit:** 2125d2c
**Branch:** (current)

## OVERVIEW

**Plants** is a plant management/simulation game built with Raylib-CS and DearImGui in C# .NET 9.0.

- **Location**: `C:\Dev\Plants`
- **Type**: Desktop game (Windows)
- **Engine**: Custom engine using Raylib-CSharp + DearImGui
- **Framework**: .NET 9.0 (self-contained, single-file publish)

## Tech Stack

| Component | Technology |
|-----------|------------|
| Game Library | Raylib-CSharp 4.0.1 |
| GUI | CopperDevs.DearImGui 1.4.2 |
| Notifications | Microsoft.Toolkit.UWP.Notifications 7.1.3 |
| System Tray | NotificationIconSharp 1.0.1 |
| Target | net9.0-windows10.0.17763.0 |

## STRUCTURE

```
C:\Dev\Plants/
├── Program.cs          # Entry point, window init, system tray
├── Plants.csproj       # .NET 9.0 project config
├── Engine/             # Custom game engine
│   ├── GameGeneral/    # Core: Sprite, GameElement, Room, AssetLoader
│   ├── Gui/            # ImGui wrappers: Gui, Texture2DFieldRenderer
│   ├── Tools/          # Helpers: WindowState, ViewCulling, Utils, Mouse, Math, Random
│   ├── Rendering.cs    # Main rendering loop
│   └── PixelCamera.cs  # 2D camera system
├── Game/               # Game logic
│   ├── Core/           # Main systems
│   │   ├── Pianta/     # Plant system: Obj_Plant, Obj_Seed, Obj_Ramo, LeafHarvest
│   │   ├── Mondo/      # World: WorldManager, WeatherManager, Day/night cycle
│   │   ├── Dati/       # Data: GameSave, Inventario
│   │   ├── Upgrade/    # Upgrade system
│   │   └── NotificationSystem/  # Event & notification handling
│   ├── Minigiochi/     # Minigames (6 total)
│   ├── Gui/            # UI components
│   │   ├── Inventario/ # Inventory UI
│   │   ├── Pacchetti/  # Pack opening UI
│   │   └── Main/       # Main UI: toolbar, scrollbar, stats panel
│   ├── Effects/        # Visual: particles, water effects
│   └── Definitions/    # Data: PlantStats, Seed definitions
├── Assets/             # Textures, shaders, icons
└── build.bat           # Build script
```

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Add new plant type | Game/Core/Pianta/ | See Obj_Plant.cs pattern |
| Add new minigame | Game/Minigiochi/ | Copy MinigiocoBase.cs |
| Fix GUI bug | Game/Gui/ | ImGui-based panels |
| Fix engine bug | Engine/GameGeneral/ | GameElement, Room base classes |
| Add new world system | Game/Core/Mondo/ | Weather, Ground, Background |

## KEY SYSTEMS

### Core Architecture

- **Game.Init()** → **Rendering.Init()** → Main loop
- **Obj_Controller** → Central game state manager
- **WorldManager** → Manages world rendering and transitions
- **GameSave** → JSON serialization for save/load
- **System Tray** → Minimize to tray, click to restore

### Object System

- All game objects inherit from `GameElement`
- `Obj_` prefix for game objects (e.g., `Obj_Plant`, `Obj_Seed`)
- GUI components in `Game/Gui/` follow same pattern

### Rendering

- Raylib-CSharp for low-level rendering
- Custom `PixelCamera` for 2D camera with zoom/pan
- Shaders in `Assets/shader/` (GLSL)
- Particle system in `Effects/`

## CODE MAP

| Symbol | Type | Location | Role |
|--------|------|----------|------|
| GameElement | base class | Engine/GameGeneral/ | All game objects inherit |
| Game | singleton | Game/Core/ | Main game state |
| Rendering | static | Engine/ | Game loop, render pipeline |
| Obj_Plant | class | Game/Core/Pianta/ | Plant entity |
| ManagerMinigames | class | Game/Minigiochi/ | Minigame lifecycle |

## CONVENTIONS (THIS PROJECT)

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `Obj_Plant`, `GameSave` |
| Methods | PascalCase | `LoadTexture()`, `UpdateWorld()` |
| Variables | camelCase | `currentPhase`, `worldState` |
| Files | PascalCase + prefix | `Obj_Plant.cs`, `Gui_Toolbar.cs` |
| GUI Objects | Obj_Prefix | `Obj_GuiBottomNavigation`, `ObjWater` |

- No interfaces - uses base classes instead
- Enums: Nested in classes or Definition files

## ANTI-PATTERNS (THIS PROJECT)

- **DO NOT** use Raylib functions directly in GUI code — use Engine wrappers
- **DO NOT** modify window handle without checking `Window.GetHandle()` returns valid
- **DO NOT** skip `GameSave.Save()` on exit — data loss
- **DO NOT** load assets in render loop — pre-load in `AssetLoader`
- **ALWAYS** call Destroy() on child GameElements
- **NO** automated tests exist

## UNIQUE STYLES

- Game loop in Rendering.cs (unconventional - typically in Game.cs)
- Uses infinite `while(true)` loop instead of `!Window.ShouldClose()`
- Room-based activation/deactivation system
- Italian method names in some places (Ferm, Chiudi)

## COMMANDS

```powershell
# Build (uses build.bat)
.\build.bat

# Run
dotnet run --project Plants.csproj

# Publish (single-file self-contained)
dotnet publish -c Release
```

## SKILLS AVAILABLE

| Skill | Trigger | Purpose |
|-------|---------|---------|
| build-plants | "build plants", "compila plants" | Build .NET project |
| run-plants | "run plants", "avvia plants" | Run game executable |
| debug-plants | "debug plants", "errore plants" | Analyze and fix errors |
| test-plants | "test plants" | Run tests (none exist) |
| clean-plants | "clean plants" | Clean build artifacts |
| lint-plants | "lint plants" | Linting (C# not applicable) |
| check-assets | "check assets" | Verify texture files |
| analyze-performance | "analyze performance" | Profile game performance |
| explore-code | "explore code", "struttura" | Show project structure |
| find-pattern | "find pattern", "cerca" | Search code patterns |
| git-status | "git status" | Git operations |

## NOTES

- No CI/CD (manual builds only)
- Single .csproj - no test project
- .NET 9.0-windows10.0.17763.0 target
- Self-contained single-file executable

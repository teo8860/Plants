# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Plants** is a plant simulation game built with C# .NET 9.0 using a custom engine on top of Raylib-CSharp and CopperDevs.DearImGui. Windows-only desktop app with system tray integration.

## Build & Run

```bash
# Run in development
dotnet run --project Plants.csproj

# Publish single-file executable to ./dist
./build.bat
# Or: dotnet publish -c Release -r win-x86 --self-contained true -o ./dist

# Clean
dotnet clean
```

No automated tests exist in this project.

## Architecture

### Entry Point & Main Loop
`Program.Main()` → `Game.Init()` → `Rendering.Init()` (infinite `while(true)` loop)

The main loop in `Rendering.cs` updates all active GameElements, sorts by depth, renders world to a RenderTexture2D via PixelCamera, then renders the ImGui GUI layer on top. Virtual resolution is 100x125, upscaled 4x to a 400x500 window.

### Object System
- **All game objects** inherit from `GameElement` (in `Engine/GameGeneral/`)
- Factory: `GameElement.Create<T>()` with optional depth and room binding
- Objects tracked in static `GameElement.elementList`; call `Destroy()` to remove
- `Obj_` prefix for game objects, `Obj_Gui` prefix for UI objects

### Room System
- `Room` manages scene activation; only one room active at a time
- 5 rooms: main, inventory, options, compost, upgrade
- GameElements bound to a room deactivate when the room does
- Switch via `room.SetActiveRoom()`

### Static Singletons in `Game.cs`
All major objects are referenced as static fields: `Game.pianta`, `Game.controller`, `Game.room_main`, `Game.statsPanel`, `Game.toolbar`, etc.

### Rendering Pipeline
- `PixelCamera` handles 2D camera with zoom/pan
- `BeginWorldMode()` → world render to texture; `BeginScreenMode()` → screen UI
- Elements sorted by `depth` (higher = drawn on top)
- Separate `guiLayer` flag distinguishes world vs UI elements

### Data Persistence
- `GameSave` singleton uses JSON serialization, auto-saves every 10 seconds
- `Inventario` singleton stores seed inventory separately
- Always call `GameSave.get().Save()` on exit

### Assets
All PNG/shader files in `Assets/` are embedded as resources (configured in .csproj). Loaded once in `AssetLoader.LoadAll()` during `Game.Init()` — never load assets in the render loop.

## Key Conventions

### Naming
- Classes/Methods: PascalCase (`Obj_Plant`, `LoadTexture()`)
- Variables: camelCase (`currentPhase`, `worldState`)
- Game objects: `Obj_` prefix; GUI objects: `Obj_Gui` prefix
- Some Italian names in code: `Annaffia` (water), `Ramo` (branch), `Radice` (root), `Fase` (phase), `Pianta` (plant), `Seme` (seed), `Inventario` (inventory)

### Rules
- Do NOT use Raylib directly in GUI code — use Engine wrappers
- Do NOT modify window handles without validation
- Do NOT skip `GameSave.Save()` on exit
- Do NOT load assets in the render loop
- Always `Destroy()` child GameElements before removing parent
- Inheritance only — no interfaces used in this codebase

## Key Dependencies

| Package | Purpose |
|---------|---------|
| Raylib-CSharp 4.0.1 | Graphics/window |
| CopperDevs.DearImGui 1.4.2 | Immediate-mode GUI |
| CopperDevs.DearImGui.Renderer.Raylib 2.0.2 | ImGui+Raylib bridge |
| Microsoft.Toolkit.Uwp.Notifications 7.1.3 | Windows toast notifications |
| NotificationIconSharp 1.0.1 | System tray icon |

## Seed Selection (Piantaggio)

- `Obj_GuiPiantaggio` shows a centered grid of seeds with rarity-colored borders
- Selecting a seed shows an info panel (name + rarity + "Pianta!" button) at the bottom
- Clicking "Pianta!" triggers a falling seed animation (with particles on impact) before actually planting
- During `IsModalitaPiantaggio`, game logic (timers, notifications, growth) is paused
- The `isFalling` flag on `Obj_GuiPiantaggio` blocks all input (including room navigation) during the fall animation
- Death screen (`Obj_GuiMorte`) is the only place that calls `GameSave.DeleteSaveFile()` — do NOT delete the save elsewhere

## Notification System

- `NotificationMonitor` checks plant events every frame and plant status every 30 seconds
- Notifications are suppressed when the game window is focused, and during `IsModalitaPiantaggio`
- `NOTIFICATION_TIMEOUT` (300s / 5 min) controls the cooldown between repeated notifications of the same type
- `PlantEventSystem` fires events based on state transitions (low water, critical health, parasites, temperature, world transition)
- Toast notifications use `Microsoft.Toolkit.Uwp.Notifications` with action buttons

## Game Systems (for context)

- **Plant growth**: branches (`Obj_Ramo`), roots (`Obj_Radice`), ivy (`Obj_RamoEdera`), stats in `PlantStats`
- **Seed genetics**: 10 types, 5 rarities, breeding via `SeedFusionManager` (70/30 stat inheritance + mutation)
- **World**: 3 worlds with different growth modifiers, weather system, 6-hour day/night phases (`FaseGiorno`)
- **Minigames**: 6 types managed by `ManagerMinigames`, all extend `MinigiocoBase`
- **Offline simulation**: `OfflineSimulator` simulates growth while game is closed

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build

```bat
dotnet publish -c Release -r win-x86 --self-contained true -o ./dist
```

Or use `build.bat`. There are no tests or linters configured.

To run in development: open `Plants.sln` in Visual Studio or use `dotnet run`.

## Project Overview

A Windows virtual plant care simulation game written in C# (.NET 9.0) using:
- **Raylib-CSharp** — 2D rendering
- **CopperDevs.DearImGui** — immediate mode GUI (via Raylib renderer)
- **Microsoft.Toolkit.Uwp.Notifications** — Windows toast notifications

Window is 400×500 pixels scaled from a 100×125 internal pixel-art resolution at 60 FPS.

## Code Architecture

### Two-Layer Structure

**Engine/** — reusable framework:
- `GameElement` — base class for all game objects; has `Update()`, `Draw()`, depth, room assignment, and active flag
- `Room` — scene container; only elements assigned to the active room are updated/drawn
- `Rendering.cs` — main loop: update all active elements → sort by depth → draw world (camera-transformed) → draw GUI (screen space) → ImGui overlays
- `PixelCamera.cs` — pixel-perfect camera with world↔screen coordinate transforms

**Game/** — game-specific logic built on the engine.

### Key Game Singletons (in `Game.cs`)

- `Game.room_main`, `room_inventory`, `room_options`, `room_compost` — the four rooms/screens
- `Game.plant` — the `Obj_Plant` instance (the main plant entity)
- All UI panels are `GameElement` instances registered to rooms

### Plant System (`Game/Core/Pianta/`)

The plant is a spline-based 2D entity:
- `Obj_Plant` — root entity; owns growth state, stats, genetics
- `Obj_Ramo` / `Obj_RamoEdera` — branch variants; `Obj_Radice` — roots
- `GameLogicPianta` — AI/simulation tick (runs every `LOGIC_UPDATE_INTERVAL` ~500ms)
- `LeafHarvestSystem` — handles resource collection from leaves

### World & Environment (`Game/Core/Mondo/`)

- `WorldManager` — 10 worlds (Terra, Luna, Marte, Europa, Venere, Titano, ReameMistico, GiardinoMistico, Origine, Serra), each described by a `WorldModifier` struct with 17 parameters (gravity, oxygen, temperature, difficulty, etc.)
- `WeatherManager` — dynamic weather effects
- `FaseGiorno` — 6-phase day/night cycle (Night, Dawn, Morning, Afternoon, Dusk, Evening)

### Genetics & Seeds (`Game/Definitions/`, `Game/Core/`)

- `SeedType` — 10 genetic types (Normale, Poderoso, Fluviale, Glaciale, Magmatico, Puro, Florido, Rapido, Antico, Cosmico)
- `SeedRarity` — 7 rarity tiers (Comune → Mitico)
- `SeedStats` — 8 stat bonuses (Vitalita, Idratazione, Resistenza variants, Vegetazione, Metabolismo)
- `SeedFusionManager` — hybrid breeding (up to 4 fusions per seed)
- `SeedUpgradeSystem` — stat enhancement

### Persistence (`Game/Core/Dati/`)

- `GameSave` — singleton; binary serialization with SHA256 encryption; auto-saves every ~500ms and on exit

### Naming Convention

The codebase uses **Italian** for domain names: Pianta=Plant, Ramo=Branch, Radice=Root, Mondo=World, Fase=Phase, Seme=Seed, Inventario=Inventory, Dati=Data, Pacchetti=Packs. Follow this convention when adding new domain concepts.

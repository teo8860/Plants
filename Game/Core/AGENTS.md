# Game/Core - Main Systems

**Parent:** Root AGENTS.md

## OVERVIEW
Core game logic and systems - plant management, world simulation, upgrades, data persistence.

## SUBMODULES
- **Pianta/** — Plant entities (Obj_Plant, Obj_Seed, Obj_Ramo, Obj_Radice)
- **Mondo/** — World rendering (WeatherManager, WorldManager, ObjGround, ObjBackground)
- **Dati/** — Data persistence (Inventario, GameSave)
- **NotificationSystem/** — Event system (PlantEventSystem, NotificationManager)
- **PackOpening/** — Pack opening mechanics (CompostSystem, OpeningSystem)

## KEY CLASSES
| Class | Role |
|-------|------|
| Game | Main singleton, state management |
| Obj_Plant | Plant growth logic, branches, roots |
| WaterSystem | Irrigation mechanics |
| UpgradeSystem | Player upgrades |
| SeedFusionManager | Seed combination logic |

## CONVENTIONS
Same as root +:
- System classes use `Manager` suffix (ManagerMinigames)
- Italian method names common (Init, Update, Ferma, Chiudi)

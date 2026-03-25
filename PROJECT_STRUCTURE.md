# Plants Project Structure

## Project Architecture Diagram

![Plants Project Structure](https://mermaid.ink/img/pako:eNptku-OnCAUxb_3KXyBfYUmrCI1rX-iTKcNMc1dvePQKkyQ3WTfvhHGUdn9YvCeH3A8nsHA7Rrx5EsURVE1grJz9PT0NaJqkArDKYPJz9zDM15gYtEYKjQwtqHMBdd6nD_Ma1ookoUbVo5Fq-CBTJvwbe5Xtdu9yk9PjsqYi1gbb45ydMvHY8pjmWZGJXCo5SN1dQ5WmqaCXC3Z2DpSEpiLBi1TSSq3mnZHlfm-E1z9E-fL3T6yVNXoc0bRHgpGcxi6jQDgTTmtxBoumeZ8tToGeEJ6JTL2hsmCkdrYaeAuPqTJScOI8uP8VLasGsXeLGiYdbMjLIinFGcFe0eSgYEATnbUZ-_UlhRmZ1EaFW08Vq0lCxek2GOjxU9tFybP0tyi0lRfZwZLcnvMJnzLfjrL06bFXufTkBe7prUDDCW9WorFg5woUjkcoK36uiI9Lm3dmZH-kKhJ_X7EKun_lDZVUA1FyciZ39pa--KieSUMfzdHPMN_T3whWi3tsDoMJ5wCJaR1_2w6J0XRXqQOooflWUd3gtK89TdOtMemv5StcbdqDWpGaL1IFxspuxH1fE3pnfKCuKC7O9iA3lCZi6U77H8rzLoQ)

## Mermaid Source Code

```mermaid
graph TD
    Plants --> Engine
    Plants --> Game
    
    Engine --> GG[GameGeneral]
    Engine --> T[Tools]
    Engine --> REND[Rendering]
    Engine --> EG[EngineGui]
    
    Game --> CORE[Core]
    Game --> GUI[Gui]
    Game --> MINI[Minigiochi]
    Game --> EFF[Effects]
    Game --> DEF[Definitions]
    
    CORE --> CTRL[Obj_Controller]
    CORE --> GAMEC[Game]
    CORE --> WATER[WaterSystem]
    CORE --> DATI[Inventario GameSave]
    CORE --> PIANTA[Obj_Plant Obj_Seed Obj_Ramo]
    CORE --> MONDO[WeatherManager WorldManager FaseGiorno]
    CORE --> UPGRADE[UpgradeSystem]
    CORE --> NOTIFY[NotificationSystem]
    
    GUI --> TOOL[Obj_GuiToolbar]
    GUI --> STATS[Obj_GuiStatsPanel]
    GUI --> INV[Obj_GuiInventoryGrid]
    GUI --> PACK[Obj_GuiPackOpeningAnimation]
    
    MINI --> MBASE[MinigiocoBase]
    MINI --> MGR[ManagerMinigames]
    MINI --> CERCH[MinigiocoCerchio]
    MINI --> SEMI[MinigiocoSemi]
    
    EFF --> WATERFX[ObjWater]
    EFF --> PART[ObjParticles]
    
    DEF --> PSTATS[PlantStats]
    DEF --> SEED[Seed]
```

## Legend

- **Plants**: Entry point (Program.cs, Plants.csproj)
- **Engine**: Custom game engine core
  - **GameGeneral**: Base classes (GameElement, Sprite, Room, AssetLoader)
  - **Tools**: Helpers (Window, ViewCulling, Utils, TrayIcon, SaveHelper, Random, Mouse, Math, Coordinate)
  - **Rendering**: Main render loop, PixelCamera
  - **EngineGui**: ImGui wrappers
- **Game**: Game logic
  - **Core**: Controllers, Game state, Water system, Data (Inventory, Save), Plant objects, World/Weather, Upgrades, Notifications
  - **Gui**: UI components (Toolbar, Stats, Inventory, Pack opening)
  - **Minigiochi**: Minigame system
  - **Effects**: Visual effects (Water, Particles)
  - **Definitions**: Data models (PlantStats, Seed)

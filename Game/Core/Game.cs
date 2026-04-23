using Raylib_CSharp;
using Raylib_CSharp.Windowing;
using Raylib_CSharp.Colors;
using System;
using System.Timers;
using System.Numerics;

namespace Plants;

public static class Game
{
    public static Room room_main;
    public static Room room_inventory;
    public static Room room_options;
    public static Room room_compost;
    public static Room room_upgrade;

    public static Obj_Controller controller;
    public static ObjWater innaffiatoio;
    public static Obj_Plant pianta;

    public static Obj_GuiToolbar toolbar;
    public static Obj_GuiToolbar toolbarBottom;

    public static ObjBackground background;
    public static ObjGround ground;
    public static ObjWeatherRender weatherSystem;

    public static bool cambiaPhase = false;
    public static bool isPaused = false;
    public static bool IsOfflineSimulation = false;
    public static bool IsModalitaPiantaggio = false;

    public static bool pendingDeath = false;
    public static HarvestResult pendingDeathHarvest = null;

    public static Obj_GuiPiantaggio guiPiantaggio;
    public static Obj_GuiMorte guiMorte;

    public static Timer Timer;
    public static Timer TimerSave;
    public static Timer TimerFase;
    public static DayPhase Phase;

    public static Obj_GuiStatsPanel statsPanel;
    public static OxygenSystem oxygenSystem;

    public static Obj_Tutorial tutorial;

    public static Obj_GuiWorldTransition worldTransition;

    public static Obj_GuiInventoryBackground inventoryBackground;
    public static Obj_GuiInventoryGrid inventoryGrid;
    public static Obj_GuiSeedDetailPanel seedDetailPanel;
    public static Obj_GuiInventoryCrates inventoryCrates;
    public static Obj_GuiInventoryCratesBackground inventoryCratesBackground;
    public static Obj_GuiSeedUpgradePanel seedUpgradePanel;
    public static Obj_GuiFusionResultPopup guiFusionResultPopup;

    public static Obj_GuiCompostPanel compostPanel;
    public static Obj_GuiCompostBackground compostBackground;
    public static Obj_GuiPackOpeningAnimation packOpening;

    public static Obj_GuiUpgradePanel upgradePanel;

    public static NotificationMonitor notificationMonitor;

    public static Obj_GuiLeafHarvestPopup leafHarvestPopup;
    public static Obj_GuiSeedRecovery guiSeedRecovery;
    public static Obj_GuiOpzioniPopup guiOpzioniPopup;
    public static Obj_GuiPostaPopup guiPostaPopup;
    public static Obj_GuiRewardPopup guiRewardPopup;
    public static Obj_GuiPostaBadge guiPostaBadge;
    public static Obj_GuiTutorialSlideshow tutorialSlideshow;

    public static Obj_GuiItemBoard itemBoard;
    public static Obj_GuiItemSlots itemSlots;

    public static void Init()
    {
        room_main = new Room();
        room_inventory = new Room(false);
        room_options = new Room(false);
        room_compost = new Room(false);
        room_upgrade = new Room(false);

        AssetLoader.LoadAll();

        ItemRegistry.Init();

        NotificationManager.Initialize();

        InitMainGame();
        InitGui();
        InitInventory();
        InitComposter();
        InitUpgrade();
        ManagerMinigames.Init();
        
        if (SaveHelper.Exists("tutorial.json") == false)
            GameElement.Create<Obj_Logo>();

        Inventario.get().Load();
        ItemInventory.get().Load();
        AddTestItems();

        bool hasSave = SaveHelper.Exists("savegame.json");
        if (hasSave)
        {
            GameSave.get().Load();
        }
        else
        {
            // Blocca subito prima che i timer partano
            isPaused = true;
        }

        notificationMonitor = GameElement.Create<NotificationMonitor>();

        SetTimerSave();
        SetTimer();
        SetTimerFase();

        Phase = FaseGiorno.GetCurrentPhase();

        Rendering.camera.position.Y = 0;

        // Priorità: tutorial non completato → sempre tutorial (anche al primo avvio).
        // Altrimenti, se non c'è save o la pianta è morta → selezione seme.
        // I dati di progressione (foglie, essence, upgrade, inventario) vengono
        // preservati se presenti nel save.
        bool tutorialPending = !Obj_Tutorial.IsTutorialCompleted();
        bool plantDead = hasSave && GameSave.get().data.PlantDead;

        if (tutorialPending)
        {
            tutorial.StartTutorial();
        }
        else if (!hasSave || plantDead)
        {
            WorldManager.SetCurrentWorld(WorldType.Terra);
            pianta.SetNaturalColors(WorldType.Terra);
            EntraModalitaPiantaggio();
        }
        //seedTest();
	}

    private static void AddTestItems()
    {
        // Aggiunge oggetti di test all'inventario ad ogni avvio
        ItemInventory.get().Add("fertilizzante");
        ItemInventory.get().Add("scudo_gelo");
        ItemInventory.get().Add("acceleratore_rami");
        ItemInventory.get().Add("parapioggia");
        ItemInventory.get().Add("radicatore");
        ItemInventory.get().Add("fotosintesi");
        Console.WriteLine("[Test] 6 oggetti di prova aggiunti all'inventario");
    }

    private static void seedTest()
    {
        Obj_Seed s;
        
        s = new Obj_Seed(new Seed(){ type = SeedType.Normale });
        s.position = new Vector2(100, 100);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Poderoso });
        s.position = new Vector2(150, 100);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Fluviale });
        s.position = new Vector2(200, 100);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Glaciale });
        s.position = new Vector2(250, 100);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Magmatico });
        s.position = new Vector2(100, 150);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Puro });
        s.position = new Vector2(150, 150);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Florido });
        s.position = new Vector2(200, 150);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Rapido });
        s.position = new Vector2(250, 150);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Antico });
        s.position = new Vector2(100, 200);
        s.scale = 2.0f;

        
        s = new Obj_Seed(new Seed(){ type = SeedType.Cosmico });
        s.position = new Vector2(150, 200);
        s.scale = 2.0f;
    }

    private static void InitMainGame()
    {
        background = GameElement.Create<ObjBackground>(100);
        ground = GameElement.Create<ObjGround>(99);

        innaffiatoio = GameElement.Create<ObjWater>(-100, room_main);
        innaffiatoio.Initialize(GameProperties.cameraWidth, GameProperties.cameraHeight);


        weatherSystem = new ObjWeatherRender();

        controller = new Obj_Controller();

        tutorial = GameElement.Create<Obj_Tutorial>(-1);
        pianta = GameElement.Create<Obj_Plant>(-2);

        var colore1 = Color.FromHSV(130, 0.45f, 0.68f);
        var colore2 = Color.FromHSV(133, 0.47f, 0.44f);
        pianta.setColori(colore1, colore2);

   
        worldTransition = GameElement.Create<Obj_GuiWorldTransition>(-200);

        oxygenSystem = new OxygenSystem();
        Game.leafHarvestPopup = new Obj_GuiLeafHarvestPopup();
    }

    private static void InitGui()
    {


        GameElement.Create<Obj_GuiScrollbar>(100);

        statsPanel = new Obj_GuiStatsPanel(Rendering.camera.screenWidth - 180 - 6, Rendering.camera.screenHeight - 487);


        // Toolbar in basso a destra con innaffiatoio - dropdown verso l'alto, aperta di default
        int bottomToolbarX = Rendering.camera.screenWidth - 46;
        int bottomToolbarY = Rendering.camera.screenHeight - 90;
        toolbarBottom = new Obj_GuiToolbar(bottomToolbarX, bottomToolbarY, buttonSize: 36, hasDropdown: true, dropUp: true, startOpen: true);
        toolbarBottom.depth = -600;
        toolbarBottom.ButtonFillColor = new Color(140, 140, 160, 255);
        toolbarBottom.ButtonHoverColor = new Color(170, 170, 190, 255);
        toolbarBottom.ButtonPressedColor = new Color(120, 120, 140, 255);
        toolbarBottom.ShowMenuButton = false;
        toolbarBottom.SetIcons(
            AssetLoader.spriteArrowDown,
            AssetLoader.spriteArrowUp,
            AssetLoader.spriteMenu
        );
        toolbarBottom.AddButton(
            AssetLoader.spriteWateringOff,
            AssetLoader.spriteWateringOn,
            "Innaffiatoio",
            (active) => {
                controller.annaffiatoioAttivo = active;
            }
        );

        toolbarBottom.AddActionButton(
            AssetLoader.spriteSeed1,
            "Riprendi Seme",
            () => {
                if (!SeedRecoverySystem.IsRecovering && !SeedRecoverySystem.IsConfirming
                    && !IsModalitaPiantaggio && guiSeedRecovery != null)
                {
                    guiSeedRecovery.ShowConfirmation();
                }
            }
        );

        toolbarBottom.AddActionButton(
            AssetLoader.spriteSeed2,
            "Posta Giornaliera",
            () => {
                if (guiPostaPopup != null && !guiPostaPopup.IsVisible)
                    guiPostaPopup.Show();
            }
        );

        toolbarBottom.AddActionButton(
            AssetLoader.spriteMenu,
            "Opzioni",
            () => {
                if (guiOpzioniPopup != null && !guiOpzioniPopup.IsVisible)
                    guiOpzioniPopup.Show();
            }
        );

        GameElement.Create<Obj_GuiBottomNavigation>(-600);

        guiPiantaggio = new Obj_GuiPiantaggio();
        guiMorte = new Obj_GuiMorte();
        guiSeedRecovery = new Obj_GuiSeedRecovery();
        guiOpzioniPopup = GameElement.Create<Obj_GuiOpzioniPopup>(-2000);
        guiPostaPopup = GameElement.Create<Obj_GuiPostaPopup>(-2000);
        guiRewardPopup = GameElement.Create<Obj_GuiRewardPopup>(-3000);
        guiPostaBadge = GameElement.Create<Obj_GuiPostaBadge>(-650);
        guiPostaBadge.PostaButtonIndex = 2;
        tutorialSlideshow = GameElement.Create<Obj_GuiTutorialSlideshow>(-5000);

        MailSystem.Load();
        MailSystem.RefreshRecurringMails();
    }

    private static void InitInventory()
    {
        // Background stile legno
        inventoryBackground = new Obj_GuiInventoryBackground();

        inventoryCrates = new Obj_GuiInventoryCrates();

        inventoryCratesBackground = new Obj_GuiInventoryCratesBackground();

        inventoryGrid = new Obj_GuiInventoryGrid();

        seedDetailPanel = new Obj_GuiSeedDetailPanel();

        seedUpgradePanel = new Obj_GuiSeedUpgradePanel();

        guiFusionResultPopup = new Obj_GuiFusionResultPopup();

        itemBoard = new Obj_GuiItemBoard();
        itemSlots = new Obj_GuiItemSlots();

        // Collega il pannello alla griglia per dimensionamento dinamico
        inventoryGrid.detailPanel = seedDetailPanel;

        inventoryGrid.OnSeedSelected = (index) => {
            seedDetailPanel.Toggle(index);
        };
    }

    private static void InitUpgrade()
    {
        upgradePanel = new Obj_GuiUpgradePanel();
    }

    private static void InitComposter()
    {
        compostBackground = new Obj_GuiCompostBackground();

        compostPanel = new Obj_GuiCompostPanel();

        packOpening = new Obj_GuiPackOpeningAnimation();
      
    }

    public static void SetTimer()
    {
        Timer = new Timer(1000); //1000
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    public static void SetTimerSave()
    {
        TimerSave = new Timer(10000); //1000
        TimerSave.Elapsed += OnTimedEvent1;
        TimerSave.AutoReset = true;
        TimerSave.Enabled = true;
    }

    private static void OnTimedEvent1(Object source, ElapsedEventArgs e)
    {
        if (IsModalitaPiantaggio) return;
        GameSave.get().Save();
	}

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        if (isPaused || IsModalitaPiantaggio) return;
        if (SeedRecoverySystem.IsRewinding) return; // Nessun danno/crescita durante il rewind

        pianta.proprieta.AggiornaTutto(
            Phase,
            WeatherManager.GetCurrentWeather(),
            WorldManager.GetCurrentModifiers()
        );
       // Console.WriteLine(pianta.proprieta.GetRiepilogo());
    }

    public static void SetTimerFase()
    {
        TimerFase = new Timer(3600000);
        TimerFase.Elapsed += OnTimedEventFase;
        TimerFase.AutoReset = true;
        TimerFase.Enabled = true;
    }

    private static void OnTimedEventFase(Object source, ElapsedEventArgs e)
    {
        Phase = FaseGiorno.GetCurrentPhase();
    }

    public static void EntraModalitaPiantaggio()
    {
        IsModalitaPiantaggio = true;
        isPaused = true;

        room_main.SetActiveRoom();

        // Nascondi elementi del gioco DOPO SetActiveRoom (che li riattiva)
        pianta.active = false;
        if (statsPanel != null) statsPanel.active = false;
        if (toolbar != null) toolbar.active = false;
        if (toolbarBottom != null) toolbarBottom.active = false;
        if (innaffiatoio != null) innaffiatoio.active = false;

        guiPiantaggio.Mostra();
    }

    public static void EsciModalitaPiantaggio()
    {
        IsModalitaPiantaggio = false;
        isPaused = false;

        guiPiantaggio.Nascondi();

        // Riattiva elementi del gioco
        pianta.active = true;
        if (statsPanel != null) statsPanel.active = true;
        if (toolbar != null) toolbar.active = true;
        if (toolbarBottom != null) toolbarBottom.active = true;
        if (innaffiatoio != null) innaffiatoio.active = true;

        room_main.SetActiveRoom();
    }

    public static void MostraMorte()
    {
        if (guiMorte.active) return; // gia' mostrata
        isPaused = true;
        guiMorte.Mostra();
    }

    public static void OnDeathConfirmed()
    {
        // Il save e' stato gia' persistito con PlantDead=true al momento della
        // morte (vedi GameLogic.AggiornaTutto). Non cancelliamo il file: serve
        // per preservare foglie accumulate, essence, upgrade, ecc. Il flag
        // PlantDead garantisce che al prossimo avvio si entri in piantaggio.
        if (pianta != null)
        {
            pianta.Stats.Salute = 0;
            pianta.Reset();
        }

        if (pendingDeathHarvest != null && pendingDeathHarvest.TotalLeaves > 0 && leafHarvestPopup != null)
        {
            leafHarvestPopup.ShowAfterDeath(pendingDeathHarvest);
        }
        else
        {
            FinalizeDeathReset();
        }
    }

    public static void FinalizeDeathReset()
    {
        pendingDeath = false;
        pendingDeathHarvest = null;

        WorldManager.SetCurrentWorld(WorldType.Terra);
        if (pianta != null) pianta.SetNaturalColors(WorldType.Terra);
        Rendering.camera.position.Y = 0;
        if (controller != null) controller.targetScrollY = 0;

        EntraModalitaPiantaggio();
    }
}

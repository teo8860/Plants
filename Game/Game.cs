
using Raylib_CSharp;
using Raylib_CSharp.Windowing;
using Raylib_CSharp.Colors;
using System;
using System.Timers;

namespace Plants;

public static class Game
{
    public static Room mainRoom;
    public static Room inventoryRoom;
    public static Room optionMenu;

    public static Controller controller;
    public static Water innaffiatoio;
    public static Plant pianta;

    public static GuiToolbar toolbar;

    public static Background background;
    public static Ground ground;
    public static WeatherRender weatherSystem;

    public static bool cambiaPhase = false;

    public static Timer Timer;
    public static Timer TimerFase;
    public static DayPhase Phase;

    public static GuiStatsPanel statsPanel;
    public static OxygenSystem oxygenSystem;

    public static Tutorial tutorial;

    public static Color colore1;
    public static Color colore2;

    public static GuiWorldTransition worldTransition;

    public static void Init()
    {
        Inventario.get().Load();

        mainRoom = new Room();
        inventoryRoom = new Room(false);
        optionMenu = new Room(false);

        tutorial = GameElement.Create<Tutorial>(-1);
        tutorial.isTutorialActive = false;

        AssetLoader.LoadAll();

        background = GameElement.Create<Background>(100);
        ground = GameElement.Create<Ground>(100);

        innaffiatoio = GameElement.Create<Water>(-100);
        innaffiatoio.Initialize(GameProperties.cameraWidth, GameProperties.cameraHeight);

        weatherSystem = new WeatherRender();

        controller = new Controller();
        pianta = GameElement.Create<Plant>(-2);

        colore1 = Color.FromHSV(130, 0.45f, 0.68f);
        colore2 = Color.FromHSV(133, 0.47f, 0.44f);
        pianta.setColori(colore1, colore2);

        GameElement.Create<GuiScrollbar>(100);

        worldTransition = GameElement.Create<GuiWorldTransition>(-200);

        InitToolbar();
        InitInventory();

        statsPanel = new GuiStatsPanel(Rendering.camera.screenWidth - 143, Rendering.camera.screenHeight - 487);
        oxygenSystem = new OxygenSystem();

        SetTimer();
        SetTimerFase();
        Phase = FaseGiorno.GetCurrentPhase();
        WorldManager.SetCurrentWorld(WorldType.Terra);
    }

    private static void InitToolbar()
    {
        toolbar = new GuiToolbar(10, 5, buttonSize: 36, spacing: 4);
        toolbar.depth = -50;

        toolbar.SetIcons(
            AssetLoader.spriteArrowDown,
            AssetLoader.spriteArrowUp,
            AssetLoader.spriteMenu
        );

        toolbar.AddButton(
            AssetLoader.spriteWaterOff,
            AssetLoader.spriteWaterOn,
            "Annaffiatoio",
            (active) => {
                controller.annaffiatoioAttivo = active;
            },
            false
        );

        toolbar.AddActionButton(
            AssetLoader.spritePhaseOff,
            "Cambia Fase",
            () => {
                cambiaPhase = true;
            }
        );

        toolbar.AddActionButton(
            AssetLoader.spriteWeatherOff,
            "Cambia Meteo",
            () => {
                WeatherManager.ForceWeatherChange();
            }
        );

        toolbar.AddActionButton(
            AssetLoader.spriteWorldIcon,
            "Cambia Mondo",
            () => {
                WorldManager.SetNextWorld();
            }
        );
    }

    public static void InitInventory()
    {
        toolbar = new GuiToolbar(10, 5, buttonSize: 36, spacing: 4);
        toolbar.depth = -50;
        toolbar.roomId = Game.inventoryRoom.id;
        toolbar.active = true;

        var inv = Inventario.get();
       

		 /*

            ______/``'``'-.
           (_   6  \    .^
         __ `'.__,  |    `'-.
        /_ \  /    /      :`^'
      /`/_` \/    /       .'
      "/  `'-     |.-'`^. `.
      / .`-._     \   `'^^^
    /`/'    \      \
    ""       \      `.
              `\      `.
                `\/     \-'-.-
                 /     /`.  `-.
                (    /'   )  .^
                 \  \\  .'^. `.
                  \ > >  `` `. )
                  // /       .`
                /`/
                ""
		 */
		foreach(var item in  inv.seeds)
		{
			toolbar.AddActionButton(
            AssetLoader.spritePhaseOff,
            "",
            () => {
              
            }
        );
		}
		
    }

    public static void SetTimer()
    {
        Timer = new Timer(500); //1000
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        pianta.proprieta.AggiornaTutto(
            Phase,
            WeatherManager.GetCurrentWeather(),
            WorldManager.GetCurrentModifiers()
        );
        Console.WriteLine(pianta.proprieta.GetRiepilogo());
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
}

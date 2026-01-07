using Raylib_CSharp;
using Raylib_CSharp.Windowing;
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

    public static void Init()
    {
        tutorial = GameElement.Create<Tutorial>(-1);
        tutorial.isTutorialActive = true;

        AssetLoader.LoadAll();

        mainRoom = new Room();
        inventoryRoom = new Room(false);
        optionMenu = new Room(false);

        GameElement.Create<Test>(0, inventoryRoom);

        background = GameElement.Create<Background>(100);
        ground = GameElement.Create<Ground>(100);

        innaffiatoio = GameElement.Create<Water>(-100);
        innaffiatoio.Initialize(GameProperties.windowWidth, GameProperties.windowHeight);

        weatherSystem = new WeatherRender();
        weatherSystem.Initialize(Window.GetScreenWidth(), Window.GetScreenHeight());

        controller = new Controller();
        pianta = GameElement.Create<Plant>(-2);

        GameElement.Create<GuiScrollbar>(100);

        InitToolbar();

        statsPanel = new GuiStatsPanel(GameProperties.windowWidth - 143, GameProperties.windowHeight - 487);
        oxygenSystem = new OxygenSystem();

        SetTimer();
        SetTimerFase();
        Phase = FaseGiorno.GetCurrentPhase();
        WorldManager.SetCurrentWorld(WorldType.Terra);

    }

    private static void InitToolbar()
    {
        toolbar = new GuiToolbar(40, 10, buttonSize: 36, spacing: 4);
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

    public static void SetTimer()
    {
        Timer = new Timer(1000);
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
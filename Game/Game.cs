using Raylib_CSharp;
using Raylib_CSharp.Windowing;
using System;
using System.Timers;

namespace Plants;


public static class Game
{
    public static Controller controller;
    public static Water innaffiatoio;
    public static Plant pianta;

    public static GuiButton gui_annaffia;
    public static GuiButton gui_cambiameteo;
    
    public static Background background;
    public static Ground ground;
    public static WeatherRender weatherSystem;

    public static bool cambiaPhase = false;

    public static Timer Timer;

    public static Timer TimerFase;

    public static DayPhase Phase;

    public static GuiStatsPanel statsPanel;
    public static GuiButton gui_cambiaMondo;

    public static OxygenSystem oxygenSystem;


    public static void Init()
    {
        AssetLoader.LoadAll();

        background = GameElement.Create<Background>(100);
        ground = GameElement.Create<Ground>(100);

        innaffiatoio = GameElement.Create<Water>(-100);
        innaffiatoio.Initialize(GameProperties.windowWidth, GameProperties.windowHeight);


        weatherSystem = new WeatherRender();
        weatherSystem.Initialize(Window.GetScreenWidth(), Window.GetScreenHeight());

        controller = new Controller();

        pianta = new Plant();

       GameElement.Create<GuiScrollbar>(100);



        gui_annaffia = new GuiButton(
            x: 40,
            y: 10,
            width: 125,
            height: 30,
            text: "Annaffiatoio",
            OnClick: () => Game.controller.annaffiatoioAttivo = !Game.controller.annaffiatoioAttivo,
            mark: true
        );

        gui_cambiameteo = new GuiButton(
            x: 40,
            y: 50,
            width: 125,
            height: 30,
            text: "Cambia fase",
            OnClick: () => cambiaPhase = true,
            mark: false
        );

        gui_cambiameteo = new GuiButton(
            x: 40,
            y: 90,
            width: 125,
            height: 30,
            text: "Cambia meteo",
            OnClick: () => WeatherManager.ForceWeatherChange(), //MeteoManager.SetWeather(Weather.Snowy)
            mark: false
        );

        statsPanel = new GuiStatsPanel(GameProperties.windowWidth - 143, GameProperties.windowHeight - 487);

        gui_cambiaMondo = new GuiButton(
            x: 40,
            y: 130,
            width: 125,
            height: 30,
            text: "Cambia mondo",
            OnClick: () =>
            {
                var mondo = WorldManager.GetCurrentWorld();
                mondo = mondo == WorldType.Terra ? WorldType.Luna : WorldType.Terra;
                WorldManager.SetCurrentWorld(mondo);
            },
            mark: false
        );

        oxygenSystem = new OxygenSystem();

        SetTimer();

        SetTimerFase();
        Phase = FaseGiorno.GetCurrentPhase();
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
        Game.pianta.proprieta.AggiornaTutto(Game.Phase, WeatherManager.GetCurrentWeather(),
           WorldManager.GetCurrentModifiers()); // da impostare la temperatura come parametro per ora fissa
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

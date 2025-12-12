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

    public static GuiBar gui_idratazione;
    public static GuiBar gui_temperatura;
    public static GuiBar gui_cibo;

    public static GuiButton gui_annaffia;
    public static GuiButton gui_cambiameteo;
    
    public static Background background;
    public static Ground ground;
    public static WeatherSystem weatherSystem;

    public static bool cambiaPhase = false;

    public static Timer Timer;

    public static Timer TimerFase;

    public static DayPhase Phase;

    public static void Init()
    {
        AssetLoader.LoadAll();

        background = GameElement.Create<Background>(100);
        ground = GameElement.Create<Ground>(100);

        innaffiatoio = GameElement.Create<Water>(-100);
        innaffiatoio.Initialize(GameProperties.windowWidth, GameProperties.windowHeight);


        weatherSystem = new WeatherSystem();
        weatherSystem.Initialize(Window.GetScreenWidth(), Window.GetScreenHeight());

        controller = new Controller();

        pianta = new Plant();

       GameElement.Create<GuiScrollbar>(100);

        gui_idratazione = new GuiBar(
            x: GameProperties.windowWidth-30,
            y: 5,
            width: 15,
            height: 90,
            Active: true
        );

        gui_temperatura = new GuiBar(
            x: GameProperties.windowWidth-55,
            y: 5,
            width: 15,
            height: 90,
            Active: false
        );

        gui_cibo = new GuiBar(
            x: GameProperties.windowWidth-80,
            y: 5,
            width: 15,
            height: 90,
            Active: true
        );

        gui_annaffia = new GuiButton(
            x: 40,
            y: 10,
            width: 125,
            height: 30,
            text: "Annaffiatoio",
            OnClick: () => Game.pianta.attivo = !Game.pianta.attivo,
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
            OnClick: () => MeteoManager.ForceWeatherChange(), //MeteoManager.SetWeather(Weather.Snowy)
            mark: false
        );

        SetTimer();

        SetTimerFase();
        Phase = FaseGiorno.GetCurrentPhase();
    }

    public static void RestartIdratazione()
    {
        Game.pianta.Idratazione = 0;
        Game.gui_idratazione.SetValue(0);
    }

    public static void SetIdratazione(float value)
    {
        Game.pianta.Idratazione += value;
        Game.gui_idratazione.SetValue(RayMath.Clamp( Game.pianta.Idratazione, 0.0f, 1.0f));
    }
    public static void SetTimer()
    {
        Timer = new Timer(5000); 
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        if (Game.pianta.Idratazione > 0.0f)
        { 
            Game.SetIdratazione(-0.025f);
            Game.pianta.Annaffia();
        }
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

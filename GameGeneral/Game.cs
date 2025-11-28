using Raylib_CSharp;
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

    public static GuiButton gui_button;


    public static Timer Timer;


    public static void Init()
    {
        AssetLoader.LoadAll();

        innaffiatoio = new Water();
        innaffiatoio.Initialize(GameProperties.screenWidth, GameProperties.screenHeight);

        controller = new Controller();

        pianta = new Plant();

        gui_idratazione = new GuiBar(
            x: GameProperties.screenWidth-30,
            y: 5,
            width: 15,
            height: 90,
            Active: true
        );

        gui_temperatura = new GuiBar(
            x: GameProperties.screenWidth-55,
            y: 5,
            width: 15,
            height: 90,
            Active: false
        );

        gui_cibo = new GuiBar(
            x: GameProperties.screenWidth-80,
            y: 5,
            width: 15,
            height: 90,
            Active: true
        );
        
        gui_button = new GuiButton(
            x: 10,
            y: 10,
            width: 100,
            height: 30,
            text: "true",
            OnClick: ()=> SetIdratazione(0.1f)
        );

        SetTimer();

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

}

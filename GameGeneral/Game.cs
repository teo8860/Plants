using Plants;
using Raylib_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    public static Timer Timer;


    public static void Init()
    {
        innaffiatoio = new Water();
        innaffiatoio.Initialize(GameProperties.screenWidth, GameProperties.screenHeight);

        pianta = new Plant();

        controller = new Controller();

        gui_idratazione = new GuiBar(
            x: 180,
            y: 5,
            width: 15,
            height: 90,
            Active: true
        );

        gui_temperatura = new GuiBar(
            x: 155,
            y: 5,
            width: 15,
            height: 90,
            Active: false
        );

        gui_cibo = new GuiBar(
            x: 130,
            y: 5,
            width: 15,
            height: 90,
            Active: true
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

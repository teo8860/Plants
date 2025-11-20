using Plants;
using Raylib_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public static class Game
{
    public static Controller controller;
    public static Water innaffiatoio;
    public static Plant pianta;

    public static GuiBar gui_idratazione;


    public static void Init()
    {
        innaffiatoio = new Water();
        innaffiatoio.Initialize(GameProperties.screenWidth, GameProperties.screenHeight);

        pianta = new Plant();

        controller = new Controller();

        gui_idratazione = new GuiBar(
            x: 170,
            y: 5,
            width: 15,
            height: 90
        );

    }


    public static void SetIdratazione(float value)
    {
        Game.pianta.Idratazione += 0.01f;
        Game.gui_idratazione.SetValue(RayMath.Clamp( Game.pianta.Idratazione, 0.0f, 1.0f));
    }
}

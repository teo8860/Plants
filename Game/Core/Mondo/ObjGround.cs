using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Shaders;
using System.Numerics;

namespace Plants;



public class ObjGround : GameElement
{

    Color CurrentWorld1, CurrentWorld2, NextWorld1, NextWorld2;

    public override void Update()
    {


    }

    public override void Draw()
    {
        int x1 = 0;
        int y1 = (int)(GameProperties.groundPosition);

        int x2 = GameProperties.cameraWidth;
        int y2 = GameProperties.groundHeight;

        Graphics.DrawRectangle(x1, y1 - y2 + 10, x2, y2, CurrentWorld1);
        Graphics.DrawRectangle(x1, y1 - y2, x2, y2, CurrentWorld2);
       
        int x3 = 0;
        int y3 = (int)(Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier) + GameProperties.groundHeight;

        int x4 = GameProperties.cameraWidth;
        int y4 = GameProperties.groundHeight;

        Graphics.DrawRectangle(x3, y3 - 10, x4, y4, NextWorld1);
        Graphics.DrawRectangle(x3, y3, x4, y4, NextWorld2);

    }

    public void SetGroundWorld(Color color1CurrentWorld, Color color2CurrentWorld, Color color1NextWorld, Color color2NextWorld)
    {
        CurrentWorld1 = color1CurrentWorld;
        CurrentWorld2 = color2CurrentWorld;
        NextWorld1 = color1NextWorld;
        NextWorld2 = color2NextWorld;

    }
}
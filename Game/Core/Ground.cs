using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System.Numerics;

namespace Plants;



public class Ground: GameElement
{

    public override void Update()
    {
        

    }

    public override void Draw()
    { 

    }

    public void SetGroundWorld(Color color1CurrentWorld, Color color2CurrentWorld, Color color1NextWorld, Color color2NextWorld)
    {
        int x1 = 0;
        int y1 = (int)(GameProperties.windowHeight - GameProperties.groundPosition + Game.controller.offsetY);

        int x2 = GameProperties.windowWidth;
        int y2 = (int)(GameProperties.windowHeight + Game.controller.offsetY);

        Graphics.DrawRectangle(x1, y1, x2, y2, color1CurrentWorld);
        Graphics.DrawRectangle(x1, y1 + 10, x2, y2, color2CurrentWorld);

        int x3 = 0;
        int y3 = (int)(GameProperties.windowHeight - GameProperties.groundPosition - (Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier) + Game.controller.offsetY);

        int x4 = GameProperties.windowWidth;
        int y4 = (int)(GameProperties.windowHeight - (Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier) + Game.controller.offsetY);

        Graphics.DrawRectangle(x3, y3 - 340, x4, y4 - 300, color1NextWorld);
        Graphics.DrawRectangle(x3, y3 - 350, x4, y4 - 300, color2NextWorld);

    }
}

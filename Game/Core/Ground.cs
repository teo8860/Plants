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
        int x1 = 0;
        int y1 = (int)(GameProperties.windowHeight - GameProperties.groundPosition + Game.controller.offsetY);

        int x2 = GameProperties.windowWidth;
        int y2 = (int)(GameProperties.windowHeight + Game.controller.offsetY);

        Graphics.DrawRectangle(x1, y1, x2, y2, Color.DarkGreen);
        Graphics.DrawRectangle(x1, y1+10, x2, y2, Color.Brown);
        
    }
}

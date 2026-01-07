using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System.Numerics;

namespace Plants;



public class Test: GameElement
{

    public override void Update()
    {
        

    }

    public override void Draw()
    {
        Graphics.DrawRectangle(10,10, 50, 60, Color.White);
        Graphics.DrawRectangle(80,90, 30, 100, Color.Red);


    }
    
}

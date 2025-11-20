using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System.Numerics;

namespace Plants;



public class Controller: GameElement
{
    public override void Update()
    {
        Vector2 mouse = Input.GetMousePosition();
        
        if(Input.IsMouseButtonDown(MouseButton.Right))
        {
            Game.innaffiatoio.EmitParticle(mouse);

            Game.SetIdratazione(0.01f);
        }
    }

}

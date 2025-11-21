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

            if (Game.pianta.Idratazione <= 1.0f)
                Game.SetIdratazione(0.05f);
            
        }

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            Game.pianta.Reset();
        }

        if (Input.IsKeyDown(KeyboardKey.Down))
        {
            Game.pianta.Scorri(-50); 
        }
        else if (Input.IsKeyDown(KeyboardKey.Up))
        {
            Game.pianta.Scorri(50);
        }
    }
}

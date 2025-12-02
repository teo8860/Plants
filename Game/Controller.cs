using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System.Numerics;

namespace Plants;



public class Controller: GameElement
{
    public float offsetY = 0;

    public override void Update()
    {
        Vector2 mouse = Input.GetMousePosition();
        
        if(Input.IsMouseButtonDown(MouseButton.Right))
        {
           
            Game.innaffiatoio.EmitParticle(mouse);

            if (Game.pianta.Idratazione <= 1.0f && Game.pianta.attivo)
                Game.SetIdratazione(0.05f);

                //Game.pianta.Annaffia();
        }

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            //Game.pianta.Reset();
        }

        if (Input.IsKeyDown(KeyboardKey.Down))
        {
            Scorri(-50); 
        }
        else if (Input.IsKeyDown(KeyboardKey.Up))
        {
            Scorri(50);
        }
    }
    public void Scorri(float delta)
    {
        if (offsetY + delta >= 0)
        {
            offsetY = offsetY + delta;
        }
    }

}

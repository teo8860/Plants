using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System;
using System.Numerics;
using System.Xml.Linq;

namespace Plants;



public class Controller: GameElement
{
    public float offsetY = 0;
    public float scrollMultiply = 0;

    public override void Update()
    {
        if (Game.cambiaPhase)
        {
            Game.Phase = FaseGiorno.ChangeDayPhase();
            Game.cambiaPhase = false;
        }

        Vector2 mouse = Input.GetMousePosition();
        
        if(Input.IsMouseButtonDown(MouseButton.Right))
        {
           
            if (Game.pianta.attivo)
            {
                Game.innaffiatoio.EmitParticle(mouse);
                if (Game.pianta.Idratazione <= 1.0f)
                {
                    Game.SetIdratazione(0.05f);
                }
            }
            //Game.pianta.Annaffia();
        }

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            //Game.pianta.Reset();
        }

        if (Input.IsKeyPressed(KeyboardKey.Down) || Input.IsKeyPressed(KeyboardKey.Up))
        {
            scrollMultiply = 1;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Down))
        {
            scrollMultiply += 0.1f;
            Scorri(-50*scrollMultiply); 
        }
        else if (Input.IsKeyDown(KeyboardKey.Up))
        {
            scrollMultiply += 0.1f;
            Scorri(50*scrollMultiply);
        }
    }


    public void Scorri(float delta)
    {
            offsetY = Math.Clamp(offsetY + delta, 0, 100000);
        
    }
}

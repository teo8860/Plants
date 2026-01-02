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
    public float offsetMinY = 0;
    public int i = 0;
    public float offsetMaxY => Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
    public float scrollMultiply = 0;

    public bool annaffiatoioAttivo = false;
    public bool isButtonRightPressed = false;

    public override void Update()
    {
        i = Game.pianta.puntiSpline.Count - 1;

        if (Game.cambiaPhase)
        {
            Game.Phase = FaseGiorno.ChangeDayPhase();
            Game.cambiaPhase = false;
        }

        Vector2 mouse = Input.GetMousePosition();
        
        if(Input.IsMouseButtonDown(MouseButton.Right))
        {
            isButtonRightPressed = true;
            if (annaffiatoioAttivo)
            {
                Game.innaffiatoio.EmitParticle(mouse);
                Game.pianta.proprieta.Annaffia(0.01f);
                
            }
        }
        if (!Input.IsMouseButtonDown(MouseButton.Right))
        {
            isButtonRightPressed = false;
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
            if (offsetY >= offsetMinY)
            {
                i--;
                scrollMultiply += 0.1f;
                Scorri(-Game.pianta.puntiSpline[i].Y * scrollMultiply);
            }
        }
        else if (Input.IsKeyDown(KeyboardKey.Up))
        {
            if (offsetY <= offsetMaxY)
            {
                i++;
                scrollMultiply += 0.1f;
                Scorri(Game.pianta.puntiSpline[^i].Y * scrollMultiply);
            }
        }
    }


    public void Scorri(float delta)
    {
            offsetY = offsetY + delta;
        
    }
}

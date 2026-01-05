using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System;
using System.Numerics;

public class Controller : GameElement
{
    public float offsetY = 0;
    public float offsetMinY = 0;
    public float offsetMaxY => (Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier);

    public float scrollSpeed = 1000f;  
    public float scrollAcceleration = 5f;  
    private float currentScrollSpeed = 0f;

    public bool annaffiatoioAttivo = false;
    public bool isButtonRightPressed = false;

    public override void Update()
    {
        if (Game.cambiaPhase)
        {
            Game.Phase = FaseGiorno.ChangeDayPhase();
            Game.cambiaPhase = false;
        }

        Vector2 mouse = Input.GetMousePosition();

        if (Input.IsMouseButtonDown(MouseButton.Right))
        {
            isButtonRightPressed = true;
            if (annaffiatoioAttivo)
            {
                Game.innaffiatoio.EmitParticle(mouse);
                Game.pianta.proprieta.Annaffia(0.01f);
            }
        }
        else
        {
            isButtonRightPressed = false;
        }

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            //Game.pianta.Reset();
        }

        float deltaTime = Time.GetFrameTime();

        if (Input.IsKeyDown(KeyboardKey.Down))
        {
            currentScrollSpeed = Math.Min(currentScrollSpeed + scrollAcceleration, scrollSpeed * 3);
            Scorri(-currentScrollSpeed * deltaTime);
        }
        else if (Input.IsKeyDown(KeyboardKey.Up))
        {
            currentScrollSpeed = Math.Min(currentScrollSpeed + scrollAcceleration, scrollSpeed * 3);
            Scorri(currentScrollSpeed * deltaTime);
        }
        else
        {
            currentScrollSpeed = scrollSpeed;
        }

        offsetY = Math.Clamp(offsetY, offsetMinY, offsetMaxY);
    }

    public void Scorri(float delta)
    {
        offsetY += delta;
    }
}
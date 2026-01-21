using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System;
using System.Linq;
using System.Numerics;
using Engine.Tools;

public class Controller : GameElement
{
    public float offsetMinY = 0;
    public float offsetMaxY => (Game.pianta.Stats.EffectiveMaxHeight);

    public float scrollSpeed = 1000f;  
    public float scrollAcceleration = 5f;  
    private float currentScrollSpeed = 0f;

    public bool annaffiatoioAttivo = false;
    public bool isButtonRightPressed = false;

    public bool autoscroll = true;

    public Controller()
    {
        this.persistent = true;
         Rendering.camera.position.Y = 0;
    }

    public override void Update()
    {
        if (Game.cambiaPhase)
        {
            Game.Phase = FaseGiorno.ChangeDayPhase();
            Game.cambiaPhase = false;
        }

        Vector2 mouse = Input.GetMousePosition();
        mouse = CoordinateHelper.ToWorld(mouse);


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

        if (Input.IsKeyDown(KeyboardKey.Down) && Rendering.camera.position.Y > 0)
        {
            currentScrollSpeed = Math.Min(currentScrollSpeed + scrollAcceleration, scrollSpeed * 3);
            Scorri(-currentScrollSpeed * deltaTime);
        }
        else if (Input.IsKeyDown(KeyboardKey.Up) && Rendering.camera.position.Y < (Game.pianta.Stats.EffectiveMaxHeight))
        {
            currentScrollSpeed = Math.Min(currentScrollSpeed + scrollAcceleration, scrollSpeed * 3);
            Scorri(currentScrollSpeed * deltaTime);
        }
        else
        {
            currentScrollSpeed = scrollSpeed;
        }

        if (Input.IsKeyDown(KeyboardKey.Right))
        {
              Rendering.camera.position.Y = Game.pianta.Stats.EffectiveMaxHeight;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Left))
        {
              Rendering.camera.position.Y = 0;
        }

          if (Input.IsKeyDown(KeyboardKey.B))
        {
            Game.inventoryRoom.SetActiveRoom();
            Game.InitInventory();
        }

         
          if (Input.IsKeyDown(KeyboardKey.V))
        {
            Game.mainRoom.SetActiveRoom();
        }

        if (Input.IsKeyDown(KeyboardKey.Space))
        {
            autoscroll = !autoscroll;
        }

    }

    public void Scorri(float delta)
    {
        Rendering.camera.position.Y += delta;
        Rendering.camera.Update();
    }
}
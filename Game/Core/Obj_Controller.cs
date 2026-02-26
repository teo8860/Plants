using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using System;
using System.Linq;
using System.Numerics;

public class Obj_Controller : GameElement
{
    public float offsetMinY = 0;
    public float offsetMaxY => (Game.pianta.Stats.EffectiveMaxHeight);

    public float scrollSpeed = 1000f;
    public float scrollAcceleration = 5f;
    private float currentScrollSpeed = 0f;

    public float targetScrollY = 0f;
    private const float autoscrollLerpSpeed = 10f;

    public bool annaffiatoioAttivo = false;
    public bool isButtonRightPressed = false;

    public bool autoscroll = true;

    public Obj_Controller()
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
   
		mouse = CoordinateHelper.ToWorld(mouse, Rendering.camera.position);
        
        float deltaTime = Time.GetFrameTime();
        WaterSystem.Update(deltaTime);

        if (Game.toolbarBottom != null)
            Game.toolbarBottom.GetButton(0).FillLevel = WaterSystem.FillPercent;

		if (Input.IsMouseButtonDown(MouseButton.Right))
        {
            isButtonRightPressed = true;
            if (annaffiatoioAttivo && WaterSystem.CanWater)
            {
                WaterSystem.Consume(deltaTime);
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

        float wheel = Input.GetMouseWheelMove();
        if (wheel != 0)
        {
            Scorri(wheel * 200f);
        }

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
        else if (wheel == 0)
        {
            currentScrollSpeed = scrollSpeed;
        }

        if (autoscroll)
        {
            float diff = targetScrollY - Rendering.camera.position.Y;
            if (Math.Abs(diff) > 0.1f)
            {
                Rendering.camera.position.Y += diff * autoscrollLerpSpeed * deltaTime;
                Rendering.camera.position.Y = Math.Clamp(Rendering.camera.position.Y, offsetMinY, offsetMaxY);
                Rendering.camera.Update();
            }
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
            Game.room_inventory.SetActiveRoom();
        }

        if (Input.IsKeyDown(KeyboardKey.N) && Game.inventoryCrates != null && Game.inventoryCrates.IsInventoryOpen && Room.GetActiveId() == Game.room_inventory.id)
        {
            Game.inventoryCrates.CloseInventory();
        }

        if (Input.IsKeyDown(KeyboardKey.C))
        {
            Game.room_compost.SetActiveRoom();
        }
         
        if (Input.IsKeyDown(KeyboardKey.V))
        {
            Game.room_main.SetActiveRoom();
        }

        if (Input.IsKeyDown(KeyboardKey.Space))
        {
            autoscroll = !autoscroll;
        }

        if (Input.IsKeyDown(KeyboardKey.G))
        { 
            Game.pianta.proprieta.Annaffia(0.01f);
            
            Game.pianta.proprieta.AggiornaTutto(
                FaseGiorno.GetCurrentPhase(),
                WeatherManager.GetCurrentWeather(),
                WorldManager.GetCurrentModifiers()
            );
        }

        
        if (Input.IsKeyDown(KeyboardKey.K))
        {
            Game.pianta.Stats.Salute = 0;
		}

        // Minigiochi: M = casuale, 1 = cerchio, 2 = tieni
        if (Input.IsKeyPressed(KeyboardKey.M) && !ManagerMinigames.InCorso)
        {
            ManagerMinigames.AvviaCasuale();
        }
        if (Input.IsKeyPressed(KeyboardKey.One) && !ManagerMinigames.InCorso)
        {
            ManagerMinigames.Avvia(TipoMinigioco.Cerchio);
        }
        if (Input.IsKeyPressed(KeyboardKey.Two) && !ManagerMinigames.InCorso)
        {
            ManagerMinigames.Avvia(TipoMinigioco.Tieni);
        }
        if (Input.IsKeyPressed(KeyboardKey.Three) && !ManagerMinigames.InCorso)
        {
            ManagerMinigames.Avvia(TipoMinigioco.Resta);
        }
        if (Input.IsKeyPressed(KeyboardKey.Four) && !ManagerMinigames.InCorso)
        {
            ManagerMinigames.Avvia(TipoMinigioco.Semi);
        }

    }

    public void Scorri(float delta)
    {
        Rendering.camera.position.Y += delta;
        Rendering.camera.position.Y = Math.Clamp(Rendering.camera.position.Y, offsetMinY, offsetMaxY);
        targetScrollY = Rendering.camera.position.Y;
        Rendering.camera.Update();
    }
}
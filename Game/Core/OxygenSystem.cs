using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Colors;
using System;

namespace Plants;

public class OxygenSystem : GameElement
{
    private float oxygenTankLevel = 1.0f;

    public OxygenSystem()
    {
        this.guiLayer = true;
    }


    public override void Update()
    {
        if (WorldManager.GetCurrentWorld() == WorldType.Luna)
        {
            if (Input.IsKeyDown(KeyboardKey.O) && oxygenTankLevel > 0)
            {
                oxygenTankLevel -= 0.01f;
                Game.pianta.Stats.Ossigeno = Math.Min(1.0f,
                    Game.pianta.Stats.Ossigeno + 0.02f);
            }
        }
    }

    public override void Draw()
    {
        if (WorldManager.GetCurrentWorld() == WorldType.Luna)
        {
            Graphics.DrawText($"Tank O2: {oxygenTankLevel:P0}", 10, 200, 12, Color.SkyBlue);
            Graphics.DrawText("[O] Fornisci ossigeno", 10, 215, 10, Color.Gray);
        }
    }
}

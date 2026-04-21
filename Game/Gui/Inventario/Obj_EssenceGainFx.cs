using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Numerics;

namespace Plants;

/// <summary>
/// Popup fluttuante "+N" con icona cristallo che sale e svanisce, mostrato
/// quando il giocatore ottiene essenza (es. sacrificio seme dall'inventario).
/// </summary>
public class Obj_EssenceGainFx : GameElement
{
    private const float LIFETIME = 1.2f;
    private const float RISE_DISTANCE = 34f;

    private static readonly Color essenceColor = new Color(180, 100, 255, 255);

    private float age = 0f;
    private readonly int amount;
    private readonly Vector2 origin;

    public Obj_EssenceGainFx(Vector2 origin, int amount)
    {
        this.roomId = Game.room_inventory.id;
        this.guiLayer = true;
        this.depth = -200;
        this.persistent = false;
        this.origin = origin;
        this.amount = amount;
    }

    public override void Update()
    {
        age += Time.GetFrameTime();
        if (age >= LIFETIME) Destroy();
    }

    public override void Draw()
    {
        float t = Math.Clamp(age / LIFETIME, 0f, 1f);
        float yOff = -t * RISE_DISTANCE;
        // Fade: opaco per meta' vita, poi sfuma
        float alphaF = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
        byte alpha = (byte)(255 * Math.Clamp(alphaF, 0f, 1f));

        string txt = $"+{amount}";
        int x = (int)origin.X;
        int y = (int)(origin.Y + yOff);

        Color textC = new Color(essenceColor.R, essenceColor.G, essenceColor.B, alpha);
        Color shadowC = new Color((byte)0, (byte)0, (byte)0, alpha);

        // Ombra poi testo
        Graphics.DrawText(txt, x + 1, y + 1, 11, shadowC);
        Graphics.DrawText(txt, x, y, 11, textC);

        // Icona cristallo a destra del testo
        int size = 5;
        int iconX = x + txt.Length * 6 + 4;
        int iconY = y + 5;

        Graphics.DrawTriangle(
            new Vector2(iconX, iconY - size),
            new Vector2(iconX - size / 2f, iconY),
            new Vector2(iconX + size / 2f, iconY),
            textC
        );
        Graphics.DrawTriangle(
            new Vector2(iconX, iconY + size),
            new Vector2(iconX - size / 2f, iconY),
            new Vector2(iconX + size / 2f, iconY),
            textC
        );
    }
}

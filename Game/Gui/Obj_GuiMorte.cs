using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

/// <summary>
/// Schermata che appare quando la pianta muore.
/// Mostra un messaggio e al click torna alla selezione del seme.
/// </summary>
public class Obj_GuiMorte : GameElement
{
    private float timer = 0f;
    private float fadeProgress = 0f;
    private bool canClick = false;

    private Color panelBg = new Color(25, 20, 15, 245);
    private Color rosso = new Color(220, 80, 80, 255);
    private Color bianco = new Color(240, 240, 240, 255);
    private Color grigio = new Color(160, 160, 160, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiMorte()
    {
        this.guiLayer = true;
        this.depth = -3000;
        this.persistent = true;
        // roomId speciale per non essere toccato da SetActiveRoom
        this.roomId = uint.MaxValue;
    }

    public void Mostra()
    {
        this.active = true;
        timer = 0f;
        fadeProgress = 0f;
        canClick = false;
    }

    public void Nascondi()
    {
        this.active = false;
    }

    public override void Update()
    {
        if (!active) return;

        float dt = Time.GetFrameTime();
        timer += dt;
        fadeProgress = Math.Min(1f, timer / 1.0f);

        // Permetti click dopo 1.5 secondi
        if (timer > 1.5f)
            canClick = true;

        if (canClick && (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyboardKey.Enter)))
        {
            // Cancella il salvataggio
            GameSave.DeleteSaveFile();

            // Torna al mondo 0
            WorldManager.SetCurrentWorld(WorldType.Terra);
            Game.pianta.SetNaturalColors(WorldType.Terra);

            // Reset camera
            Rendering.camera.position.Y = 0;
            Game.controller.targetScrollY = 0;

            Nascondi();
            Game.EntraModalitaPiantaggio();
        }
    }

    public override void Draw()
    {
        if (!active) return;

        byte alpha = (byte)(220 * fadeProgress);
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, alpha));

        if (fadeProgress < 0.3f) return;

        float textAlpha = Math.Min(1f, (fadeProgress - 0.3f) / 0.4f);

        // Pannello centrale
        int pw = Math.Min(300, sw - 40);
        int ph = 160;
        int px = (sw - pw) / 2;
        int py = (sh - ph) / 2;

        byte panelAlpha = (byte)(245 * textAlpha);
        Graphics.DrawRectangleRounded(
            new Rectangle(px, py, pw, ph), 0.1f, 8,
            new Color(panelBg.R, panelBg.G, panelBg.B, panelAlpha));
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(px, py, pw, ph), 0.1f, 8, 2,
            new Color(rosso.R, rosso.G, rosso.B, panelAlpha));

        // Titolo
        string titolo = "La tua pianta e' morta";
        int titoloW = titolo.Length * 8;
        byte tAlpha = (byte)(255 * textAlpha);
        Graphics.DrawText(titolo, px + (pw - titoloW) / 2, py + 25, 16,
            new Color(rosso.R, rosso.G, rosso.B, tAlpha));

        // Messaggio
        string msg1 = "Dovrai ricominciare";
        int msg1W = msg1.Length * 5;
        Graphics.DrawText(msg1, px + (pw - msg1W) / 2, py + 60, 10,
            new Color(bianco.R, bianco.G, bianco.B, tAlpha));

        string msg2 = "con un nuovo seme.";
        int msg2W = msg2.Length * 5;
        Graphics.DrawText(msg2, px + (pw - msg2W) / 2, py + 78, 10,
            new Color(bianco.R, bianco.G, bianco.B, tAlpha));

        // Hint per continuare
        if (canClick)
        {
            float pulse = (MathF.Sin(timer * 3f) + 1f) * 0.5f;
            byte hintA = (byte)(120 + pulse * 80);
            string hint = "Clicca per continuare";
            int hintW = hint.Length * 5;
            Graphics.DrawText(hint, px + (pw - hintW) / 2, py + 120, 10,
                new Color(grigio.R, grigio.G, grigio.B, hintA));
        }
    }
}

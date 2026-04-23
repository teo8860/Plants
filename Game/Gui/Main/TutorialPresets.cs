using System;
using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;

namespace Plants
{
    /// <summary>
    /// Factory di slide predefinite per il tutorial.
    /// Ogni metodo restituisce una TutorialSlide con icona/preset già configurata.
    /// </summary>
    public static class TutorialPresets
    {
        // Colori usati nei preset di disegno custom
        private static readonly Color BtnOffBg     = new Color(45,  60,  35,  255);
        private static readonly Color BtnOnBg      = new Color(70,  130, 55,  255);
        private static readonly Color BtnBorder    = new Color(110, 160, 85,  255);
        private static readonly Color WaterColor   = new Color(80,  160, 220, 200);
        private static readonly Color TextColor    = new Color(220, 230, 200, 255);
        private static readonly Color SubText      = new Color(170, 185, 150, 255);

        // ── Slides ─────────────────────────────────────────────────

        /// <summary>Innaffiatoio spento — spiega che la pianta ha sete.</summary>
        public static TutorialSlide InnaffiatoioOff(string[] righe) =>
            new TutorialSlide("Innaffiatoio", righe,
                Icon: AssetLoader.spriteWateringOff);

        /// <summary>Innaffiatoio acceso — spiega come si usa.</summary>
        public static TutorialSlide InnaffiatoioOn(string[] righe) =>
            new TutorialSlide("Innaffiatoio", righe,
                Icon: AssetLoader.spriteWateringOn);

        /// <summary>Mostra innaffiatoio spento → acceso (disegno custom a due stati).</summary>
        public static TutorialSlide InnaffiatoioComparison(string[] righe) =>
            new TutorialSlide("Come Innaffiare", righe,
                DrawExtra: (ax, ay) => DrawComparison(ax, ay,
                    AssetLoader.spriteWateringOff,
                    AssetLoader.spriteWateringOn,
                    "OFF", "ON"));

        /// <summary>Foglia dorata — spiega i minigame.</summary>
        public static TutorialSlide FogliaDorata(string[] righe) =>
            new TutorialSlide("Foglie Dorate", righe,
                Icon: AssetLoader.spriteLeaf);

        /// <summary>Slide generica con sprite custom.</summary>
        public static TutorialSlide ConSprite(string titolo, string[] righe, Sprite icon) =>
            new TutorialSlide(titolo, righe, Icon: icon);

        /// <summary>Slide generica con draw custom.</summary>
        public static TutorialSlide ConDisegno(string titolo, string[] righe, Action<int, int> drawExtra) =>
            new TutorialSlide(titolo, righe, DrawExtra: drawExtra);

        // ── Helper di disegno custom ────────────────────────────────

        /// <summary>
        /// Disegna due sprite piccoli affiancati (before/after) con label.
        /// </summary>
        private static void DrawComparison(int ax, int ay, Sprite left, Sprite right, string labelL, string labelR)
        {
            int half    = 24;
            int gap     = 4;
            int totalW  = half * 2 + gap;
            int startX  = ax + (52 - totalW) / 2;

            // Sinistra (OFF)
            DrawMiniButton(startX, ay + 2, half, BtnOffBg, left);
            int lW = TextManager.MeasureText(labelL, 8);
            Graphics.DrawText(labelL, startX + (half - lW) / 2, ay + half + 6, 8, SubText);

            // Freccia
            Graphics.DrawText(">", startX + half + 1, ay + half / 2, 9, TextColor);

            // Destra (ON)
            int rightX = startX + half + gap;
            DrawMiniButton(rightX, ay + 2, half, BtnOnBg, right);
            int rW = TextManager.MeasureText(labelR, 8);
            Graphics.DrawText(labelR, rightX + (half - rW) / 2, ay + half + 6, 8, SubText);
        }

        private static void DrawMiniButton(int x, int y, int size, Color bg, Sprite sprite)
        {
            Graphics.DrawRectangleRounded(new Rectangle(x, y, size, size), 0.2f, 6, bg);
            Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, size, size), 0.2f, 6, 1, BtnBorder);
            float scale = (float)size / Math.Max(sprite.texture.Width, sprite.texture.Height) * 0.7f;
            GameFunctions.DrawSprite(sprite, new Vector2(x + size / 2f, y + size / 2f), 0f, scale);
        }
    }
}

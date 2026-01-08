using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;

namespace Plants
{
    public class Tutorial : GameElement
    {
        public bool isTutorialActive = false;

        public Tutorial()
        {
        }

        public override void Update()
        {
        }

        public override void Draw()
        {
            if (!isTutorialActive) return;
            int GlassWidth = GameProperties.cameraWidth / 5;
            int GlassHeight = GameProperties.cameraHeight / 5;

            Color vetro = Color.SkyBlue;
            vetro.A = 150;

            Color finestra = Color.White;
            finestra.A = 100;

            Graphics.DrawRectangle(0,0,GameProperties.cameraWidth,GameProperties.cameraHeight, vetro);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 1, 0), new Vector2(GlassWidth * 1, GameProperties.cameraHeight), 4 ,finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 2, 0), new Vector2(GlassWidth * 2, GameProperties.cameraHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 3, 0), new Vector2(GlassWidth * 3, GameProperties.cameraHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 4, 0), new Vector2(GlassWidth * 4, GameProperties.cameraHeight), 4, finestra);

            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 1), new Vector2(GameProperties.cameraWidth, GlassHeight * 1), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 2), new Vector2(GameProperties.cameraWidth, GlassHeight * 2), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 3), new Vector2(GameProperties.cameraWidth, GlassHeight * 3), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 4), new Vector2(GameProperties.cameraWidth, GlassHeight * 4), 4, finestra);

        }

    }
}

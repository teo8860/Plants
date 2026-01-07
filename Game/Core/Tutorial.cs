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

        public override void Update()
        {

        }

        public override void Draw()
        {
            int GlassWidth = GameProperties.windowWidth / 5;
            int GlassHeight = GameProperties.windowHeight / 5;

            Color vetro = Color.SkyBlue;
            vetro.A = 150;

            Color finestra = Color.White;
            finestra.A = 100;

            Graphics.DrawRectangle(0,0,GameProperties.windowWidth,GameProperties.windowHeight, vetro);
            Graphics.DrawLineEx(new Vector2(GlassWidth, 0), new Vector2(GlassWidth, GameProperties.windowHeight), 4 ,finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 2, 0), new Vector2(GlassWidth * 2, GameProperties.windowHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 3, 0), new Vector2(GlassWidth * 3, GameProperties.windowHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(GlassWidth * 4, 0), new Vector2(GlassWidth * 4, GameProperties.windowHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight), new Vector2(GameProperties.windowWidth, GlassHeight), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 2), new Vector2(GameProperties.windowWidth, GlassHeight * 2), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 3), new Vector2(GameProperties.windowWidth, GlassHeight * 3), 4, finestra);
            Graphics.DrawLineEx(new Vector2(0, GlassHeight * 4), new Vector2(GameProperties.windowWidth, GlassHeight * 4), 4, finestra);

        }

    }
}

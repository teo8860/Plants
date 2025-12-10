using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;

namespace Plants;

internal class Rendering
{
    private static WeatherSystem weatherSystem;

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        while (true)
        {
            if (Window.ShouldClose())
            {
                Window.Close();
            }

            var elements = GameElement.GetList();
            foreach (var item in elements)
            {
                item.Update();
            }

            elements.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            Graphics.BeginDrawing();

           

            foreach (var item in elements)
            {
                item.Draw();
            }

            Graphics.EndDrawing();
        }
    }

    
}
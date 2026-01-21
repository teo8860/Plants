using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Plants;

internal class Rendering
{
    public static PixelCamera camera = new(GameProperties.windowWidth, GameProperties.windowHeight, (float)GameProperties.windowWidth / (float)GameProperties.viewWidth);
    

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
            
            camera.Update();
            var elements = GameElement.GetList();
            elements = elements.FindAll((o)=> o.active == true);


            foreach (var item in elements)
            {
                item.Update();
            }


            var layerBase = elements.FindAll((o)=> (o.guiLayer == false && o.active == true));
            layerBase.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            var layerGui = elements.FindAll((o)=> (o.guiLayer == true && o.active == true));
            layerGui.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);

            // World rendering phase
            camera.BeginWorldMode();
            foreach (var item in layerBase)
            {
                item.Draw();
            }
            camera.EndWorldMode();

            // Draw world render texture to screen
            camera.DrawWorld();

            // GUI rendering phase
            foreach (var item in layerGui)
            {
                item.Draw();
            }

            Graphics.DrawFPS(0,0);
            Graphics.EndDrawing();
        }
    }

}
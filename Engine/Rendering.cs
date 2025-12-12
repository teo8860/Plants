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
    private static WeatherSystem weatherSystem;
    
    static RenderTexture2D windowView = RenderTexture2D.Load(GameProperties.windowWidth, GameProperties.windowHeight);
    static RenderTexture2D screenView = RenderTexture2D.Load(GameProperties.viewWidth, GameProperties.viewHeight);

    static Camera2D worldSpaceCamera = new();  // Game world camera
    static Camera2D screenSpaceCamera = new(); // Smoothing camera

    

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        screenView.Texture.SetFilter(TextureFilter.Point);
        windowView.Texture.SetFilter(TextureFilter.Point);

        float virtualRatio = GameProperties.windowWidth / GameProperties.viewWidth;
        Rectangle sourceRec = new( 0.0f, 0.0f, screenView.Texture.Width, -screenView.Texture.Height );
        Rectangle destRec = new(-virtualRatio, -virtualRatio, GameProperties.windowWidth + (virtualRatio * 2), GameProperties.windowHeight + (virtualRatio * 2) );

        worldSpaceCamera.Zoom = 1.0f;
        screenSpaceCamera.Zoom = 1.0f;
        screenSpaceCamera.Target = new(0, 0);

        while (true)
        {
            if (Window.ShouldClose())
            {
                Window.Close();
            }


            // Round worldSpace coordinates, keep decimals into screenSpace coordinates
            worldSpaceCamera.Target.X = screenSpaceCamera.Target.X;
            screenSpaceCamera.Target.X -= worldSpaceCamera.Target.X;
            screenSpaceCamera.Target.X *= virtualRatio;

            worldSpaceCamera.Target.Y = screenSpaceCamera.Target.Y;
            screenSpaceCamera.Target.Y -= worldSpaceCamera.Target.Y;
            screenSpaceCamera.Target.Y *= virtualRatio;


            var elements = GameElement.GetList();
            foreach (var item in elements)
            {
                item.Update();
            }

            var layerBase = elements.FindAll((o)=> o.guiLayer == false);
            layerBase.Sort((GameElement a, GameElement b)=> b.depth - a.depth);




            Graphics.BeginTextureMode(windowView);
            Graphics.ClearBackground(Color.Black);
            Graphics.BeginMode2D(worldSpaceCamera);

            foreach (var item in layerBase)
            {
                item.Draw();
            }

            Graphics.EndMode2D();
            Graphics.EndTextureMode();


            Graphics.BeginTextureMode(screenView);
		    Graphics.ClearBackground(Color.Black);
		    Graphics.DrawTexturePro(windowView.Texture,
			    new(0, 0, windowView.Texture.Width, windowView.Texture.Height),
			    new(0, 0, screenView.Texture.Width, screenView.Texture.Height),
			    Vector2.Zero,
			    0.0f,
			    Color.White);
		    Graphics.EndTextureMode();


            
            var layerGui = elements.FindAll((o)=> o.guiLayer == true);
            layerGui.Sort((GameElement a, GameElement b)=> b.depth - a.depth);

            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);
            Graphics.BeginMode2D(screenSpaceCamera);

             Graphics.DrawTexturePro(screenView.Texture,
			    new(0, 0, screenView.Texture.Width, screenView.Texture.Height),
			    new(0, 0, windowView.Texture.Width, windowView.Texture.Height),
			    Vector2.Zero,
			    0.0f,
			    Color.White);

                foreach (var item in layerGui)
                {
                    item.Draw();
                }

            //Graphics.DrawTexturePro(Rendering.gameView.Texture, sourceRec, destRec,Vector2.Zero, 0, Color.White);

            Graphics.EndMode2D();
            Graphics.EndDrawing();
        }
    }

    
}
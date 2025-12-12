using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;
using System;
using System.Numerics;

namespace Plants;

internal class Rendering
{
    private static WeatherSystem weatherSystem;

    static RenderTexture2D gameView = RenderTexture2D.Load(GameProperties.viewWidth, GameProperties.viewHeight);

    static Camera2D worldSpaceCamera = new();  // Game world camera

    static Camera2D screenSpaceCamera = new(); // Smoothing camera

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);

        float virtualRatio = GameProperties.screenWidth / GameProperties.viewWidth;
        Rectangle sourceRec = new( 0.0f, 0.0f, gameView.Texture.Width, -gameView.Texture.Height );
        Rectangle destRec = new(-virtualRatio, -virtualRatio, GameProperties.screenWidth + (virtualRatio * 2), GameProperties.screenHeight + (virtualRatio * 2) );

        worldSpaceCamera.Zoom = 1.0f;
        screenSpaceCamera.Zoom = 1.0f;

        while (true)
        {
            if (Window.ShouldClose())
            {
                Window.Close();
            }

            screenSpaceCamera.Target = new(0, 0);
            
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

            elements.Sort((GameElement a, GameElement b)=> b.depth - a.depth);




            Graphics.BeginTextureMode(gameView);
            Graphics.ClearBackground(Color.Black);
            Graphics.BeginMode2D(worldSpaceCamera);

            foreach (var item in elements)
            {
                item.Draw();
            }

            Graphics.EndMode2D();
            Graphics.EndTextureMode();



            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);
            Graphics.BeginMode2D(screenSpaceCamera);

            Graphics.DrawTexturePro(Rendering.gameView.Texture, sourceRec, destRec,Vector2.Zero, 0, Color.White);

            Graphics.EndMode2D();
            Graphics.EndDrawing();
        }
    }

    
}
using System;
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;

namespace Plants;

public class Obj_Logo : GameElement
{
    private float bgAlpha = 1f;      
    private float logoAlpha = 0f; 
    private bool isFadingOut = false;
    private bool isActive = true;
    private bool logoAppeared = false;

    public Obj_Logo()
    {
        guiLayer = true;
        depth = -2000;
    }

    public override void Update()
    {
        if (!isActive) return;

        float dt = Time.GetFrameTime();

        if (!isFadingOut)
        {
            if (logoAlpha < 1f)
            {
                logoAlpha += dt * 0.8f; 
                if (logoAlpha >= 1f)
                {
                    logoAlpha = 1f;
                    logoAppeared = true;
                }
            }
            
            if (logoAlpha > 0.5f)
            {
                if (Input.GetKeyPressed() != 0 || Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    isFadingOut = true;
                }
            }
        }
        else
        {
            bgAlpha -= dt * 1.5f;
            logoAlpha = bgAlpha;

            if (bgAlpha <= 0f)
            {
                bgAlpha = 0f;
                logoAlpha = 0f;
                isActive = false;
            }
        }
    }

    public override void Draw()
    {
        if (!isActive) return;

        int screenW = Rendering.camera.screenWidth;
        int screenH = Rendering.camera.screenHeight;

        Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, (byte)(255 * bgAlpha)));

        int x = (screenW - AssetLoader.spriteLogo.texture.Width/2);
        int y = (screenH - AssetLoader.spriteLogo.texture.Height) / 2;
            
        Color tint = new Color(255, 255, 255, (byte)(255 * logoAlpha));
      
        GameFunctions.DrawSprite(AssetLoader.spriteLogo, new Vector2(x,y+100), 0, 0, Color.White, logoAlpha);

	}
}
using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.ComponentModel.Design;


namespace Plants;


public class GuiScrollbar: GameElement
{


    public GuiScrollbar()
    {
       
        this.guiLayer = true;
    }


    public override void Update()
    {
        
    }

    public override void Draw()
    {
        int trackTop = Rendering.camera.screenHeight - 10;
        int trackBottom = 10;
        int trackHeight = trackTop - trackBottom;

        float contentHeight = Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;

        float viewportHeight = GameProperties.cameraHeight;

        float thumbRatio = viewportHeight / (contentHeight + viewportHeight);
        int thumbHeight = (int)Math.Clamp(thumbRatio * trackHeight, 20, trackHeight); 

        Color c = Color.Black;
        c.A = 100;

        Graphics.DrawRectangleRounded(
            new Rectangle(10, trackBottom, 10, trackHeight),
            0.5f,
            16,
            c
        );

        float maxCamera = Game.controller.offsetMaxY;
        float currentCamera = Rendering.camera.position.Y;

        float scrollPercent = maxCamera > 0 ? currentCamera / maxCamera : 0;
        scrollPercent = Math.Clamp(scrollPercent, 0f, 1f);

        int scrollableArea = trackHeight - thumbHeight;
        float thumbY = trackBottom + (1f - scrollPercent) * scrollableArea;

        Graphics.DrawRectangleRounded(
            new Rectangle(12, (int)thumbY, 6, thumbHeight),
            0.5f,
            16,
            Color.White
        );
    }
}


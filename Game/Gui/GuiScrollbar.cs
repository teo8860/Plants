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
        int bottom = GameProperties.windowHeight-20;

        Color c = Color.Black;
        c.A = 100;

        Graphics.DrawRectangleRounded(
             new Rectangle(10, 10, 10, bottom),
             0.5f,
             16,
             c
         );


          Graphics.DrawRectangleRounded(
             new Rectangle(12, bottom-(Game.controller.offsetY/100)-10, 6, 20),
             0.5f,
             16,
             Color.White
         );
    }
}


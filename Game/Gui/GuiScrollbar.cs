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
       
    }


    public override void Update()
    {
        
    }

    public override void Draw()
    {
        int bottom = GameProperties.screenHeight-20;

        Graphics.DrawRectangleRounded(
             new Rectangle(10, 10, 10, bottom),
             0.5f,
             16,
             Color.Black
         );


          Graphics.DrawRectangleRounded(
             new Rectangle(11, bottom-(Game.controller.offsetY/100)-10, 7, 20),
             0.5f,
             16,
             Color.DarkGray
         );
    }
}


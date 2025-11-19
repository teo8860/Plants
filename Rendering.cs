using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


internal class Rendering
{
    public static void Init()
    {
		Window.Init(200, 400, "Hello World");
	
		while(true)
		{
			if(Window.ShouldClose())
			{
				Window.SetState(ConfigFlags.HiddenWindow);
			}

			Graphics.BeginDrawing();
			Graphics.ClearBackground(Color.White);

			Graphics.DrawText("Hello, world!", 12, 12, 20, Color.Black);

			Graphics.EndDrawing();
		}
	}
}

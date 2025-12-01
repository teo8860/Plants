using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;


namespace Plants;


internal class Rendering
{

    public static void Init()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);
        Time.SetTargetFPS(60);


        while (true)
		{        
            if (Window.ShouldClose())
			{
                Window.Close();
				//Window.SetState(ConfigFlags.HiddenWindow);
            }


            var elements = GameElement.GetList();

            foreach (var item in elements)
            {
                item.Update();
            }

           

            Graphics.BeginDrawing();

            DayPhase currentPhase = FaseGiorno.GetCurrentPhase();

            switch (currentPhase)
            {
                case DayPhase.Night:
                    Graphics.ClearBackground(new Color(15, 15, 35, 255));
                    break;
                case DayPhase.Dawn:
                    Graphics.ClearBackground(new Color(255, 160, 122, 255));
                    break;
                case DayPhase.Morning:
                    Graphics.ClearBackground(new Color(135, 206, 235, 255));
                    break;
                case DayPhase.Afternoon:
                    Graphics.ClearBackground(new Color(100, 149, 237, 255));
                    break;
                case DayPhase.Dusk:
                    Graphics.ClearBackground(new Color(255, 140, 105, 255));
                    break;
                case DayPhase.Evening:
                    Graphics.ClearBackground(new Color(25, 25, 60, 255));
                    break;
            }


            foreach (var item in elements)
            {
                item.Draw();
            }


            Graphics.EndDrawing();
		}
	}
}

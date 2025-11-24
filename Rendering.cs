using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using Raylib_CSharp;


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
			Graphics.ClearBackground(Color.White);
            

            foreach (var item in elements)
            {
                item.Draw();
            }


            Graphics.EndDrawing();
		}
	}
}

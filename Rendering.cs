using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using Raylib_CSharp.Collision;
using Raylib_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Plants;


internal class Rendering
{
    private static indicator _healthGauge;
    private static float _currentValue = 0.0f;

    public static void Init()
    {
        const int screenWidth = 200;
        const int screenHeight = 400;
        const float pointRadius = 10.0f;

        _healthGauge = new indicator(
            x: 170,
            y: 5,
            width: 15,
            height: 90
        );

        Window.Init(screenWidth, screenHeight, "Plants");
        Water.Initialize(screenWidth, screenHeight);
        Raylib.SetConfigFlags(ConfigFlags.Msaa4XHint);

        Time.SetTargetFPS(60);

        while (true)
		{        

            if (Window.ShouldClose())
			{
				Window.SetState(ConfigFlags.HiddenWindow);
            }

            Vector2 mouse = Input.GetMousePosition();

            if (Input.IsMouseButtonDown(MouseButton.Right))
            {
                Water.EmitParticle(mouse);
                _currentValue += 0.005f;
            }

            Water.Update();

            if (_currentValue > 1.0f) _currentValue = 1.0f;

            _healthGauge.Update(_currentValue);

            Graphics.BeginDrawing();
			Graphics.ClearBackground(Color.White);

            Water.Draw();

            _healthGauge.Draw();

            Graphics.EndDrawing();
		}
	}
}

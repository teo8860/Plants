using Raylib_CSharp;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public class Obj_Seed : GameElement
{
	Seed dati;
	Vector3 color = new Vector3( 0.0f, 1.0f, 1.0f ); 
	double time = 0;

	int un_time = AssetLoader.shaderSeed.GetLocation("time");
	int un_color = AssetLoader.shaderSeed.GetLocation("color");
	int un_noise = AssetLoader.shaderSeed.GetLocation("noise");


	public Obj_Seed(Seed dati)
	{
		this.dati = dati;
		this.guiLayer = true;
	}

	public override void Update()
	{
		color.Y = 0.6f;
		time += 0.1;
	}

	public override void Draw()
	{

        Graphics.BeginShaderMode(AssetLoader.shaderSeed);
		
			AssetLoader.shaderSeed.SetValue(un_time, (float)Time.GetTime()*2, ShaderUniformDataType.Float);
			AssetLoader.shaderSeed.SetValue(un_color, color, ShaderUniformDataType.Vec3);
			AssetLoader.shaderSeed.SetValueTexture(un_noise, AssetLoader.spriteNoise1.texture);

            GameFunctions.DrawSprite(AssetLoader.spriteSeed1, new Vector2(30,100),0,8);

       Graphics.EndShaderMode();
	}
}

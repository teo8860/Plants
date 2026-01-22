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
	public Seed dati;
	public Vector2 position = new Vector2(0,0);
	public float scale = 8;
	public Vector3 color = new Vector3( 0.0f, 1.0f, 1.0f ); 
	double time = 0;

	int un_time = AssetLoader.shaderSeed.GetLocation("time");
	int un_color = AssetLoader.shaderSeed.GetLocation("color");
	int un_noise = AssetLoader.shaderSeed.GetLocation("noise");

	public Obj_Seed()
	{
		this.guiLayer = true;
	}
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

            GameFunctions.DrawSprite(AssetLoader.spriteSeed1, position,0,scale);

       Graphics.EndShaderMode();
	}
}

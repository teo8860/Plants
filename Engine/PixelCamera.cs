using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using System.Numerics;


namespace Plants;

public class PixelCamera
{
	public Vector2 offset;
	public Vector2 position;
	public float rotation;
	public float windowZoom;
	public float zoom;
	public int screenWidth;
	public int screenHeight;
	public float virtualRatio;

	public Vector2 view => new Vector2(renderTexture.Texture.Width/zoom, renderTexture.Texture.Height/zoom);

	private Color clearColor;
	private Camera2D worldSpaceCamera;
	private Camera2D screenSpaceCamera;
	private RenderTexture2D renderTexture;

	public PixelCamera(int screenWidth, int screenHeight, float virtualRatio)
	{
		offset = new Vector2(0.0f, 0.0f);
		position = new Vector2(0.0f, 0.0f);
		rotation = 0.0f;
		windowZoom = 1.0f;
		zoom = 0.5f;

		this.screenWidth = screenWidth;
		this.screenHeight = screenHeight;
		this.virtualRatio = virtualRatio;

		clearColor = new Color(10, 10, 10, 255);

		worldSpaceCamera = new Camera2D();
		worldSpaceCamera.Zoom = 1.0f;

		screenSpaceCamera = new Camera2D();
		screenSpaceCamera.Zoom = 1.0f;

		renderTexture =  RenderTexture2D.Load(
			(int)(screenWidth / virtualRatio) + 2,
			(int)(screenHeight / virtualRatio) + 2
		);

		renderTexture.Texture.SetFilter(TextureFilter.Point);
	}

	public void BeginWorldMode()
	{
		Graphics.BeginTextureMode(renderTexture);

		Graphics.BeginMode2D(worldSpaceCamera);
		Graphics.ClearBackground(clearColor);
	}

	public void BeginScreenMode()
	{
		Graphics.BeginMode2D(screenSpaceCamera);
	}

	public void EndWorldMode()
	{
		Graphics.EndMode2D();
		Graphics.EndTextureMode();
	}

	public void DrawWorld()
	{
		Graphics.BeginMode2D(screenSpaceCamera);

		Graphics.DrawTexturePro(
			renderTexture.Texture,
			new Raylib_CSharp.Transformations.Rectangle(0, 0, renderTexture.Texture.Width, renderTexture.Texture.Height),
			new Raylib_CSharp.Transformations.Rectangle(-virtualRatio, -virtualRatio,
				screenWidth + (virtualRatio * 2),
				screenHeight + (virtualRatio * 2)
			),
			Vector2.Zero,
			0.0f,
			Color.White
		);

		Graphics.EndMode2D();
	}

	public void SetClearColor(Color color)
	{
		clearColor = color;
	}

	public void Update()
	{
		screenSpaceCamera.Offset = offset;
		screenSpaceCamera.Target = position;
		screenSpaceCamera.Rotation = rotation;
		screenSpaceCamera.Zoom = windowZoom;

		worldSpaceCamera.Target = new Vector2((int)screenSpaceCamera.Target.X,  (int)screenSpaceCamera.Target.Y);
		worldSpaceCamera.Zoom = zoom;

		screenSpaceCamera.Target = new Vector2(
			(screenSpaceCamera.Target.X - worldSpaceCamera.Target.X) * virtualRatio,
			(screenSpaceCamera.Target.Y - worldSpaceCamera.Target.Y) * virtualRatio
		);
	}
}

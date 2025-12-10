using Raylib_CSharp.Images;
using Raylib_CSharp.Textures;
using System;
using System.Numerics;

namespace Plants;

public class Sprite
{
    public readonly Texture2D texture;
    public readonly  Vector2 origin;

    public Sprite(string fileName, float size = 1, Vector2 origin = default)
    {
        try
        {
            Image image = Utility.LoadImageFromEmbedded(fileName, "Assets");
            image.Resize((int)(image.Width*size), (int)(image.Height*size));
            this.texture = Texture2D.LoadFromImage(image);
            this.origin = new Vector2(texture.Width*origin.X, texture.Height*origin.Y);

            image.Unload();
        }
        catch
        {
            Console.WriteLine("Impossibile caricare texture foglia. Assicurati che il percorso sia corretto.");
        }
    }



}
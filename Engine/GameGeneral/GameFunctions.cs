using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public static class GameFunctions
{
    
    public static void DrawSprite(Sprite sprite, Vector2 position, float angle = 0.0f, float scale = 1.0f)
    {
       GameFunctions.DrawSprite(sprite, position, angle, new Vector2(scale, scale));
    }

     public static void DrawSprite(Sprite sprite, Vector2 position, float angle = 0.0f, Vector2 scale = default)
     {
        if(scale == default)
            scale = new Vector2(1.0f, 1.0f);

        Vector2 size = new Vector2(sprite.texture.Width * scale.X, sprite.texture.Height * scale.Y);

        Rectangle source = new Rectangle(0, 0, sprite.texture.Width, sprite.texture.Height);
        Rectangle dest = new Rectangle(position.X, position.Y, size.X, size.Y);

        Graphics.DrawTexturePro(sprite.texture, source, dest, sprite.origin, angle, Color.White);
     }
}

using Plants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public static class AssetLoader
{
    public static Sprite spriteLeaf;

    public static void LoadAll()
    {
        spriteLeaf = new Sprite("leaf.png", 0.05f, new(0.5f, 0.0f));
    }
}

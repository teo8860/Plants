
using RayImg = Raylib_CSharp.Images;
using System.Drawing;
using System.Reflection;
using System;
using System.IO;

namespace Plants;


internal class Utility
{
    public static Icon LoadIconFromEmbedded(string resourceName, string path = "")
    {
       var asm = Assembly.GetExecutingAssembly();

       // Normalize path: replace / with . for .NET resource naming convention
       string normalizedPath = path.Replace("/", ".");
       string normalizedName = resourceName.Replace("/", ".");
       using var stream = asm.GetManifestResourceStream("Plants."+normalizedPath+"."+normalizedName);
       if (stream == null)
       {
            return new Icon(resourceName);
       }

       return new Icon(stream);
    }

    public static void PrintAssets()
    {
       var asm = Assembly.GetExecutingAssembly();
    
       foreach (var name in asm.GetManifestResourceNames())
            Console.WriteLine("Resource: " + name);
      }
    
    public static RayImg.Image LoadImageFromEmbedded(string resourceName, string path = "")
    {
       var asm = Assembly.GetExecutingAssembly();

       // Normalize path: replace / with . for .NET resource naming convention
       string normalizedPath = path.Replace("/", ".");
       string normalizedName = resourceName.Replace("/", ".");
       using var stream = asm.GetManifestResourceStream("Plants."+normalizedPath+"."+normalizedName);
       if (stream == null)
       {
            return RayImg.Image.Load(Path.Combine(path, resourceName));
       }

        using MemoryStream ms = new();
        stream.CopyTo(ms);

        return RayImg.Image.LoadFromMemory(".png", ms.ToArray());
    }
    
    
    public static string LoadTextFromEmbedded(string resourceName, string path = "")
    {
       var asm = Assembly.GetExecutingAssembly();

       // Normalize path: replace / with . for .NET resource naming convention
       string normalizedPath = path.Replace("/", ".");
       string normalizedName = resourceName.Replace("/", ".");
       using var stream = asm.GetManifestResourceStream("Plants."+normalizedPath+"."+normalizedName);
       if (stream != null)
		{
			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		// Se non esiste come risorsa incorporata, prova a leggere dal filesystem
		return File.ReadAllText(Path.Combine(path, resourceName));
    }
}

using System;
using System.Drawing;
using System.Reflection;

namespace Plants;


internal class Utility
{
    public static Icon LoadIconFromEmbedded(string resourceName, string path = "")
    {
       var asm = Assembly.GetExecutingAssembly();

       foreach (var name in asm.GetManifestResourceNames())
            Console.WriteLine("Resource: " + name);

       using var stream = asm.GetManifestResourceStream("Plants."+path+"."+resourceName);
       if (stream == null)
       {
            return new Icon(resourceName);
       }

       return new Icon(stream);
    }

}

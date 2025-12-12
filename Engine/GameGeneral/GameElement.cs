using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public class GameElement
{
    public static List<GameElement> elementList = new();

    public int depth = 0;
    public bool guiLayer = false;

    public static List<GameElement> GetList()
    {
        return elementList;
    }

    public static T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T >(int depth = 0) where T : GameElement
    {
        T obj =  Activator.CreateInstance<T>();
        obj.depth = depth;
        return obj;
    }

    public GameElement()
    {
        elementList.Add(this);
    }

    ~GameElement()
    {
        elementList.Remove(this);
    }

    public virtual void Update()
    {

    }

    public virtual void Draw()
    {

    }
}

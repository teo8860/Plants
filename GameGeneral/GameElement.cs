using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public class GameElement
{
    public static List<GameElement> elementList = new();

    public static List<GameElement> GetList()
    {
        return elementList;
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

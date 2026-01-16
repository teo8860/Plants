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

    public bool persistent = false;
    public bool active = true;
    public int depth = 0;
    public bool guiLayer = false;

    
    private uint _roomId;
    public uint roomId 
    {
        get 
        {
            return _roomId;
        }
        set{
           if(Room.GetActiveId() != value)
            {
                active = false;
                _roomId = value;
            }
        }
    }
   

    public static T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T >(int depth = 0, Room room = null) where T : GameElement
    {
        T obj =  Activator.CreateInstance<T>();

        if(depth != 0)
            obj.depth = depth;

        if(room != null)
        {
            obj.roomId = room.id;

            if(Room.GetActiveId() != room.id)
            {
                obj.active = false;
            }
        }

        return obj;
    }

	public static List<GameElement> GetList()
    {
        return elementList;
    }

    public GameElement()
    {
        GameElement.elementList.Add(this);
        roomId = Room.GetActiveId();
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plants;


public class GameElement
{
    
	public static List<GameElement> elementList = new();
	private static readonly object _elementLock = new();
    
    // Static constructor to ensure proper initialization
    static GameElement()
    {
        // Force initialization of static fields
        if (elementList == null)
            elementList = new List<GameElement>();
    }

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
        T obj = Activator.CreateInstance<T>();

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
        try
        {
            if (_elementLock == null || elementList == null)
                return new List<GameElement>();
                
            lock (_elementLock)
            {
                return new List<GameElement>(elementList);
            }
        }
        catch
        {
            // Defensive fallback
            return new List<GameElement>();
        }
    }

    public GameElement()
    {
        lock (_elementLock)
        {
            if (elementList == null)
                elementList = new List<GameElement>();
            GameElement.elementList.Add(this);
        }
        
        try
        {
            roomId = Room.GetActiveId();
        }
        catch
        {
            _roomId = 0;
        }
    }

    ~GameElement()
    {
        // Finalizer runs on GC thread - be extra safe
        try
        {
            lock (_elementLock)
            {
                elementList?.Remove(this);
            }
        }
        catch
        {
            // Best effort - list may already be cleared
        }
    }

    public void Destroy()
    {
        try
        {
            lock (_elementLock)
            {
                elementList?.Remove(this);
            }
        }
        catch
        {
            // Best effort
        }
    }

    public virtual void Update()
    {

    }

    public virtual void Draw()
    {

    }
}

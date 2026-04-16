using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

public class Room
{
	public static List<Room> list = new List<Room>();
	protected static Room activeRoom;
	protected static uint roomId = 0;

	public string name = "";
	public uint id = 0;

	public Room(bool isNewActive = true)
	{
		this.id = Room.roomId;

		Room.roomId += 1;
		Room.list.Add(this);

		if(isNewActive)
		{
			this.SetActiveRoom();
		}
	}

	public void Create()
	{
		
	}

	public static bool IsActive()
	{
		return activeRoom != null;
	}

	public static uint GetActiveId()
	{
		return Room.activeRoom?.id ?? 0;
	}

	public void SetActiveRoom()
	{
		Room.activeRoom = this;

		Room.list.ForEach(o=>
		{
			if(o.id != this.id)
			{
				o.Deactivate();
			}
			else
			{
				o.Activate();
			}
		});
	}

	private void Activate()
	{
		var elements = GameElement.GetList();  // Returns a safe copy with lock
		foreach(var item in elements)
		{
			if(item.roomId == this.id)
			{
				item.active = true;
			}
		}
	}

	private void Deactivate()
	{
		var elements = GameElement.GetList();  // Returns a safe copy with lock
		foreach(var item in elements)
		{
			if(item.roomId == this.id)
			{
				if(item.persistent == false)
					item.active = false;
			}
		}
	}

	~Room()
    {
		// Finalizer runs on GC thread - be safe
		try
		{
			Room.list.Remove(this);

			var elements = GameElement.GetList();
			foreach(var element in elements)
			{
				if(element.roomId == this.id)
				{
					element.Destroy();
				}
			}

			if(Room.activeRoom == this)
			{
				Room.activeRoom = Room.list.LastOrDefault();
				Room.activeRoom?.Activate();
			}
		}
		catch
		{
			// Best effort during shutdown
		}
	}
}

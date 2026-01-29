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
		return Room.activeRoom.id;
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
		for(int i=GameElement.elementList.Count-1; i>-1; i--)
		{
			var item = GameElement.elementList[i];
			if(item.roomId == this.id)
			{
				item.active = true;
			}
		}
	}

	private void Deactivate()
	{
		for(int i=GameElement.elementList.Count-1; i>-1; i--)
		{
			var item = GameElement.elementList[i];
		
			if(item.roomId == this.id)
			{
				if(item.persistent == false)
				item.active = false;
			}
		}
	}

	~Room()
    {
        Room.list.Remove(this);

		for(int i=GameElement.elementList.Count-1; i>-1; i--)
		{
			if(GameElement.elementList[i].roomId == this.id)
			{
				Room.list.RemoveAt(i);
			}
		}

		if(Room.activeRoom == this)
		{
			Room.activeRoom = Room.list.Last();
			Room.activeRoom.Activate();
		}
	}
}

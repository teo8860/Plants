using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;

public class Room
{
	static List<Room> rooms = new List<Room>();

	public Room()
	{
		Room.rooms.Add(this);
	}
}

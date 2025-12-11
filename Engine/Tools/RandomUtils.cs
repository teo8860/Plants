using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants;


public static class RandomUtils
{
	private static readonly Random _rnd = new Random();

	public static float RandomFloat(float min, float max)
	{
		if(max < min)
			throw new ArgumentException("max deve essere >= min");

		float value = (float)_rnd.NextDouble();
		return min + value * (max - min);
	}
}

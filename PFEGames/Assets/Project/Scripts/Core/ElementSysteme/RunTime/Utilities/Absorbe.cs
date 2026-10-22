using UnityEngine;

namespace Goblfin
{
	public static class Absorbe<T> where T : struct, IElementChannel
	{
		public static void Run(ElementSimulation simulation, float amount)
		{
			T channel = ChannelBag<T>.Channel;
			
			float value = channel.Get(simulation) + amount / channel.Resistance(simulation.ElementData);
			channel.Set(simulation, Mathf.Clamp(value, 0f, 100f));
		}
	}
}
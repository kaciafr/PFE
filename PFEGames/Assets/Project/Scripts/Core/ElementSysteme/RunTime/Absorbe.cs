using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public static class Absorbe<T> where T : struct, IElementChannel
	{
		public static void Run(ElementSimulation simulation, float amount)
		{
			T channel = default;
			
			float value = channel.Get(simulation) + amount / channel.Resistance(simulation.ElementData);
			channel.Set(simulation, Mathf.Clamp(value, 0f, 100f));
		}
	}
}
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public static class Propagation<T> where T : struct, IElementChannel 
	{
		private static readonly T Channel = default;

		public static void Run(ElementSimulation source)
		{
			ElementChain chain = source.GetInfo<ElementChain>();
			
			if (chain == null) 
				return;

			float currentStat = Channel.Get(source);
			float speed = Channel.Rate(source);

			foreach (ElementSimulation voisin in chain.neighbours)
			{
				float transfer = (currentStat - Channel.Get(voisin)) * speed * Time.deltaTime;
				Channel.Add(voisin, transfer);
			}
		}
	}
}
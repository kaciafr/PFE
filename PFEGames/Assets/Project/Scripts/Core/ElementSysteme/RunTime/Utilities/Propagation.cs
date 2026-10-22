using Runtime.Project.Scripts.Core;

namespace Goblfin
{
	public static class Propagation<T> where T : struct, IElementChannel 
	{

		public static void Run(ElementSimulation source)
		{
			ElementChain chain = source.GetInfo<ElementChain>();
			
			if (chain == null) 
				return;

			T channel = ChannelBag<T>.Channel;
			
			float currentStat = channel.Get(source);
			float speed = channel.Rate(source);

			foreach (ElementSimulation voisin in chain.neighbours)
			{
				float transfer = (currentStat - channel.Get(voisin)) * speed/2;
				channel.Add(voisin, transfer);
			}
		}
	}
}
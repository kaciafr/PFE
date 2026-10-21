using System.Collections.Generic;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public static class PnjRegistry
	{
		private static readonly List<BrainPnj> all = new List<BrainPnj>();
		public static IReadOnlyList<BrainPnj> All => all;

		public static void Register(BrainPnj brain)
		{
			if (!all.Contains(brain)) all.Add(brain);
		}

		public static void Unregister(BrainPnj brain) => all.Remove(brain);

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => all.Clear();
	}
}
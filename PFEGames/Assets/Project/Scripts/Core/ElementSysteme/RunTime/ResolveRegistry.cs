using System.Collections.Generic;
using UnityEngine;

namespace Goblfin
{
	public enum TypeOfResolution
	{
		Water,
		Reparation,
	}
	public static class ResolveRegistry
	{
		private static Dictionary<TypeOfResolution,HashSet<ElementSimulation>> resolution = new ();
		private static readonly HashSet<ElementSimulation> Empty = new ();
		
		public static IReadOnlyCollection<ElementSimulation> GetResolve(TypeOfResolution type) => resolution.TryGetValue(type, out var result) ? result : Empty;

		public static void RegisterResolve(ElementSimulation simulation, TypeOfResolution type)
		{
			if(!resolution.TryGetValue(type, out var result))
			{
				resolution[type] = result = new HashSet<ElementSimulation>();
				result.Add(simulation);
			}
		}
		
		public static void UnregisterProblem(ElementSimulation simulation, TypeOfResolution type)
		{
			if(resolution.TryGetValue(type, out var result))
				result.Remove(simulation);
		}
			
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => resolution.Clear();
	}
}
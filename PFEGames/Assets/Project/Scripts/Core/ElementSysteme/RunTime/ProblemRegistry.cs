using System.Collections.Generic;
using UnityEngine;

namespace Goblfin
{
	public enum TypeOfProblem
	{
		Burn,
		Broke,
	}
	public static class ProblemRegistry
	{
		private static Dictionary<TypeOfProblem, HashSet<ElementSimulation>> problems=
			new Dictionary<TypeOfProblem, HashSet<ElementSimulation>>();
		private static readonly HashSet<ElementSimulation> Empty = new();
		public static IReadOnlyCollection<ElementSimulation> GetProblems(TypeOfProblem type)
			=> problems.TryGetValue(type, out var result) ? result : Empty;

		public static void RegisterProblem(ElementSimulation simulation, TypeOfProblem type)
		{
			if(!problems.TryGetValue(type, out var result))
				{
					problems[type] = result = new HashSet<ElementSimulation>();
					result.Add(simulation);
				}
		}

		public static void UnregisterProblem(ElementSimulation simulation, TypeOfProblem type)
		{
			if(problems.TryGetValue(type, out var result))
				result.Remove(simulation);
		}
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => problems.Clear();
	}
}
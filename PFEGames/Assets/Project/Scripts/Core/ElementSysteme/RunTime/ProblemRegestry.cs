using System.Collections.Generic;
using Goblfin;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public static class ProblemRegestry
	{
		private static HashSet<ElementSimulation> problems = new HashSet<ElementSimulation>();
		public static IReadOnlyCollection<ElementSimulation> Problems => problems;
		
		public static void RegisterProblem(ElementSimulation simulation)=> problems.Add(simulation);
		public static void UnregisterProblem(ElementSimulation simulation)=> problems.Remove(simulation);
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => problems.Clear();
	}
}
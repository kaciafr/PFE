using Goblfin.PnjSystem.PnjDetection;
using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ProblemAttraction : PnjAptitude
	{
		[SerializeField] private float scanInterval = 0.25f;
		[SerializeField] private TypeOfProblem[] handledTypes = { TypeOfProblem.Burn, TypeOfProblem.Broke };

		private float nextScan;
		private VisionCone visionCone;

		private void Awake() => visionCone = GetComponent<VisionCone>();

		public bool TryFindProblem(out ElementSimulation target, out TypeOfProblem type)
		{
			target = null;
			type = default;

			if (Time.time < nextScan) return false;
			nextScan = Time.time + scanInterval;

			float bestSqr = float.MaxValue;

			foreach (TypeOfProblem problem in handledTypes)
			{
				foreach (ElementSimulation sim in ProblemRegistry.GetProblems(problem))
				{
					Vector3 pos = sim.transform.position;
					float sqr = (pos - transform.position).sqrMagnitude;
					if (sqr >= bestSqr) continue;

					if (visionCone.CanSeePoint(pos, sim.Root.transform))
					{
						target = sim;
						type = problem;
						bestSqr = sqr;
					}
				}
			}
			return target != null;
		}
		
		//public Vector3 FindResolution(TypeOfProblem type)
	}
}
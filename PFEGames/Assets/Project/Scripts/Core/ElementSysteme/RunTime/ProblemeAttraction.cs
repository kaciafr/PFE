using Goblfin.PnjSystem.PnjDetection;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ProblemeAttraction : PnjAptitude
	{
		[SerializeField] private float scanInterval = 0.25f;
		private float nextScan;
		private VisionCone visionCone;
		private void Awake()=> visionCone = GetComponent<VisionCone>();

		public bool TryFindProblem(out ElementSimulation hasProblem)
		{
			hasProblem = null;
			if (Time.time < nextScan) return false;
			nextScan = Time.time + scanInterval;
			
			foreach (ElementSimulation sim in ProblemRegestry.Problems)
			{
				Vector3 pos = sim.transform.position;
				float sqr = (pos - transform.position).sqrMagnitude;
				float bestSqr = float.MaxValue;
				if (sqr >= bestSqr) continue;

				if (visionCone.CanSeePoint(pos, sim.Root.transform))
				{
					hasProblem = sim;
					bestSqr = sqr;
				}
			}

			return hasProblem != null;
		}
		
	}
}
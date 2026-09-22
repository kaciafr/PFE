using PnjDetection;
using UnityEngine;
using Utilities;

namespace PnjStates
{
	public class ChaseState : IPnjStates
	{
		private IDetected target;
		private Vector3 lastKnownPosition;

		public ChaseState(IDetected target)
		{
			this.target = target;
			lastKnownPosition = target.transform.position;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GardeChaseSpeed;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			VisionCone vision = brainPnj.GetAptitude<VisionCone>();
			
			if (vision != null && vision.CanSee(target))
			{
				lastKnownPosition = target.transform.position;
				brainPnj.Agent.SetDestination(lastKnownPosition);
			}
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GardePatrolSpeed;
		}
	}
}
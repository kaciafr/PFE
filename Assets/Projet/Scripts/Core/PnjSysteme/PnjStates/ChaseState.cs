using UnityEngine;

namespace PnjStates
{
	public class ChaseState : IPnjStates
	{
		private Vector3 targetPosition;

		public ChaseState(Vector3 targetPosition)
		{
			this.targetPosition = targetPosition;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			brainPnj.Agent.SetDestination(targetPosition);
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.5f)
				brainPnj.PnjGoTo(new SearchState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
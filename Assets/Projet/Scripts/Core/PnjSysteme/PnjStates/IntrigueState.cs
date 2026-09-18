using UnityEngine;

namespace PnjStates
{
	public class IntrigueState : IPnjStates
	{
		private Vector3 targetPosition;

		public IntrigueState(Vector3 targetPosition)
		{
			this.targetPosition = targetPosition;
		}

		public void EnterState(BrainPnj brainPnj)
		{
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			brainPnj.Agent.SetDestination(targetPosition);
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
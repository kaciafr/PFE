using CharacterController.Script;
using UnityEngine;

namespace PnjStates
{
	public class ChaseState : IPnjStates
	{
		private Vector3 targetPosition;
		private CharacterSetup playerSetup;

		public ChaseState(Vector3 targetPosition, CharacterSetup playerSetup)
		{
			this.targetPosition = targetPosition;
			this.playerSetup = playerSetup;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			GameManager.Instance.See(playerSetup);
			Debug.Log($"Chase State Entered: {brainPnj.name}");
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			brainPnj.Agent.SetDestination(targetPosition);
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f && !GameManager.Instance.IsSee)
				brainPnj.PnjGoTo(new SearchState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			GameManager.Instance.UnSee(playerSetup);
		}
	}
}
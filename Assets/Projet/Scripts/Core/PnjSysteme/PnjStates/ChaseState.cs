using CharacterController.Script;
using PnjDetection;
using UnityEngine;
using Utilities;

namespace PnjStates
{
	public class ChaseState : IPnjStates
	{
		private CharacterSetup playerSetup;
		private Vector3 lastKnownPosition;

		public ChaseState(CharacterSetup playerSetup)
		{
			this.playerSetup = playerSetup;
			this.lastKnownPosition = playerSetup.transform.position;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GardeChaseSpeed;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			VisionCone vision = brainPnj.GetAptitude<VisionCone>();
			
			if (vision != null && vision.CanSee(playerSetup))
			{
				lastKnownPosition = playerSetup.transform.position;
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
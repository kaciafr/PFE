using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ChaseState : IPnjStates
	{
		private IDetected target;
		private Vector3 lastKnownPosition;
		private VisionCone vision;

		public ChaseState(IDetected target)
		{
			this.target = target;
			lastKnownPosition = target.transform.position;
		}

		public StatePriority Priority => StatePriority.Chase;
		public float AgentSpeed => GameMetrix.GuardChaseSpeed;

		public void EnterState(BrainPnj brainPnj)
		{
			//A ENLEVER
			brainPnj.material.color = Color.blue;
			Debug.Log(nameof( ChaseState));
			brainPnj.Agent.speed = AgentSpeed;
			vision = brainPnj.GetAptitude<VisionCone>();
			if (vision == null)
			{
				Debug.LogError($"{nameof(vision)} is null");
				brainPnj.PnjGoTo(new SearchState(brainPnj,lastKnownPosition));
			}
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (vision != null && vision.CanSee(target))
			{
				lastKnownPosition = target.transform.position;
				brainPnj.Agent.SetDestination(lastKnownPosition);
			}
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj,lastKnownPosition));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
		}
	}
}
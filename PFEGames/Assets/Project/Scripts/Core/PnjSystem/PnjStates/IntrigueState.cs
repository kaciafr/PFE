using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class IntrigueState : IPnjStates
	{
		public StatePriority Priority => StatePriority.Intrigue;
		public float AgentSpeed => GameMetrix.GuardPatrolSpeed;
		private Vector3 targetPosition;
		private ISondDetected sondTarget;
		private Transform dangerTarget;
		private IDetected cachedDetected;
		private VisionCone vision;

		public IntrigueState(ISondDetected sondTarget,Transform dangerTarget)
		{
			this.sondTarget = sondTarget;
			this.dangerTarget = dangerTarget;
			targetPosition = sondTarget.Transform.position;
			if (dangerTarget != null)
				cachedDetected = dangerTarget.GetComponent<IDetected>();
		}

		public void EnterState(BrainPnj brainPnj)
		{
			//A ENLEVER
			brainPnj.material.color = Color.green;
			vision = brainPnj.GetAptitude<VisionCone>();

			Debug.Log(nameof(IntrigueState));
			brainPnj.Agent.updateRotation = false;
			brainPnj.Agent.SetDestination(targetPosition);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (vision != null && cachedDetected != null && vision.CanSee(cachedDetected))
			{
				brainPnj.PnjGoTo(new ChaseState(cachedDetected));
				return; 
			}
			
			Vector3 direction = brainPnj.transform.position - targetPosition;
			direction.y = 0;
			
			if (sondTarget != null && sondTarget.Transform != null)
			{
				targetPosition = sondTarget.Transform.position;
				brainPnj.Agent.SetDestination(targetPosition);
			}
			
			if (direction.sqrMagnitude > 0.001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0, 180, 0);
				brainPnj.transform.rotation = Quaternion.Slerp(brainPnj.transform.rotation, targetRotation, Time.deltaTime * 5f);
			}
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj,targetPosition));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.updateRotation = true;
		}
	}
}
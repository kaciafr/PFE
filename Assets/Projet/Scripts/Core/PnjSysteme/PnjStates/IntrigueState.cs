using PnjDetection;
using UnityEngine;

namespace PnjStates
{
	public class IntrigueState : IPnjStates
	{
		private Vector3 targetPosition;
		private ISondDetected sondTarget;
		private GameObject dangerTarget;
		private IDetected cachedDetected;

		public IntrigueState(ISondDetected sondTarget,GameObject dangerTarget)
		{
			this.sondTarget = sondTarget;
			this.dangerTarget = dangerTarget;
			this.targetPosition = sondTarget.Transform.position;
			if (dangerTarget != null)
				cachedDetected = dangerTarget.GetComponent<IDetected>();
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.updateRotation = false;
			brainPnj.Agent.SetDestination(targetPosition);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			var vision = brainPnj.GetAptitude<VisionCone>();
			
			if (vision != null && cachedDetected != null && vision.CanSee(cachedDetected))
			{
				brainPnj.PnjGoTo(new ChaseState(cachedDetected));
				return; 
			}
			
			Vector3 direction = brainPnj.transform.position - targetPosition;
			direction.y = 0;
			
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
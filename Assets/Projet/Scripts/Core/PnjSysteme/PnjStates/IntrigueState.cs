using PnjDetection;
using UnityEngine;

namespace PnjStates
{
	public class IntrigueState : IPnjStates
	{
		private Vector3 targetPosition;
		private IDetected target;

		public IntrigueState(IDetected target)
		{
			this.target = target;
			this.targetPosition = target.transform.position;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.updateRotation = false;
			brainPnj.Agent.SetDestination(targetPosition);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			VisionCone vision = brainPnj.GetAptitude<VisionCone>();
			if (vision != null && vision.CanSee(target))
			{
				brainPnj.PnjGoTo(new ChaseState(target));
				return;
			}

			Vector3 direction = brainPnj.transform.position - targetPosition;
			if (direction.sqrMagnitude > 0.001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0, 180, 0);
				brainPnj.transform.rotation = Quaternion.Slerp(brainPnj.transform.rotation, targetRotation, Time.deltaTime * 5f);
			}

			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.updateRotation = true;
		}
	}
}
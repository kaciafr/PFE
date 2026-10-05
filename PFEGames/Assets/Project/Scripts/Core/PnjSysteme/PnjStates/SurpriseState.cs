using UnityEngine;
using Utilities;

namespace PnjStates
{
	public class SurpriseState : IPnjStates
	{
		private float timeBeSurprised = GameMetrix.TimeToBeSurprise;
		private float time;
		private IDetected target;
		private Vector3 pnjPos;

		public SurpriseState(IDetected target)
		{
			this.target = target;
		}
		public void EnterState(BrainPnj brainPnj)
		{
			time = 0;
			pnjPos = brainPnj.transform.position;
			brainPnj.Agent.isStopped = true;    
			brainPnj.Agent.updateRotation = false;
			//brainPnj.Agent.SetDestination(pnjPos);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (target == null)
			{
				Debug.Log("playerSetup is null regarde directement dans VisionCone");
				return;
			}
			
			Vector3 direction = brainPnj.transform.position - target.transform.position;
			if (direction.sqrMagnitude > 0.001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0, 180, 0);
				brainPnj.transform.rotation = Quaternion.Slerp(brainPnj.transform.rotation, targetRotation, Time.deltaTime * 5f);
			}
			
			time += Time.deltaTime;
			if (time >= timeBeSurprised)
				brainPnj.PnjGoTo(new ChaseState(target));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.isStopped = false; 
			brainPnj.Agent.updateRotation = true;
		}
	}
}
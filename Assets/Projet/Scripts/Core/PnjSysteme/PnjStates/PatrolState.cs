using UnityEngine;

namespace PnjStates
{
	public class PatrolState : IPnjStates
	{
		private PnjMove guard;
		private float waitTimer;
		private bool isWaiting;
		
		public PatrolState(PnjMove guard)
		{
			this.guard = guard;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			GoToCurrentPoint();
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (isWaiting)
			{
				
				guard.transform.rotation = Quaternion.Slerp(
					guard.transform.rotation,
					guard.firstRoutine[guard.currentStep].targetRotation,
					Time.deltaTime * 5f
				);
				waitTimer -= Time.deltaTime;

				if (waitTimer <= 0f)
				{
					isWaiting = false;
					guard.currentStep = (guard.currentStep + 1) % guard.firstRoutine.Count;
					GoToCurrentPoint();
				}
			}
			else
			{
				if (!guard.agent.pathPending && guard.agent.remainingDistance < 0.3f)
				{
					isWaiting = true;
					waitTimer = guard.firstRoutine[guard.currentStep].GetComponent<Routine.PointTime>().MinTime;
				}
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			isWaiting = false;
		}
		
		void GoToCurrentPoint()
		{
			guard.agent.destination = guard.firstRoutine[guard.currentStep].transform.position;
		}
	}
}
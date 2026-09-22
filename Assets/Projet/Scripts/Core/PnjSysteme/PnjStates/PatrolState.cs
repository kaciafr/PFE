using UnityEngine;
using Utilities;

namespace PnjStates
{
	public class PatrolState : IPnjStates
	{
		private BrainPnj guard;
		private float waitTimer;
		private bool isWaiting;
		
		public PatrolState(BrainPnj guard)
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
				brainPnj.Agent.speed = 0;
				guard.transform.rotation = Quaternion.Slerp(
					guard.transform.rotation,
					guard.firstRoutine[guard.currentStep].targetRotation,
					Time.deltaTime * 5f
				);
				waitTimer -= Time.deltaTime;

				if (waitTimer <= 0f)
				{
					brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
					isWaiting = false;
					guard.currentStep = (guard.currentStep + 1) % guard.firstRoutine.Count;
					GoToCurrentPoint();
				}
			}
			else
			{
				if (!guard.Agent.pathPending && guard.Agent.remainingDistance < 0.02f)
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
			guard.Agent.destination = guard.firstRoutine[guard.currentStep].transform.position;
		}
	}
}
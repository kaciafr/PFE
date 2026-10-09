using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem.PnjStates
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
			Debug.Log("PatrolState");
			GoToCurrentPoint();
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (isWaiting)
			{
				brainPnj.Agent.speed = 0;
				guard.transform.rotation = Quaternion.Slerp(
					guard.transform.rotation,
					guard.FirstRoutine[guard.currentStep].targetRotation,
					Time.deltaTime * 5f
				);
				waitTimer -= Time.deltaTime;

				if (waitTimer <= 0f)
				{
					brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
					isWaiting = false;
					guard.currentStep = (guard.currentStep + 1) % guard.FirstRoutine.Count;
					GoToCurrentPoint();
				}
			}
			else
			{
				if (!guard.Agent.pathPending && guard.Agent.remainingDistance < 0.02f)
				{
					isWaiting = true;
					waitTimer = guard.FirstRoutine[guard.currentStep].GetComponent<Routine.PointTime>().MinTime;
				}
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			isWaiting = false;
		}
		
		void GoToCurrentPoint()
		{
			guard.Agent.destination = guard.FirstRoutine[guard.currentStep].transform.position;
		}
	}
}
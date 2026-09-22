using PrimeTween;
using UnityEngine;
using Utilities;
namespace PnjStates
{
	public class SearchState : IPnjStates
	{
		private float timer;
		private BrainPnj guard;
		private Tween lookTween;

		[SerializeField] private float lookAngle = 45f;
		[SerializeField] private float lookDuration = 2f;

		public SearchState(BrainPnj guard)
		{
			this.guard = guard;
			timer = 0f;
			guard.Agent.destination = guard.transform.position;
			guard.Agent.updateRotation = false;

			Quaternion baseRotation = guard.transform.rotation;
			Quaternion leftRotation = baseRotation * Quaternion.Euler(0, -lookAngle, 0);
			Quaternion rightRotation = baseRotation * Quaternion.Euler(0, lookAngle, 0);
			guard.Agent.speed = 0;
			lookTween = Tween.LocalRotation(
				guard.transform,
				leftRotation,
				rightRotation,
				lookDuration,
				cycles: -1,
				cycleMode: CycleMode.Yoyo
			);
		}

		public void EnterState(BrainPnj brainPnj)
		{
			
			timer = 0f;
			brainPnj.Agent.destination = brainPnj.transform.position;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			timer += Time.deltaTime;
			if (timer >= GameMetrix.TimeToSearch)
			{
				brainPnj.PnjGoTo(new PatrolState(guard));
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			lookTween.Stop();
			guard.Agent.speed = GameMetrix.GardePatrolSpeed;
			brainPnj.Agent.updateRotation = true;
		}
	}
}
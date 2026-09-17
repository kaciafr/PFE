using UnityEngine;

namespace PnjStates
{
	public class SearchState : IPnjStates
	{
		private float timer;
		private BrainPnj guard;

		public SearchState(BrainPnj guard)
		{
			this.guard = guard;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			timer = 0f;
			brainPnj.Agent.destination = brainPnj.transform.position;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			timer += Time.deltaTime;
			Quaternion targetRotation = Quaternion.Euler(0, 360, 0);
			guard.transform.rotation = Quaternion.Slerp(guard.transform.rotation, targetRotation, Time.deltaTime * 5f);
			if (timer >= 5)
			{
				brainPnj.PnjGoTo(new PatrolState(guard));
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
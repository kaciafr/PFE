using UnityEngine;
using Utilities;

namespace PnjStates
{
	public class KoState : IPnjStates
	{
		private float wakeUpTime;
		private float maxWakeUpTime = GameMetrix.WakeUpTime;
		public void EnterState(BrainPnj brainPnj)
		{
			wakeUpTime = 0;
			brainPnj.Agent.speed = 0;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			wakeUpTime += Time.deltaTime;
			if (wakeUpTime > maxWakeUpTime)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.WakeUp(brainPnj.Animator);
		}
	}
}
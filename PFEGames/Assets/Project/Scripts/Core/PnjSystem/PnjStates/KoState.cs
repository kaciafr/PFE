using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class KoState : IPnjStates
	{
		public StatePriority Priority => StatePriority.Ko;
		public float AgentSpeed => 0;
		private float wakeUpTime;
		private float maxWakeUpTime = GameMetrix.WakeUpTime;
		public void EnterState(BrainPnj brainPnj)
		{
			//A ENLEVER
			brainPnj.material.color = Color.black;
			brainPnj.Agent.enabled = false; 
			brainPnj.SetRagdollPhysics(true);
			wakeUpTime = 0;
			brainPnj.Agent.speed = AgentSpeed;
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
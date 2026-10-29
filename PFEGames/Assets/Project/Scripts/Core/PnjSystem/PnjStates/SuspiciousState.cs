using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class SuspiciousState : IPnjStates
	{
		public StatePriority Priority =>  StatePriority.Patrol;
		public float AgentSpeed { get; }
		private SuspiciousMeter meter;
		private IDetected target;

		public SuspiciousState(IDetected target)
		{
			this.target = target;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			meter.Reset();
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			VisionCone vision = brainPnj.GetAptitude<VisionCone>();
			if (vision == null) return;

			if (vision.CanSee(target))
			{
				float distFactor = vision.GetDistanceFactor(target);
				float angleFactor = vision.GetAngleFactor(target);
				meter.IncreaseFromDetection(distFactor, angleFactor, Time.deltaTime);
			}
			else
			{
				meter.Decrease(Time.deltaTime);
			}

			if (meter.Value >= 1f)
			{
				brainPnj.PnjGoTo(new SurpriseState(target));
			}
			else if (meter.Value <= 0f)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			
		}
	}
}
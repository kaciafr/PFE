using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ChaseState : IPnjStates
	{
		private IDetected target;
		private Vector3 lastKnownPosition;
		private VisionCone vision;
		float nextReachCheck;
		private ThrowThingsManager thrower;

		public ChaseState(IDetected target)
		{
			this.target = target;
			lastKnownPosition = target.transform.position;
		}
		public StatePriority Priority => StatePriority.Chase;
		public float AgentSpeed => GameMetrix.GuardChaseSpeed;

		public void EnterState(BrainPnj brainPnj)
		{
			Debug.Log("Chase State");
			brainPnj.Agent.speed = AgentSpeed;
			vision = brainPnj.GetAptitude<VisionCone>();
			thrower = brainPnj.GetAptitude<ThrowThingsManager>();
			nextReachCheck = Time.time + 0.5f;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			var canSee = vision.CanSee(target);
			if (canSee)
			{
				lastKnownPosition = target.transform.position;

				if (Time.time >= nextReachCheck)
				{
					nextReachCheck = Time.time + 0.5f;

					if (thrower != null && thrower.RocksLeft > 0
					                    && thrower.InRange(lastKnownPosition)
					                    && !thrower.CanReach(lastKnownPosition))
					{
						brainPnj.PnjGoTo(new ThrowState(target));
						return;
					}
				}
				brainPnj.Agent.SetDestination(lastKnownPosition);
			}
			if (!canSee && !brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj,lastKnownPosition));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			
		}
	}
}
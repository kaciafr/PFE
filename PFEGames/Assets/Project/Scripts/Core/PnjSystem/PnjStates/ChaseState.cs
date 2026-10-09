using Goblfin.PnjSystem.PnjDetection;
using Goblfin.PnjSystem.Utilities;
using Goblfin.PnjSystem.WhatTheyDetect;
using UnityEngine;

namespace Goblfin.PnjSystem.PnjStates
{
	public class ChaseState : IPnjStates
	{
		private IDetected target;
		private Vector3 lastKnownPosition;

		public ChaseState(IDetected target)
		{
			this.target = target;
			lastKnownPosition = target.transform.position;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			Debug.Log(nameof( ChaseState));
			brainPnj.Agent.speed = GameMetrix.GuardChaseSpeed;
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			VisionCone vision = brainPnj.GetAptitude<VisionCone>();
			
			if (vision != null && vision.CanSee(target))
			{
				lastKnownPosition = target.transform.position;
				brainPnj.Agent.SetDestination(lastKnownPosition);
			}
			
			if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.2f)
				brainPnj.PnjGoTo(new SearchState(brainPnj,lastKnownPosition));
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
		}
	}
}
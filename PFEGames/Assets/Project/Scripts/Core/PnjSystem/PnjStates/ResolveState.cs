using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ResolveState : IPnjStates
	{
		public StatePriority Priority => StatePriority.Resolve;
		public float AgentSpeed=> GameMetrix.GuardChaseSpeed;
		private GrabThings grabThings;
		private Vector3 lastPos;
		private readonly ElementSimulation elementSim;
		private ResolveProblem resolveProblem;
		private ResolutionSource resolutionSource;

		public ResolveState(Vector3 lastPos, ElementSimulation elementSim, ResolutionSource resolutionSource)
		{
			this.lastPos = lastPos;
			this.elementSim = elementSim;
			this.resolutionSource = resolutionSource;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = AgentSpeed;
			brainPnj.Agent.SetDestination(resolutionSource.transform.position);
			resolveProblem = brainPnj.GetAptitude<ResolveProblem>();
			grabThings = brainPnj.GetAptitude<GrabThings>();
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			
			if (brainPnj.Agent.remainingDistance <= 0.2f)
			{
				grabThings.Grab();
				brainPnj.Agent.SetDestination(lastPos);
				if (brainPnj.Agent.remainingDistance <= 0.2f)
				{
					if (resolveProblem.CloseToTheProbleme(elementSim) && elementSim.CurrentState is BurningState)
					{
						brainPnj.Agent.speed = 0;
						resolveProblem.Resolve(elementSim);
					}
				}
				
				if(elementSim.CurrentState is not BurningState)
				{
					brainPnj.ForceState(new PatrolState(brainPnj));
				}
			}
			

		}

		public void ExitState(BrainPnj brainPnj)
		{
			grabThings.UnGrab();
			Communication.EndAction(brainPnj);
		}
	}
}
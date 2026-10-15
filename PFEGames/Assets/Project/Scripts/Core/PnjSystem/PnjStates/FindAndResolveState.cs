using Goblfin.PnjSystem.PnjStates;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class FindAndResolveState : IPnjStates
	{
		
		private readonly TypeOfProblem problem;
		private readonly ElementSimulation elementSim;
		private bool isResolving;
		private bool isGrabbing;
		private Vector3 lastPos;
		private TypeOfResolution resolution;
		private ProblemAttraction resolveAptitude;
		private ResolveProblem resolveProblem;
		private GrabThings grabThings;

		public FindAndResolveState(ElementSimulation elementSim,TypeOfProblem problem)
		{
			this.problem = problem;
			this.elementSim = elementSim;
		}


		public void EnterState(BrainPnj brainPnj)
		{
			lastPos = elementSim.transform.position;
			isGrabbing = false;
			resolveAptitude = brainPnj.GetAptitude<ProblemAttraction>();
			grabThings = brainPnj.GetAptitude<GrabThings>();
			resolveProblem = brainPnj.GetAptitude<ResolveProblem>();
			brainPnj.Agent.speed = GameMetrix.GuardChaseSpeed;
			resolution = ResolveRegistry.WhatForWhat(problem);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (resolveAptitude.TryFindResolution(out ResolutionSource resolutionSource, out resolution) && !isGrabbing)
			{
				brainPnj.Agent.SetDestination(resolutionSource.transform.position);
				if (brainPnj.Agent.remainingDistance <= 0.2f)
				{
					isGrabbing = true;
					grabThings.Grab();
				}
			}

			if (isGrabbing)
			{
				if (resolveAptitude == null || elementSim.CurrentState is not BurningState)
				{
					brainPnj.PnjGoTo(new PatrolState(brainPnj));
					return;
				}
				brainPnj.Agent.SetDestination(lastPos);
				if(brainPnj.Agent.remainingDistance <= 0.2f)
				{
					if (resolveProblem.CloseToTheProbleme(elementSim))
					{
						resolveProblem.Resolve(elementSim);
					}
				}
				
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
		}
	}
}
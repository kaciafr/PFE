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
				if (resolveAptitude == null)
				{
					brainPnj.PnjGoTo(new PatrolState(brainPnj));
					return;
				}
				if (resolveProblem.CloseToTheProbleme(elementSim))
				{
					brainPnj.Agent.SetDestination(elementSim.transform.position);
					resolveProblem.Resolve(elementSim);
				}
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
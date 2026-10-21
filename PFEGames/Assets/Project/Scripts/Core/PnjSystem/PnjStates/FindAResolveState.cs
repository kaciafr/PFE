using System.Collections.Generic;
using Goblfin.PnjSystem.Utilities;
using PrimeTween;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class FindAResolveState : IPnjStates
	{
		public StatePriority Priority => StatePriority.Resolve;
		private readonly TypeOfProblem problem;
		private readonly ElementSimulation elementSim;
		private Vector3 lastPos;
		private TypeOfResolution resolution;
		private ProblemAttraction resolveAptitude;
		
		private int currentSearchIndex;
		private List<Vector3> searchPoints = new List<Vector3>();
		private bool isWaiting;
		private float timer;

		public FindAResolveState(ElementSimulation elementSim,TypeOfProblem problem)
		{
			this.problem = problem;
			this.elementSim = elementSim;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.material.color = Color.yellowGreen;
			timer = 0;
			lastPos = elementSim.transform.position;
			resolveAptitude = brainPnj.GetAptitude<ProblemAttraction>();
			
			brainPnj.GetAptitude<ResolveProblem>();
			brainPnj.Agent.speed = GameMetrix.GuardChaseSpeed;
			resolution = ResolveRegistry.WhatForWhat(problem);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (resolveAptitude.TryFindResolution(out ResolutionSource resolutionSource, out resolution))
			{
				brainPnj.PnjGoTo(new ResolveState(lastPos,elementSim,resolutionSource));
			}
			else
			{
				searchPoints = GameMetrix.SearchPointsGenerated(brainPnj.transform.position, GameMetrix.SearchRadius, GameMetrix.SearchPointCount);
				if (searchPoints.Count > 0)
					brainPnj.Agent.SetDestination(searchPoints[0]);
				
				if (isWaiting)
				{
					timer -= Time.deltaTime;
					if (timer <= 0f)
						GoToNextPoint(brainPnj);
				}
				else
				{
					if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.5f)
					{
						isWaiting = true;
						timer = GameMetrix.SearchWaitPerPoint;
					}
				}
				
			}
			
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GuardPatrolSpeed;
		}
		
		private void GoToNextPoint(BrainPnj brainPnj)
		{
			currentSearchIndex++;

			if (currentSearchIndex >= searchPoints.Count)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
				return;
			}

			isWaiting = false;
			brainPnj.Agent.SetDestination(searchPoints[currentSearchIndex]);
		}
	}
}
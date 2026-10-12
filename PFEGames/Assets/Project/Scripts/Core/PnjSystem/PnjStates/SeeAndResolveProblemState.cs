using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Goblfin.PnjSystem.PnjStates
{
	public class SeeAndResolveProblemState : IPnjStates
	{
		private readonly ElementSimulation problem;
		private bool isResolving;

		public SeeAndResolveProblemState(ElementSimulation problem)
		{
			this.problem = problem;
		}


		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.SetDestination(problem.transform.position);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (problem == null || problem.currentState is not BurningState)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
				return;
			}

			var resolveAptitude = brainPnj.GetAptitude<ResolveProbleme>();
			
			if(resolveAptitude == null)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
				return;
			}

			if (resolveAptitude.CloseToTheProbleme(problem))
			{
				if (isResolving)
				{
					isResolving = false;
					resolveAptitude.Resolve(problem);
					brainPnj.Agent.isStopped = true;
					brainPnj.Agent.SetDestination(problem.transform.position);
				}
				return;
			}

			if (!isResolving)
			{
				isResolving = true;
				brainPnj.Agent.isStopped = true;
			}
			FaceFire(brainPnj);
		}

		public void ExitState(BrainPnj brainPnj)
		{
			
		}
		
		private void FaceFire(BrainPnj brainPnj)
		{
			Vector3 dir = problem.transform.position - brainPnj.transform.position;
			dir.y = 0f;
			if (dir.sqrMagnitude < 0.001f) return;

			brainPnj.transform.rotation = Quaternion.RotateTowards(
				brainPnj.transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);
		}
	}
}
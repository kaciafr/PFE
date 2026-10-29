using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ResolveBrokeProblemState : IPnjStates
	{
		public StatePriority Priority => StatePriority.Resolve;
		public float AgentSpeed => GameMetrix.GuardChaseSpeed;
		private readonly ElementSimulation problem;
		private bool isResolving;
		public ResolveBrokeProblemState(ElementSimulation problem)
		{
			this.problem = problem;
		}
		public void EnterState(BrainPnj brainPnj)
		{
			Debug.Log("Broke state entered");
			brainPnj.Agent.speed = AgentSpeed;
			brainPnj.Animator.SetBool("ResolveAction", isResolving);
			brainPnj.Agent.SetDestination(problem.transform.position);
		}
		public void UpdateState(BrainPnj brainPnj)
		{
			if (problem == null || problem.CurrentState is not BurningState)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
				return;
			}
			var resolveAptitude = brainPnj.GetAptitude<ResolveProblem>();
			
			if(resolveAptitude == null)
			{
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
				return;
			}

			if (resolveAptitude.CloseToTheProbleme(problem))
			{
				if (isResolving)
				{
					FaceFire(brainPnj);
					isResolving = false;
					resolveAptitude.Resolve(problem);
					brainPnj.Agent.SetDestination(problem.transform.position);
				}
				return;
			}

			if (!isResolving)
			{
				isResolving = true;
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
				brainPnj.transform.rotation, Quaternion.LookRotation(dir), 360f *0.2f);
		}
	
	}
}
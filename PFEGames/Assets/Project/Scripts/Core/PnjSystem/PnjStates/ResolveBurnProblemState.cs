using Goblfin.PnjSystem.PnjStates;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ResolveBurnProblemState : IPnjStates
	{
		private readonly ElementSimulation problem;
		private bool isResolving;

		public ResolveBurnProblemState(ElementSimulation problem)
		{
			this.problem = problem;
		}


		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.Agent.speed = GameMetrix.GuardChaseSpeed;
			Debug.Log("Burn state entered");
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
				resolveAptitude.Resolve(problem);
				FaceFire(brainPnj);
				brainPnj.Agent.SetDestination(problem.transform.position);
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Animator.SetBool("Blender Tree", isResolving);
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
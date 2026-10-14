using Goblfin.PnjSystem.PnjStates;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ResolveBurnProblemState : IPnjStates
	{
		
		private readonly TypeOfProblem problem;
		private readonly ElementSimulation elementSim;
		private bool isResolving;

		public ResolveBurnProblemState(ElementSimulation elementSim,TypeOfProblem problem)
		{
			this.problem = problem;
			this.elementSim = elementSim;
		}


		public void EnterState(BrainPnj brainPnj)
		{
			var resolveAptitude = brainPnj.GetAptitude<ProblemAttraction>();
			
			var resolvePos = resolveAptitude.FindBurnResolution(problem,brainPnj.transform.position);
			brainPnj.Agent.SetDestination(resolvePos);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if (problem == null || elementSim.CurrentState is not BurningState)
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

			if (resolveAptitude.CloseToTheProbleme(elementSim))
			{
				resolveAptitude.Resolve(elementSim);
				brainPnj.Agent.SetDestination(elementSim.transform.position);
			}
		}

		public void ExitState(BrainPnj brainPnj)
		{
			brainPnj.Animator.SetBool("Blender Tree", isResolving);
		}
	}
}
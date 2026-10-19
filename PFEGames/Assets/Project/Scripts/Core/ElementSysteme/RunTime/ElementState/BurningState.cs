using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Goblfin
{
	public class BurningState: IElementState
	{
		private Health currentHealth;
		public void Enter(ElementSimulation element)
		{
			Debug.Log("Entering BurningState");
			element.burn.Play();
			currentHealth = element.GetInfo<Health>();
			currentHealth.SetDeathVisual(DeathVisual.Burned);
			ProblemRegistry.RegisterProblem(element, TypeOfProblem.Burn);
		}

		public void Update(ElementSimulation element)
		{
			currentHealth.TakeDamageEnv(element.ElementData.HitPower);
			element.FirePropagation();
			if (element.Heat <= 0.1f)
			{
				ProblemRegistry.UnregisterProblem(element,TypeOfProblem.Burn);
				element.ChangeState(new NormalState());
			}
		}

		public void Exit(ElementSimulation element)
		{
			ProblemRegistry.UnregisterProblem(element,TypeOfProblem.Burn);
			element.burn.Stop();
		}
	}
}
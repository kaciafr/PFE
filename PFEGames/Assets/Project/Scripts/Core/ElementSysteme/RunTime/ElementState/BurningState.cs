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
			element.ElementData.phaseTwoParticle.Play();
			currentHealth = element.GetInfo<Health>();
			currentHealth.SetDeathVisual(DeathVisual.Burned);
			ProblemRegestry.RegisterProblem(element);
		}

		public void Update(ElementSimulation element)
		{
			currentHealth.TakeDamageEnv(element.ElementData.HitPower);
			element.FirePropagation();
			if (element.Heat <= element.ElementData.Hot)
			{
				element.ChangeState(new NormalState());
			}
		}

		public void Exit(ElementSimulation element)
		{
			element.ElementData.phaseTwoParticle.Stop();
			ProblemRegestry.UnregisterProblem(element);
		}
	}
}
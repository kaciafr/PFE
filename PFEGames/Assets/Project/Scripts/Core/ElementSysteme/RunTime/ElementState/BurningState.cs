using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class BurningState: IElementState
	{
		private Health currentHealth;
		public void Enter(ElementSimulation element)
		{
			Debug.Log("Entering BurningState");
			currentHealth = element.GetInfo<Health>();
			currentHealth.SetDeathVisual(DeathVisual.Burned);
		}

		public void Update(ElementSimulation element)
		{
			currentHealth.TakeDamageEnv(element.ElementData.DamageHealth);
			element.FirePropagation();
			if (element.Heat <= element.ElementData.Brule / 2)
			{
				element.ChangeState(new NormalState());
			}
		}

		public void Exit(ElementSimulation element)
		{
		}
	}
}
using Runtime.Project.Scripts.Core;
using Runtime.Project.Scripts.Core.ElementSysteme.RunTime;
using UnityEngine;

namespace Runtime
{
	public class Health : ElementInfo
	{
		[field: SerializeField] public DeathVisual CurrentDeathVisual { get; private set; }

		private ElementSimulation simulation;
		private float currentHealth;
		private bool isDead;

		public override void Init(ElementSimulation element)
		{
			simulation = element;
			ResetHealth();
		}

		private void OnEnable() => ResetHealth(); 

		private void ResetHealth()
		{
			isDead = false;
			CurrentDeathVisual = DeathVisual.Normal;

			if (simulation != null)
				currentHealth = simulation.ElementData.MaxHealth;
		}

		public void SetDeathVisual(DeathVisual visual) => CurrentDeathVisual = visual;

		public void TakeDamageEnv(float damagePerSecond)
		{
			if (isDead) return;

			currentHealth -= damagePerSecond * Time.deltaTime;

			if (currentHealth <= 0f)
				Die();
		}

		private void Die()
		{
			isDead = true;
			ElementDeath.Replace(simulation, CurrentDeathVisual);
		}
	}
}
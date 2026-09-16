using System;
using System.Windows.Input;

namespace CharacterController.Script
{
	public struct DamageCommand : IGameCommand
	{
		private HealthComponent healthComponent;
		private float damage;

		public DamageCommand(HealthComponent cible, float damage)
		{
			this.healthComponent = cible;
			this.damage = damage;
		}

		public void Execute()
		{
			healthComponent.AddOrRemoveHealth(-damage);
		}
	}
}
namespace Runtime.Project.Scripts.Core
{
	public class FrozenState : IElementState
	{
		private Health currentHealth;
		public void Enter(ElementSimulation element)
		{
			currentHealth.SetDeathVisual(DeathVisual.Frozen);
		}

		public void Update(ElementSimulation element)
		{
			element.AddHumidity(1);
			
			if (element.Heat > 5f)
			{
				element.ChangeState(new NormalState());
			}
		}

		public void Exit(ElementSimulation element)
		{
			
		}
	}
}
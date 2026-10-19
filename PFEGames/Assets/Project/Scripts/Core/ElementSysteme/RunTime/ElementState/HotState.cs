using Goblfin.PnjSystem;
using Runtime.Project.Scripts.Core;

namespace Goblfin
{
	public class HotState : IElementState
	{
		public void Enter(ElementSimulation element)
		{
			element.hot.Play();
		}

		public void Update(ElementSimulation element)
		{
			if (element.Heat > element.ElementData.Burn)
			{
				element.hot.Stop();
				element.ChangeState(new BurningState());
			}
		}

		public void Exit(ElementSimulation element)
		{
		}
	}
}
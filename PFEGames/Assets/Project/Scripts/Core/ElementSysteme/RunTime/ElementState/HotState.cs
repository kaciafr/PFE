using Goblfin.PnjSystem;
using Goblfin.PnjSystem.PnjStates;
using Runtime.Project.Scripts.Core;

namespace Goblfin
{
	public class HotState : IElementState
	{
		public void Enter(ElementSimulation element)
		{
			element.ElementData.phaseOneParticle.Play();
		}

		public void Update(ElementSimulation element)
		{
			if (element.Heat > element.ElementData.Burn)
			{
				element.ChangeState(new BurningState());
			}
		}

		public void Exit(ElementSimulation element)
		{
			element.ElementData.phaseOneParticle.Stop();
		}
	}
}
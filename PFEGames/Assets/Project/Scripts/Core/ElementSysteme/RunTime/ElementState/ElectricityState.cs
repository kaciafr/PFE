using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ElectricityState : IElementState
	{
		private bool isElectrifing = false;
		public void Enter(ElementSimulation element)
		{
		}

		public void Update(ElementSimulation element)
		{
			element.ElectricityPropagation();
			if (element.Electricity >= element.ElementData.Tazzer)
			{
				isElectrifing = true;
				element.ElectricityPropagation();
			}
			else
				isElectrifing = false;

			if (isElectrifing && element.Humidity > 50f)
				element.AddHeat(5 * Time.deltaTime);
		}

		public void Exit(ElementSimulation element)
		{
		}
	}
}
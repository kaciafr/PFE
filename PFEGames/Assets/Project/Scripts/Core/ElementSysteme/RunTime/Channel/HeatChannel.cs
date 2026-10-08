namespace Runtime.Project.Scripts.Core
{
	public struct HeatChannel : IElementChannel
	{
		public float Get(ElementSimulation element)=> element.Heat;
		public void Add(ElementSimulation element, float amount) => element.AddHeat(amount);
		public float Rate(ElementSimulation element) => 0.5f;
	}
}
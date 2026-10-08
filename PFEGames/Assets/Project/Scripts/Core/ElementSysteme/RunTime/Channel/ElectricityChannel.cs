namespace Runtime.Project.Scripts.Core
{
	public struct ElectricityChannel : IElementChannel
	{
		public float Get(ElementSimulation element) => element.Electricity;
		public void Add(ElementSimulation element, float amount) => element.AddElectricity(amount);
		public float Rate(ElementSimulation element) =>  0.5f;
		public float Resistance(ElementData element) => element.ElectricityConductibility;
		public float Set(ElementSimulation element, float amount) => amount * Rate(element);
	}
}
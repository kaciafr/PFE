namespace Runtime.Project.Scripts.Core
{
	public struct HumidityChannel : IElementChannel
	{
		public float Get(ElementSimulation element) => element.Humidity;
		public void Add(ElementSimulation element, float amount) => element.AddHumidity(amount);
		public float Rate(ElementSimulation element) => 0.5f;
		public float Resistance(ElementData element) => element.FireResistance;
		public float Set(ElementSimulation element, float amount) => amount * Rate(element);
	}
}
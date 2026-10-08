namespace Runtime.Project.Scripts.Core
{
	public interface IElementChannel
	{
		float Get(ElementSimulation element);
		void Add(ElementSimulation element, float amount);
		float Rate(ElementSimulation element); 
		float Resistance(ElementData element);
		float Set (ElementSimulation element, float amount);
	}
}
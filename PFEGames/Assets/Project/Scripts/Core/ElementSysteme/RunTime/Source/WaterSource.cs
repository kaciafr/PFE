using UnityEngine;

namespace Goblfin
{
	public class WaterSource : ResolutionSource
	{
		[SerializeField] private float humidityPerSecond;

		private void OnTriggerStay(Collider other)
		{
			ElementSimulation elementSimulation = other.GetComponent<ElementSimulation>();
			if (elementSimulation != null)
			{
				elementSimulation.AddHumidity(humidityPerSecond * 0.3f);
			}
		}
	}
}
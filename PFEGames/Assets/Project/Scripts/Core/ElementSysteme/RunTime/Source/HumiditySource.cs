using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class HumiditySource : MonoBehaviour
	{
		[SerializeField] private float humidityPerSecond;

		private void OnTriggerStay(Collider other)
		{
			ElementSimulation elementSimulation = other.GetComponent<ElementSimulation>();
			if (elementSimulation != null)
			{
				elementSimulation.AddHumidity(humidityPerSecond * Time.deltaTime);
			}
		}
	}
}
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ElectricitySource : MonoBehaviour
	{
		[SerializeField] public	float electricityPerSecond;

		private void OnTriggerStay(Collider other)
		{
			ElementSimulation elementSimulation = other.GetComponent<ElementSimulation>();

			if (elementSimulation != null)
			{
				elementSimulation.AddElectricity(electricityPerSecond*Time.deltaTime);
			}
		}
	}
}
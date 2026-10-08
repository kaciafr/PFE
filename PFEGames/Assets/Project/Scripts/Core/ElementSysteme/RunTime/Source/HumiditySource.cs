using System;
using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Runtime
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
using UnityEngine;

namespace Goblfin
{
	public class FireSource : MonoBehaviour
	{
		[SerializeField] private float heatPerSecond;

		private void OnTriggerStay(Collider other)
		{
			ElementSimulation elementSimulation = other.GetComponent<ElementSimulation>();

			if (elementSimulation != null)
			{
				elementSimulation.AddHeat(heatPerSecond * 0.3f);
			}
		}
	}
}
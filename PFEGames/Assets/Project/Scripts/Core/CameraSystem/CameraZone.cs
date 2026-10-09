using UnityEngine;

namespace Goblfin
{
	[RequireComponent(typeof(Collider))]
	public class CameraZone : MonoBehaviour
	{
		[SerializeField] CameraState state;

		void Reset() => GetComponent<Collider>().isTrigger = true;

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
				CameraController.Instance.Request(state);
		}

		void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
				CameraController.Instance.Release();
		}
	}
}
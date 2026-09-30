using UnityEngine;

namespace RagdollSysteme
{
	public class ActiveRagdoll :  MonoBehaviour
	{
		[SerializeField] private TestRagDoll ragdollController;

		private void OnTriggerEnter(Collider other)
		{
			if (other.transform.CompareTag("Player"))
			{
				ragdollController.EnableRagdoll();
			}
		}
	}
}
using UnityEngine;

namespace GamePlay
{
	public class RagdollController : MonoBehaviour
	{
		private Rigidbody[] ragdollRigidbodies;
		private Collider[] ragdollColliders;
		private Animator animator;

		void Awake()
		{
			animator = GetComponent<Animator>();
			ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
			ragdollColliders = GetComponentsInChildren<Collider>();

			SetRagdollActive(false); 
		}

		public void SetRagdollActive(bool active)
		{
			animator.enabled = !active;

			foreach (Rigidbody rb in ragdollRigidbodies)
				rb.isKinematic = !active;

			foreach (Collider col in ragdollColliders)
				col.enabled = active;
		}

		
		public void Die()
		{
			SetRagdollActive(true);
		}
	}
}
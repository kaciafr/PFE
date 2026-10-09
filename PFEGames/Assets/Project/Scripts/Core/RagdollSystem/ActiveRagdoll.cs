using UnityEngine;

namespace Goblfin.RagdollSystem
{
	public class ActiveRagdoll :  MonoBehaviour
	{
		private RagDoll ragdoll;
		private Animator animator;

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag("Guard"))
				return;

			RagDoll ragdoll = other.GetComponentInParent<RagDoll>();
			Animator animator = other.GetComponentInParent<Animator>();

			if (ragdoll != null && animator != null)
				ragdoll.Ragdool(animator);
		}
	}
}
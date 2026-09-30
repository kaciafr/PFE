using UnityEngine;

namespace RagdollSysteme
{
	public class TestRagDoll : MonoBehaviour
	{
		public Animator animator;
	
		public void EnableRagdoll()
		{
			animator.enabled = false;
		}
	}
}

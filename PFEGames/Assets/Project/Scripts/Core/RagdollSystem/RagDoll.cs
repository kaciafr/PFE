using System;
using UnityEngine;

namespace Goblfin.RagdollSystem
{
	public abstract class RagDoll : MonoBehaviour
	{
		public event Action OnRagdoll;
		public event Action OnWakeUp;

		public bool IsRagdolled { get; private set; }

		public void Ragdool(Animator animator)
		{
			if (IsRagdolled) return;
			IsRagdolled = true;

			animator.enabled = false;
			OnRagdoll?.Invoke();
		}

		public void WakeUp(Animator animator)
		{
			if (!IsRagdolled) return;
			IsRagdolled = false;

			animator.enabled = true;
			OnWakeUp?.Invoke();
		}
	}
}
using UnityEngine;

namespace CharacterController.Script.PlayerControllers.Utilities
{
	[RequireComponent(typeof(Animator))]
	public class RootMotion : MonoBehaviour
	{
		[SerializeField] private Transform characterRoot;
		private Animator animator;

		private void Awake()
		{
			animator = GetComponent<Animator>();

			if (characterRoot == null)
				Debug.LogError($"{nameof(RootMotion)} sur {name} : characterRoot n'est pas assigné dans l'inspecteur.");
		}

		private void OnAnimatorMove()
		{
			if (characterRoot == null || !animator.applyRootMotion)
				return;

			var deltaPosition = animator.deltaPosition;
			var deltaRotation = animator.deltaRotation;

			characterRoot.position += deltaPosition;
			characterRoot.rotation *= deltaRotation;
		}
	}
}
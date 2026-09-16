using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script.PlayerControllers
{
	public class AnimationHandler : MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private CharacterSetup characterSetup;

		[SerializeField] private UnityEngine.CharacterController controller;

		[Header("Setup")] [SerializeField] private float smoothing = 0.2f;
		[SerializeField] private float runningSmoothing = 0.25f;

		[Header("Blend Tree Scale")] [SerializeField]
		private float runBlendValue = 1.5f;

		private void Update()
		{
			if (characterSetup.MoveState is VaultState)
				return;

			UpdateLocomotion();
			characterSetup.Animator.SetBool("jump", controller.isGrounded);
		}

		private void UpdateLocomotion()
		{
			var localVelocity = transform.InverseTransformDirection(characterSetup.MovementBase.CurrentMoveDirection);
			var maxSpeed = characterSetup.CharacterData.playerRunVelocity;
			var normalizedVelocity = maxSpeed > 0.01f
				? Vector3.ClampMagnitude(localVelocity / maxSpeed, 1f) * runBlendValue
				: Vector3.zero;

			var isRunning = characterSetup.ReadInput.IsRunning;
			var currentSmoothing = isRunning ? runningSmoothing : smoothing;

			characterSetup.Animator.SetFloat("X", normalizedVelocity.x, currentSmoothing, Time.deltaTime);
			characterSetup.Animator.SetFloat("Y", normalizedVelocity.z, currentSmoothing, Time.deltaTime);
		}
	}
}
using CharacterController.Script.PlayerControllers;
using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script
{
	public class MovementBase : MonoBehaviour
	{
		[Header("Component")] [SerializeField] private CharacterSetup characterSetup;

		[SerializeField] public UnityEngine.CharacterController controller;

		private float targetSpeed;
		private Vector3 verticalForce;
		private float verticalVelocity;
		private float walkSpeed;

		public Vector3 CurrentMoveDirection { get; private set; }

		private void Start()
		{
			walkSpeed = characterSetup.CharacterData.playerVelocity;
		}

		private void LateUpdate()
		{
			if (characterSetup.MoveState is VaultState)
				return;

			HandleRotation();
			Jump();
			HandleMovement(characterSetup.ReadInput.IsRunning);

			if (CurrentMoveDirection.magnitude > characterSetup.CharacterData.vaultAutoSpeedThreshold)
				Vaulting.TryVault(characterSetup, CurrentMoveDirection.magnitude);
		}

		private void HandleRotation()
		{
			var direction = characterSetup.ReadInput.Direction;
			var inputDir = new Vector3(direction.x, 0, direction.y);

			if (inputDir.magnitude < 0.1f)
				return;

			var dataCharacter = characterSetup.CharacterData;
			var targetRotation = Quaternion.LookRotation(inputDir);
			controller.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
				dataCharacter.rotationSpeed * Time.deltaTime);
		}

		private void HandleMovement(bool isRunning)
		{
			var dataCharacter = characterSetup.CharacterData;
			var direction = characterSetup.ReadInput.Direction;

			var maxSpeed = isRunning ? dataCharacter.playerRunVelocity : walkSpeed;
			targetSpeed = maxSpeed;

			var characterDirection = new Vector3(direction.x, 0, direction.y).normalized * targetSpeed;

			if (direction.magnitude > 0.1f)
				CurrentMoveDirection = Vector3.MoveTowards(CurrentMoveDirection,
					characterDirection,
					dataCharacter.playerAcceleration * Time.deltaTime);
			else
				CurrentMoveDirection = Vector3.MoveTowards(CurrentMoveDirection,
					Vector3.zero,
					dataCharacter.playerDeceleration * Time.deltaTime);

			/*Vector3 horizontal = new Vector3(moveDirection.x, 0, moveDirection.z);
			horizontal = Vector3.ClampMagnitude(horizontal, maxSpeed);
			moveDirection = new Vector3(horizontal.x, moveDirection.y, horizontal.z);*/

			if (controller.isGrounded && verticalForce.y < 0)
				verticalForce.y = -2f;
			else
				verticalForce.y += Physics.gravity.y * Time.deltaTime;

			var finalMove = CurrentMoveDirection + verticalForce;

			controller.Move(finalMove * Time.deltaTime);
		}

		private void Jump()
		{
			var isJumping = characterSetup.ReadInput.isJumping;

			if (isJumping)
			{
				Vaulting.TryVault(characterSetup, CurrentMoveDirection.magnitude);
				characterSetup.ReadInput.isJumping = false;

				if (characterSetup.MoveState is VaultState)
					return;
			}

			var jumpForce = characterSetup.CharacterData.jumpForce;
			var gravity = characterSetup.CharacterData.gravity;

			if (controller.isGrounded && verticalVelocity < 0f)
				verticalVelocity = -2f;

			if (isJumping && controller.isGrounded)
			{
				verticalVelocity = jumpForce;
				characterSetup.ReadInput.isJumping = false;
			}

			verticalVelocity += gravity * Time.deltaTime;
			controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
		}

		public void AddOrRemoveSpeed(float speed)
		{
			walkSpeed += speed;
		}
	}
}
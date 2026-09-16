using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterController.Script
{
	public class ReadInput : MonoBehaviour
	{
		[SerializeField] private PlayerInput playerInput;

		public bool isJumping;
		[field: SerializeField] public Vector2 Direction { get; private set; }
		[field: SerializeField] public bool IsRunning { get; set; }
		[field: SerializeField] public bool IsFighting { get; set; }
		[field: SerializeField] public bool IsCrunching { get; set; }

		private InputAction moveAction;
		public event Action OnAttackPressed;

		private void Awake()
		{
			playerInput = GetComponent<PlayerInput>();
			moveAction = playerInput.actions["Player/Move"];
		}

		private void Update()
		{
			Direction = moveAction.ReadValue<Vector2>();
		}

		public void Run(InputAction.CallbackContext context)
		{
			IsRunning = context.performed;
		}

		public void Crunch(InputAction.CallbackContext context)
		{
			if (context.performed)
				IsCrunching = !IsCrunching;
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if (context.performed)
				isJumping = true;
		}

		public void FightMode(InputAction.CallbackContext context)
		{
			if (context.performed)
				IsFighting = !IsFighting;
		}

		public void Attack(InputAction.CallbackContext context)
		{
			if (context.performed)
				OnAttackPressed?.Invoke();
		}
	}
}
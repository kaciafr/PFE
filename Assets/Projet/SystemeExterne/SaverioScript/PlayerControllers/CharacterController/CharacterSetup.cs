using System;
using UnityEngine;

namespace CharacterController.Script
{
	public class CharacterSetup : MonoBehaviour
	{
		[field: SerializeField] public CharacterData CharacterData { get; private set; }

		[field: SerializeField] public ReadInput ReadInput { get; private set; }

		[field: SerializeField] public MovementBase MovementBase { get; private set; }

		[field: SerializeField] public Animator Animator { get; private set; }

		public IMoveState MoveState { get; private set; }
		public event Action<IMoveState> OnStateChanged;


		private void Start()
		{
			GoTo(new NormalWalk());
		}

		private void Update()
		{
			MoveState.UpdateState(this);
		}

		private void OnEnable()
		{
			this.ReadInput.OnAttackPressed += HandleAttackPressed;
		}

		private void OnDisable()
		{
			this.ReadInput.OnAttackPressed -= HandleAttackPressed;
			Debug.Log("OnDisable");
		}

		private void HandleAttackPressed()
		{
			if (this.MovementBase.controller.isGrounded)
			{
				this.GoTo(new AttackState(this.CharacterData.firstAttack));
			}
		}

		public void GoTo(IMoveState state)
		{
			MoveState?.ExitState(this);

			MoveState = state;

			MoveState?.EnterState(this);

			OnStateChanged?.Invoke(state);
		}
	}
}
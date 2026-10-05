using System.Collections;
using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script
{
	public class AttackState : IMoveState
	{
		private readonly AttackComboData attack;
		private bool bufferedInput;

		public AttackState(AttackComboData attack)
		{
			this.attack = attack;
		}

		private void OnAttackPressed()
		{
			bufferedInput = true;
		}

		public void EnterState(CharacterSetup characterSetup)
		{
			characterSetup.ReadInput.IsFighting = true;

			characterSetup.Animator.CrossFadeInFixedTime(attack.animStateName, 0.1f);
			characterSetup.ReadInput.OnAttackPressed += OnAttackPressed;
			characterSetup.StartCoroutine(RunAttack(characterSetup));
			Debug.Log("attack");
		}

		public void UpdateState(CharacterSetup characterSetup)
		{
		}

		public void ExitState(CharacterSetup characterSetup)
		{
			characterSetup.ReadInput.OnAttackPressed -= OnAttackPressed;
		}

		private IEnumerator RunAttack(CharacterSetup characterSetup)
		{
			var animator = characterSetup.Animator;
			var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			float clipLength = stateInfo.length;
			bool advancedToNext = false;

			while (true)
			{
				float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
				bool windowOpen = normalizedTime >= attack.comboWindowStart && normalizedTime <= attack.comboWindowEnd;


				if (windowOpen && bufferedInput && attack.nextAttack != null)
				{
					advancedToNext = true;
					characterSetup.GoTo(new AttackState(attack.nextAttack));
					yield break;
				}


				if (normalizedTime > attack.comboWindowEnd)
					break;
				yield return null;
			}

			if (!advancedToNext)
			{
				float remaining = clipLength * (1f - animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f);
				if (remaining > 0f)
					yield return new WaitForSeconds(remaining);

				characterSetup.Animator.CrossFadeInFixedTime("FightState", attack.exitCrossFadeDuration);
				characterSetup.GoTo(new FightState());
			}
		}
	}
}
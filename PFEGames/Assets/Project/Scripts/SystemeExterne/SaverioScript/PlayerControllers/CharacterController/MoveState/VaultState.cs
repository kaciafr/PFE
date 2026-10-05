using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace CharacterController.Script
{
	public class VaultState : IMoveState
	{
		private readonly VaultAnimData animData;
		private readonly Vector3 handTarget;

		public VaultState(Vector3 handTarget, VaultAnimData animData)
		{
			this.handTarget = handTarget;
			this.animData = animData;
		}

		public void EnterState(CharacterSetup characterSetup)
		{
			characterSetup.Animator.CrossFadeInFixedTime(animData.animStateName, 0f);
			characterSetup.StartCoroutine(DoVaultWithMatchTarget(characterSetup));
		}

		public void UpdateState(CharacterSetup characterSetup)
		{
		}

		public void ExitState(CharacterSetup characterSetup)
		{
			characterSetup.Animator.CrossFadeInFixedTime("NormalState", 0.01f);
		}

		private IEnumerator DoVaultWithMatchTarget(CharacterSetup characterSetup)
		{
			yield return null;

			var animator = characterSetup.Animator;
			var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < animData.matchTargetTime)
			{
				var normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

				if (normalizedTime >= animData.matchStartTime && normalizedTime <= animData.matchTargetTime)
					animator.MatchTarget(
						handTarget,
						characterSetup.transform.rotation,
						animData.matchBodyPart,
						new MatchTargetWeightMask(animData.matchPosWeight, 0f),
						animData.matchStartTime,
						animData.matchTargetTime
					);
				yield return null;
			}

			yield return new WaitForSeconds(stateInfo.length * (1f - animData.matchTargetTime));
			characterSetup.GoTo(new NormalWalk());
		}
	}
}
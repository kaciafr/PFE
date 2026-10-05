using System.Collections;
using System.Collections.Generic;
using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script
{
	public class FightState : IMoveState
	{
		public void EnterState(CharacterSetup characterSetup)
		{
			characterSetup.Animator.SetBool("fight", true);
			characterSetup.MovementBase.AddOrRemoveSpeed(-0.25f);
		}


		public void UpdateState(CharacterSetup characterSetup)
		{
			if (!characterSetup.ReadInput.IsFighting) characterSetup.GoTo(new NormalWalk());
		}

		public void ExitState(CharacterSetup characterSetup)
		{
			characterSetup.MovementBase.AddOrRemoveSpeed(+0.25f);
			characterSetup.Animator.SetBool("fight", false);
		}
	}
}
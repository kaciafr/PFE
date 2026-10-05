using CharacterController.Script.PlayerControllers.Utilities;
using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script.PlayerControllers
{
	public class CrounchState : IMoveState
	{
		public void EnterState(CharacterSetup characterSetup)
		{
			characterSetup.Animator.SetBool("crounch", true);
			characterSetup.MovementBase.AddOrRemoveSpeed(-0.75f);
			Debug.Log("crounch");
		}

		public void UpdateState(CharacterSetup characterSetup)
		{
			var characterController = characterSetup.MovementBase.controller;
			var data = characterSetup.CharacterData;

			characterController.height = Mathf.Lerp(characterController.height, data.cruchHeight, 0.3f);
			characterController.center =
				Vector3.Lerp(characterController.center, new Vector3(0, data.cruchCenter, 0), 0.3f);

			var wantsToExit = !characterController.isGrounded || !characterSetup.ReadInput.IsCrunching;
			var checkDistance = data.standHeight - characterController.height;
			var origin = characterSetup.transform.position + Vector3.up * characterController.height;

			var hasCeiling = RaycastChecker.Check(origin, Vector3.up, checkDistance, GameLayers.Wall, out var hit,
				new Color(0, 1, 0));

			if (hasCeiling)
			{
				characterSetup.ReadInput.IsRunning = false;
			}

			if (wantsToExit && !hasCeiling)
			{
				characterController.height = characterSetup.CharacterData.standHeight;
				characterController.center = new Vector3(0, data.standCenter, 0);
				characterSetup.GoTo(new NormalWalk());
			}
		}

		public void ExitState(CharacterSetup characterSetup)
		{
			characterSetup.MovementBase.AddOrRemoveSpeed(+0.75f);
			characterSetup.Animator.SetBool("crounch", false);
		}
	}
}
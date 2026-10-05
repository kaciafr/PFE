using CharacterController.Script.PlayerControllers;
using Script.PlayerControllers;

namespace CharacterController.Script
{
	public class NormalWalk : IMoveState
	{
		public void EnterState(CharacterSetup characterSetup)
		{
			characterSetup.Animator.SetTrigger("Normal");
		}

		public void UpdateState(CharacterSetup characterSetup)
		{
			if (characterSetup.ReadInput.IsCrunching) characterSetup.GoTo(new CrounchState());
			if (characterSetup.ReadInput.IsFighting) characterSetup.GoTo(new FightState());
		}

		public void ExitState(CharacterSetup characterSetup)
		{
		}
	}
}
using UnityEngine;

namespace Characters
{
	public class GrabRagdollBodyState : PlayerState
	{
		public override void Enter(PlayerStateMachine ctx)
		{
			Debug.Log("Entered GrabRagdollBodyState");
			ctx.Player.RagdollGrabing();
			
			ctx.Player.Animator.Play(AnimIds.Grab);
			ctx.Player.Animator.SetGrabSpeed(0f);
		}

		public override void Exit(PlayerStateMachine ctx)
		{
			ctx.Player.UnRagdollGrabing();
			ctx.Player.Animator.SetGrabSpeed(0f);
		}
	}
}
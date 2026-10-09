using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
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

		public override void Tick(PlayerStateMachine ctx)
		{
			if (!ctx.Player.Input.GrabHeld )
			{
				ctx.SwitchState(ctx.IdleState);
				ctx.Player.Animator.Play(AnimIds.Grab);
			}
		}

		public override void FixedTick(PlayerStateMachine ctx)
		{
			MoveAt(ctx,ctx.Player.Settings.Grab.PushPullSpeed);
		}
		

		public override void Exit(PlayerStateMachine ctx)
		{
			ctx.Player.UnRagdollGrabing();
			ctx.Player.Animator.SetGrabSpeed(0f);
		}
	}
}
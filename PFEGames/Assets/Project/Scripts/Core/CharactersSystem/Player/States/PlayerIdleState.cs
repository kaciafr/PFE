using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerIdleState : PlayerState
    {

        public override void Enter(PlayerStateMachine ctx)
        {

            ctx.Player.Animator.Play(AnimIds.Idle , 0.2f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {

            if (IsFalling(ctx))
            {
                ctx.SwitchState(ctx.FallState);
                return;
            }
            if (ctx.Player.Input.GrabHeld && ctx.Player.CanGrab)
            {
                ctx.SwitchState(ctx.GrabState);
                return;
            }

            if (ctx.Player.Input.GrabHeld && ctx.Player.CanGrabRagdoll)
            {
                ctx.SwitchState(ctx.GrabRagdollState);
                return;
            }

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (ctx.Player.Input.InteractPressed && ctx.Player.CanPickUp)
            {
                ctx.SwitchState(ctx.InteractObjectState);
                return;
            }


            if (ctx.Player.Input.CrouchHeld)
            {
                ctx.SwitchState(ctx.CrouchState);
                return;
            }

            if (ctx.Player.Input.InteractPressed && ctx.Player.CanInteract)
            {
                ctx.SwitchState(ctx.InteractState);
                return;
            }


            if (ctx.Player.WantsJump)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }

            if (ctx.Player.MoveInput.sqrMagnitude > 0.01f)
            {
                ctx.SwitchState(ctx.MoveState);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.SlowDown();
        }
    }


}
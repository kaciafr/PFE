using UnityEngine;

namespace Characters
{
    public class PlayerMoveState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entered PlayerMoveState");
            ctx.Player.Animator.Play(AnimIds.Move);
        }

        public override void Tick(PlayerStateMachine ctx)
        {

            if (!HasMoveInput(ctx))
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }
            

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (ctx.Player.Input.InteractPressed && ctx.Player.CanPickUp)
            {
                ctx.SwitchState(ctx.InteractState);
                return;
            }

            if (ctx.Player.Input.CrouchHeld)
            {
                ctx.SwitchState(ctx.CrouchState);
                return;
            }

            if (ctx.Player.Input.JumpPressed && ctx.Player.Sensor.IsGrounded)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }

            if (ctx.Player.Input.SprintHeld)
            {
                ctx.SwitchState(ctx.SprintState);
                return;
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Movement.WalkSpeed);
        }
    }
}
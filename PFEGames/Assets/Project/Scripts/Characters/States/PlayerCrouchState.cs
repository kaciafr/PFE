using UnityEngine;

namespace Characters
{
    public class PlayerCrouchState : PlayerState
    {
        public PlayerCrouchState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerCrouchState");
            ctx.movement.Crouch();
            ctx.animator.Play("Crouch");
        }

        public override void Tick()
        {
            if (ctx.movement.IsClimbing)
            {
                ctx.movement.UnCrouch();
                ctx.SwitchState(new PlayerClimbState(ctx));
                return;
            }

            ctx.movement.Move(ctx.inputAction.MoveValue);

            if (!ctx.inputAction.CrouchHeld && ctx.movement.CanStand)
            {
                ctx.movement.UnCrouch();
                ctx.SwitchState(new PlayerIdleState(ctx));
                return;
            }

            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded && ctx.movement.CanStand)
            {
                ctx.movement.UnCrouch();
                ctx.SwitchState(new PlayerJumpState(ctx));
                return;
            }
        }

        public override void Exit()
        {
            Debug.Log("Exiting PlayerCrouchState");
            ctx.movement.UnCrouch();
        }
    }
}
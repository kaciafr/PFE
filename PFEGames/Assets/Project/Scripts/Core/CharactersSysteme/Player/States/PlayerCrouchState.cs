using UnityEngine;

namespace Characters
{
    public class PlayerCrouchState : PlayerState
    {
        private bool isMoving;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerCrouchState");
            ctx.Player.Crouch();

            isMoving = HasMoveInput(ctx);
            ctx.Player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            var player = ctx.Player;

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (!player.Input.CrouchHeld && player.CanStand)
            {
                ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
                return;
            }

            if (player.Input.JumpPressed && player.IsGrounded && player.CanStand)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }

            bool moving = HasMoveInput(ctx);
            if (moving != isMoving)
            {
                isMoving = moving;
                player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Crouch.Speed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exiting PlayerCrouchState");
            ctx.Player.UnCrouch();
        }
    }
}
using UnityEngine;

namespace Characters
{
    public class PlayerCrouchState : PlayerState
    {
        public PlayerCrouchState(PlayerStateMachine ctx) : base(ctx) { }

        private bool isMoving;
        private const float MoveThreshold = 0.05f;

        public override void Enter()
        {
            Debug.Log("Entering PlayerCrouchState");
            ctx.movement.Crouch();

            isMoving = IsMoveInput();
            ctx.animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle);
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

            bool moving = IsMoveInput();
            if (moving != isMoving)
            {
                isMoving = moving;
                ctx.animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle);
            }
        }

        public override void Exit()
        {
            Debug.Log("Exiting PlayerCrouchState");
            ctx.movement.UnCrouch();
        }

        private bool IsMoveInput()
        {
            return ctx.inputAction.MoveValue.sqrMagnitude > MoveThreshold * MoveThreshold;
        }
    }
}
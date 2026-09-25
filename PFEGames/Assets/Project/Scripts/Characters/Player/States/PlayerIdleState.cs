using UnityEngine;

namespace Characters
{
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerIdleState");
            ctx.animator.Play(AnimIds.Idle);
        }

        public override void Tick()
        {
            if (ctx.inputAction.GrabHeld && ctx.movement.CanGrab)
            {
                ctx.SwitchState(new PlayerGrabState(ctx));
                return;
            }
            if (ctx.movement.IsClimbing)
            {
                ctx.SwitchState(new PlayerClimbState(ctx));
                return;
            }

            if (ctx.inputAction.CrouchHeld)
            {
                ctx.SwitchState(new PlayerCrouchState(ctx));
                return;
            }

            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerJumpState(ctx));
                return;
            }

            if (ctx.inputAction.MoveValue.sqrMagnitude > 0.01f)
            {
                ctx.SwitchState(new PlayerMoveState(ctx));
            }
        }
        
    }
    
    
}
using UnityEngine;

namespace Characters
{
    public class PlayerIdleState : PlayerState
    {

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerIdleState");
            ctx.Player.Animator.Play(AnimIds.Idle);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (ctx.Player.Input.GrabHeld && ctx.Player.CanGrab)
            {
                ctx.SwitchState(ctx.GrabState);
                return;
            }
            
            if (ctx.Player.Input.GrabHeld)
            {
	            ctx.SwitchState(ctx.GrabRagdollState);
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

            if (ctx.Player.MoveInput.sqrMagnitude > 0.01f)
            {
                ctx.SwitchState(ctx.MoveState);
            }
        }


        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.Stop();
        }
    }
    
    
}
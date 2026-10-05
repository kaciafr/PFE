using UnityEngine;

namespace Characters
{
    public class PlayerSprintState :  PlayerState
    {

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log ("Entered PlayerSprintState");
            ctx.Player.Animator.Play (AnimIds.Run);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log ("Exited PlayerSprintState");
        }

        public override void Tick(PlayerStateMachine ctx)
        {

            if (!ctx.Player.Input.SprintHeld || !HasMoveInput(ctx))
            {
                ctx.SwitchState(ctx.MoveState);
                return; 
            }
            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (ctx.Player.Input.JumpPressed && ctx.Player.Sensor.IsGrounded)
            {
                ctx.SwitchState(ctx.JumpState);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Movement.SprintSpeed);
        }
    }
}
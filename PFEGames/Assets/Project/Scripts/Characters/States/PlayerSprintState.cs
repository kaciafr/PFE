using UnityEngine;

namespace Characters
{
    public class PlayerSprintState :  PlayerState
    {
        public PlayerSprintState (PlayerStateMachine ctx ) :  base(ctx) {}

        public override void Enter()
        {
            Debug.Log ("Entered PlayerSprintState");
            ctx.movement.Sprint();
            ctx.animator.Play ("Sprint");
        }

        public override void Exit()
        {
            Debug.Log ("Exited PlayerSprintState");
            ctx.movement.UnSprint();
        }

        public override void Tick()
        {
            ctx.movement.Move(ctx.inputAction.MoveValue);

            if (!ctx.inputAction.SprintHeld || ctx.inputAction.MoveValue.sqrMagnitude < 0.01f)
            {
                ctx.SwitchState(new PlayerMoveState(ctx));
                return; 
            }
            if (ctx.movement.IsClimbing)
            {
                ctx.SwitchState(new PlayerClimbState(ctx));
                return;
            }

            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerJumpState(ctx));
            }
        }
    }
}
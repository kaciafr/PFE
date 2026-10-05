using UnityEngine;

namespace Characters
{
    public class PlayerClimbState : PlayerState
    {


        private const float MoveThreshold = 0.05f;

        private float climbInput;
        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerClimbState");
            climbInput = 0f;
            ctx.Player.StartClimb();
            ctx.Player.Animator.Play(AnimIds.Climb);
        }
        

        public override void Tick(PlayerStateMachine ctx)
        {
            
 
            climbInput = ctx.Player.MoveInput.y;
            if (Mathf.Abs(climbInput) < MoveThreshold)
                climbInput = 0f;
 
            ctx.Player.Animator.SetClimbSpeed(climbInput);
 
            if (climbInput < 0f && ctx.Player.Sensor.IsGrounded)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }
 
            if (!ctx.Player.LadderInFront)
            {
                ctx.SwitchState(ctx.ClimbTopState);
                return;
            }
        }
        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.Climb(climbInput);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnClimb();
            ctx.Player.Animator.SetClimbSpeed(0f);
        }
    }
}
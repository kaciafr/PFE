using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
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


            climbInput = Vector3.Dot(ctx.Player.MoveDirection, -ctx.Player.LadderNormal);
            if (Mathf.Abs(climbInput) < MoveThreshold)
                climbInput = 0f;

            ctx.Player.Animator.SetClimbSpeed(climbInput);

            if (climbInput < 0f && ctx.Player.Sensor.IsGrounded)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (climbInput > 0f && ctx.Player.Sensor.LadderTopReached)
            {
                ctx.SwitchState(ctx.ClimbTopState);
                return;
            }

            if (!ctx.Player.LadderInFront)
            {
                ctx.SwitchState(climbInput > 0f ? ctx.ClimbTopState : ctx.IdleState);
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
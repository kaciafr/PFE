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
            climbInput = 0f;
            ctx.Player.StartClimb();
            ctx.Player.Animator.Play(AnimIds.Climb);
        }


        public override void Tick(PlayerStateMachine ctx)
        {


            float stickUp = ctx.Player.MoveInput.y;
            float towardWall = Vector3.Dot(ctx.Player.MoveDirection, -ctx.Player.LadderNormal);
            climbInput = Mathf.Abs(stickUp) >= Mathf.Abs(towardWall) ? stickUp : towardWall;
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

            if (ctx.Player.Input.InteractPressed)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (ctx.Player.Input.JumpPressed)
            {
                ctx.SwitchState(ctx.JumpState);
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
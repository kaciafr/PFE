using UnityEngine;

namespace Characters
{
    public class PlayerClimbState : PlayerState
    {
        public PlayerClimbState(PlayerStateMachine ctx) : base(ctx) { }


        private const float MoveThreshold = 0.05f;

        public override void Enter()
        {
            Debug.Log("Entering PlayerClimbState");
            ctx.animator.Play(AnimIds.Climb);
        }

        public override void Tick()
        {
            if (ctx.movement.IsClimbingTop)
            {
                ctx.SwitchState(new PlayerClimbTopState(ctx));
                return;
            }

            if (!ctx.movement.IsClimbing)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
                return;
            }

            Vector2 input = ctx.inputAction.MoveValue;
            ctx.movement.Move(input);

            float climbInput = input.y;
            if (Mathf.Abs(climbInput) < MoveThreshold)
                climbInput = 0f;

            ctx.animator.SetClimbSpeed(climbInput);
        }

        public override void Exit()
        {
            ctx.animator.SetClimbSpeed(0f);
        }
    }
}
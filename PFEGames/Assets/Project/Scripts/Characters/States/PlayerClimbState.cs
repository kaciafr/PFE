using UnityEngine;

namespace Characters
{
    public class PlayerClimbState : PlayerState
    {
        public PlayerClimbState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerClimbState");
            ctx.animator.Play("Climb");
        }

        public override void Tick()
        {
            ctx.movement.Move(ctx.inputAction.MoveValue);
            ctx.animator.SetClimbSpeed(Mathf.Clamp(ctx.movement.VerticalVelocity, -1f, 1f));

            if (ctx.movement.IsClimbingTop)
            {
                ctx.SwitchState(new PlayerClimbTopState(ctx));
                return;
            }

            if (!ctx.movement.IsClimbing)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
            }
        }
    }
}
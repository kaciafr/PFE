using UnityEngine;

namespace Characters
{
    public class PlayerClimbTopState : PlayerState
    {
        public PlayerClimbTopState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerClimbTopState");
            ctx.animator.Play(AnimIds.ClimbTop);
        }

        public override void Tick()
        {
            if (!ctx.movement.IsClimbingTop)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
            }
        }
    }
}
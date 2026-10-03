using UnityEngine;

namespace Characters
{
    public class PlayerJumpState : PlayerState
    {
        private const float MinAirTime = 0.15f;
        private float timer;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerJumpState");
            timer = 0f;
            ctx.Player.Jump();
            ctx.Player.Animator.Play(AnimIds.Jump);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;
            if (timer < MinAirTime) return;

            if (ctx.Player.IsGrounded && ctx.Player.VerticalVelocity <= 0.01f)
            {
                ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Movement.WalkSpeed);
        }
    }
}
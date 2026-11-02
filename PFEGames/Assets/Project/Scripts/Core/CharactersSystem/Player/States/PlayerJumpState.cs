using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerJumpState : PlayerState
    {
        private const float MinAirTime = 0.15f;
        private float timer;
        private float airSpeed;
        private float maxFallSpeed;

        public override void Enter(PlayerStateMachine ctx)
        {
            timer = 0f;

            ctx.Player.ConsumeJump();

            airSpeed = ctx.Player.Input.SprintHeld
                ? ctx.Player.Settings.Movement.SprintSpeed
                : ctx.Player.Settings.Movement.WalkSpeed;

            ctx.Player.Jump();
            ctx.Player.Animator.Play(AnimIds.Jump , 0.05f );

            maxFallSpeed = 0f;
            ctx.Player.Juice?.OnJump();
            ctx.Player.VFX?.OnJump();
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;
            if (timer < MinAirTime) return;

            if (ctx.Player.VerticalVelocity <= 0.01f)
                TryLand(ctx, maxFallSpeed);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.MoveinAir(ctx.Player.MoveInput, airSpeed);
            maxFallSpeed = Mathf.Max(maxFallSpeed, -ctx.Player.VerticalVelocity);
        }

        public override void Exit(PlayerStateMachine ctx)
        {

        }
    }
}

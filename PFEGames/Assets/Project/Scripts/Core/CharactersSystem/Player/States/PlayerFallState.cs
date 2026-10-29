using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerFallState : PlayerState
    {
        private const float MinAirTime = 0.15f;
        private float timer;
        private float airSpeed;
        private float maxFallSpeed;

        public override void Enter(PlayerStateMachine ctx)
        {

            airSpeed = ctx.Player.Input.SprintHeld
                ? ctx.Player.Settings.Movement.SprintSpeed
                : ctx.Player.Settings.Movement.WalkSpeed;

            ctx.Player.Animator.Play(AnimIds.Fall, 0.05f);

            maxFallSpeed = 0f;
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (ctx.Player.WantsJump)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }
            ;

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

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
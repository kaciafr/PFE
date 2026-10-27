using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerCrouchState : PlayerState
    {
        private bool isMoving;

        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.Crouch();

            isMoving = HasMoveInput(ctx);
            ctx.Player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle, 0.2f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            var player = ctx.Player;
            player.Animator.SetMoveSpeed(player.HorizontalSpeed / player.Settings.Crouch.Speed);

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (!player.Input.CrouchHeld && player.CanStand)
            {
                ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
                return;
            }

            if (player.WantsJump && player.CanStand)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }

            bool moving = HasMoveInput(ctx);
            if (moving != isMoving)
            {
                isMoving = moving;
                player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle, 0.2f);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Crouch.Speed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnCrouch();
        }
    }
}
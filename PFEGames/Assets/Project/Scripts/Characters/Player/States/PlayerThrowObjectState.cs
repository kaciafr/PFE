using UnityEngine;

namespace Characters
{
    public class PlayerThrowObjectState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entered PlayerThrowObjectState");
            ctx.Player.SetAiming(true);
            ctx.Player.Animator.Play(AnimIds.Hold);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            var player = ctx.Player;

            if (!player.IsHolding)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (player.Input.ThrowPressed)
            {
                player.Throw();
                player.Animator.Play(AnimIds.Throw);
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (player.Input.InteractPressed)
            {
                player.DropObject();
                ctx.SwitchState(ctx.IdleState);
                return;
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Movement.WalkSpeed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exited PlayerThrowObjectState");
            ctx.Player.SetAiming(false);

            if (ctx.Player.IsHolding)
                ctx.Player.DropObject();
        }
    }
}
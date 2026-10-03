using UnityEngine;

namespace Characters
{
    public class PlayerThrowObjectState : PlayerState
    {
        private bool isMoving;

        // Phase de lancer : l'anim Throw joue, l'objet reste en main jusqu'à ReleaseTime
        private bool isThrowing;
        private float throwTimer;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entered PlayerThrowObjectState");
            isThrowing = false;
            throwTimer = 0f;
            ctx.Player.SetAiming(true);

            isMoving = HasMoveInput(ctx);
            ctx.Player.Animator.Play(isMoving ? AnimIds.HoldWalk : AnimIds.Hold);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            var player = ctx.Player;

            if (isThrowing)
            {
                TickThrow(ctx);
                return;
            }

            if (!player.IsHolding)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            player.AdjustThrowPower(player.Input.AimScroll);

            if (player.Input.ThrowPressed)
            {
                isThrowing = true;
                throwTimer = 0f;
                player.Animator.Play(AnimIds.Throw);
                return;
            }

            if (player.Input.InteractPressed)
            {
                player.DropObject();
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            bool moving = HasMoveInput(ctx);
            if (moving != isMoving)
            {
                isMoving = moving;
                player.Animator.Play(isMoving ? AnimIds.HoldWalk : AnimIds.Hold);
            }
        }

        private void TickThrow(PlayerStateMachine ctx)
        {
            var player = ctx.Player;
            var settings = player.Settings.Throw;
            throwTimer += Time.deltaTime;

            if (player.IsHolding && throwTimer >= settings.ReleaseTime)
                player.Throw();

            if (throwTimer >= settings.ThrowDuration)
                ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            if (isThrowing)
                ctx.Player.Stop();
            else
                MoveAt(ctx, ctx.Player.Settings.Movement.WalkSpeed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exited PlayerThrowObjectState");
            isThrowing = false;
            ctx.Player.SetAiming(false);

            if (ctx.Player.IsHolding)
                ctx.Player.DropObject();
        }
    }
}

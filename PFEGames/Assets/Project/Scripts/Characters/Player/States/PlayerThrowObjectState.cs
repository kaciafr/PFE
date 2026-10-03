using UnityEngine;

namespace Characters
{
    public class PlayerThrowObjectState : PlayerState
    {
        private bool isMoving;
        private PlayerManager player;

        // Phase de lancer : l'anim Throw joue, l'objet reste en main jusqu'à l'Animation Event OnThrowRelease
        private bool isThrowing;
        private float throwTimer;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entered PlayerThrowObjectState");
            player = ctx.Player;
            isThrowing = false;
            throwTimer = 0f;
            player.SetAiming(true);

            if (player.AnimEvents != null)
                player.AnimEvents.ThrowRelease += OnThrowRelease;

            isMoving = HasMoveInput(ctx);
            player.Animator.Play(isMoving ? AnimIds.HoldWalk : AnimIds.Hold);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
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

        private void OnThrowRelease()
        {
            if (isThrowing && player.IsHolding)
                player.Throw();
        }

        private void TickThrow(PlayerStateMachine ctx)
        {
            throwTimer += Time.deltaTime;

            if (throwTimer < player.Settings.Throw.ThrowDuration) return;

            // Filet de sécurité si l'event n'est pas posé sur le clip Throw
            if (player.IsHolding)
            {
                Debug.LogWarning("Throw : Animation Event OnThrowRelease manquant sur le clip Throw.");
                player.Throw();
            }

            ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            if (isThrowing)
                player.Stop();
            else
                MoveAt(ctx, player.Settings.Movement.WalkSpeed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exited PlayerThrowObjectState");
            isThrowing = false;
            player.SetAiming(false);

            if (player.AnimEvents != null)
                player.AnimEvents.ThrowRelease -= OnThrowRelease;

            if (player.IsHolding)
                player.DropObject();
        }
    }
}

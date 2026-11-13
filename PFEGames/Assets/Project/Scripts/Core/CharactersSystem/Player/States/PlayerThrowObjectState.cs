using Goblfin.CharactersSystem.Player.Data;
using Goblfin.Player;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerThrowObjectState : PlayerState
    {
        private bool isMoving;
        private PlayerManager player;

        private bool isThrowing;
        private float throwTimer;
        private float holdTimer;
        private bool pickupRelease;
        private const float MinHoldTime = 0.2f;

        public override void Enter(PlayerStateMachine ctx)
        {
            player = ctx.Player;
            isThrowing = false;
            throwTimer = 0f;
            holdTimer = 0f;
            pickupRelease = player.Input.InteractHeld;
            player.SetAiming(false);

            if (player.AnimEvents != null)
                player.AnimEvents.ThrowRelease += OnThrowRelease;

            isMoving = HasMoveInput(ctx);
            player.Animator.Play(isMoving ? AnimIds.HoldWalk : AnimIds.Hold , 0.2f);
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

            player.Animator.SetMoveSpeed(player.HorizontalSpeed / player.Settings.Movement.WalkSpeed);

            if (player.Input.InteractHeld)
            {
                holdTimer += Time.deltaTime;
                player.ChargeThrowPower(Time.deltaTime);
            }

            player.SetAiming(player.Input.InteractHeld);

            if (player.Input.InteractReleased)
            {
                if (holdTimer >= MinHoldTime)
                {
                    isThrowing = true;
                    throwTimer = 0f;
                    player.Animator.Play(AnimIds.Throw , 0.05f);
                    return;
                }

                if (!pickupRelease)
                {
                    player.DropObject();
                    ctx.SwitchState(ctx.IdleState);
                    return;
                }

                pickupRelease = false;
                player.ResetThrowPower();
                holdTimer = 0f;
            }

            bool moving = HasMoveInput(ctx);
            if (moving != isMoving)
            {
                isMoving = moving;
                player.Animator.Play(isMoving ? AnimIds.HoldWalk : AnimIds.Hold , 0.2f);
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

            if (player.IsHolding)
            {
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
            isThrowing = false;
            player.SetAiming(false);

            if (player.AnimEvents != null)
                player.AnimEvents.ThrowRelease -= OnThrowRelease;

            if (player.IsHolding)
                player.DropObject();
        }
    }
}

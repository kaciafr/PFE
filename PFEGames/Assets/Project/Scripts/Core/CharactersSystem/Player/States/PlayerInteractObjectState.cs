using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerInteractObjectState : PlayerState
    {
        private float timer;
        private Rigidbody target;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entered PlayerInteractObjectState");
            timer = 0f;
            target = ctx.Player.ThrowableInFront;
            ctx.Player.Animator.Play(AnimIds.Interact);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;
            if (timer < ctx.Player.Settings.Throw.PickupDuration) return;

            if (target == null)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            ctx.Player.PickUp(target);
            ctx.SwitchState(ctx.ThrowState);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.Stop();
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exited PlayerInteractObjectState");
            target = null;
        }
    }
}
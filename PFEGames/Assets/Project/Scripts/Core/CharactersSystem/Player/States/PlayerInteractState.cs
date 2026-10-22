using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerInteractState : PlayerState
    {
        private float timer;
        private const float Duration = 0.3f;
        public override void Enter(PlayerStateMachine ctx)
        {
            timer = 0f;
            ctx.Player.Animator.Play(AnimIds.Idle);

            ctx.Player.Interact();
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (DialogueManager.IsOpen)
            {
                ctx.SwitchState(ctx.DialogueState);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= Duration)
                ctx.SwitchState(ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx) => ctx.Player.Stop();

        public override void Exit(PlayerStateMachine ctx){}
    }
}

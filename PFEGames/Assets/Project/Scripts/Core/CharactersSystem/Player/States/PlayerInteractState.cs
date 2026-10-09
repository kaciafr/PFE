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
            Debug.Log("Entered PlayerInteractState");
            timer = 0f;

            // Une seule interaction par entrée dans l'état (= un appui sur la touche)
            ctx.Player.Interact();
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;
            if (timer >= Duration)
                ctx.SwitchState(ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx) => ctx.Player.Stop();

        public override void Exit(PlayerStateMachine ctx){}
    }
}

using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerClimbTopState : PlayerState
    {
        private float timer;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerClimbTopState");
            timer = 0f;
            ctx.Player.StartClimbTop();
            ctx.Player.Animator.Play(AnimIds.ClimbTop);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;

            if (timer >= ctx.Player.Settings.Climb.TopDuration)
            {
                ctx.Player.FinishClimbTop();
                ctx.SwitchState(ctx.IdleState);
            }
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.EndClimbTop();
        }
    }
}
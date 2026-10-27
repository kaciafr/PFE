
using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerLandState : PlayerState
    {
        private float timer;
        public override void Enter(PlayerStateMachine ctx)
        {
            timer = 0f; 
            ctx.Player.Animator.Play(AnimIds.Land, 0.05f);
            
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;
            if (timer< ctx.Player.Settings.Jump.HardLandDuration) return;
            ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.SlowDown();
            
        }
    }
}
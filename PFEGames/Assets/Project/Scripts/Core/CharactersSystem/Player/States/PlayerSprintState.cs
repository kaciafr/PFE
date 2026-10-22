using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerSprintState :  PlayerState
    {

        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.Animator.Play (AnimIds.Run);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
        }

        public override void Tick(PlayerStateMachine ctx)
        {

            if (!ctx.Player.Input.SprintHeld || !HasMoveInput(ctx))
            {
                ctx.SwitchState(ctx.MoveState);
                return; 
            }
            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (ctx.Player.WantsJump)
            {
                ctx.SwitchState(ctx.JumpState);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            MoveAt(ctx, ctx.Player.Settings.Movement.SprintSpeed);
        }
    }
}
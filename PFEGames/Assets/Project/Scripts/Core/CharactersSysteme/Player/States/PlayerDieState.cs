using UnityEngine;

namespace Characters
{
    public class PlayerDieState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering Die State");
            ctx.Player.Input.DisableAllInput();
            ctx.Player.Stop();
            ctx.Player.Animator.Play(AnimIds.Die);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.Input.EnablePlayerInput();
        }
    }
}
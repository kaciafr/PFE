using Goblfin.CharactersSystem.Player.Component;
using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerDieState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.Input.DisableAllInput();
            ctx.Player.Stop();

            PlayerRagdoll ragdoll = ctx.Player.GetComponentInChildren<PlayerRagdoll>();
            if (ragdoll != null)
                ragdoll.Kill();
            else
                ctx.Player.Animator.Play(AnimIds.Die);

            ctx.Player.Respawn(ctx.Player.Settings.Death.RespawnDelay);
        }



        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.Input.EnablePlayerInput();
        }
    }
}

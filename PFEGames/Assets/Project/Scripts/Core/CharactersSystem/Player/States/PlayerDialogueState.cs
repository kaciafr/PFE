using Goblfin.CharactersSystem.Player.Data;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerDialogueState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.Stop();
            ctx.Player.Animator.Play(AnimIds.Idle);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (ctx.Player.Input.InteractPressed)
            {
                DialogueManager.instance.Continue();
            }

            if (!DialogueManager.IsOpen)
            {
                ctx.SwitchState(ctx.IdleState);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            ctx.Player.Stop();
        }
        
        public override void Exit(PlayerStateMachine ctx){}
    }
}
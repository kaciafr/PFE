namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerHideState : PlayerState
    {
        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.SetFrozen(true);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (ctx.Player.HideSpot == null)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (ctx.Player.Input.InteractPressed)
                ctx.Player.HideSpot.RequestExit();
        }

        public override void FixedTick(PlayerStateMachine ctx) { }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.SetFrozen(false);
            ctx.Player.SetVisible(true);
        }
    }
}

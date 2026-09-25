using UnityEngine;

namespace Characters
{
    public class PlayerDieState: PlayerState
    {
        public PlayerDieState(PlayerStateMachine ctx) : base(ctx) { }
        
        

        public override void Enter()
        {
            ctx.inputAction.DisableAllInput();
            ctx.movement.StopStateMovement();
            Debug.Log("Entering Die State");
            ctx.animator.Play(AnimIds.Die);
        }
        
        public override void Exit(){}

        public override void Tick() {}
    }
}
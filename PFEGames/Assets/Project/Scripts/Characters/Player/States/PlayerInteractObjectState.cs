using UnityEngine;

namespace Characters
{
    public class PlayerInteractObjectState: PlayerState
    {
        public PlayerInteractObjectState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entered PlayerInteractObjectState");
            ctx.animator.Play(AnimIds.InteractObject);
        }

        public override void Exit()
        {
            Debug.Log("Existing PlayerInteractObjectState");
        }
        public override void Tick()
        {
            
        }
    }
}
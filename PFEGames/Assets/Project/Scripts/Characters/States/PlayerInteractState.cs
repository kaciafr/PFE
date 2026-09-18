using UnityEngine;

namespace Characters.States
{
    public class PlayerInteractState: PlayerState
    {
        public PlayerInteractState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entered PlayerInteractState");
        }

        public override void Tick()
        {
            //if ()
            //{
                //ctx.SwitchState(new PlayerState(ctx));
            //}
        }
    }
}
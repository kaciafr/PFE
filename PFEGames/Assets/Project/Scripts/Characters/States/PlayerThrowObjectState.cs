using UnityEngine;

namespace Characters
{
    public class PlayerThrowObjectState :  PlayerState
    {
        public PlayerThrowObjectState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log ("Entered PlayerThrowObjectState");
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
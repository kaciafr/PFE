using UnityEngine;

namespace Characters.States
{
    public class PlayerSprintState :  PlayerState
    {
        public PlayerSprintState (PlayerStateMachine ctx ) :  base(ctx) {}

        public override void Enter()
        {
            Debug.Log ("Entered PlayerSprintState");
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
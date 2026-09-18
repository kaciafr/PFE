using Characters;
using UnityEngine;

namespace Characters
{
    public class PlayerCrouchState: PlayerState
    {
        public PlayerCrouchState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerIdleState");
        }

        public override void Tick()
        {
           // if ()
           // {
                //ctx.SwitchState(new PlayerState(ctx));
            //}
        }
    }
}
using UnityEngine;

namespace Characters.States
{
    public class PlayerVaultState : PlayerState
    {
        public PlayerVaultState(PlayerStateMachine ctx) : base(ctx) {}

        public override void Enter()
        {
            Debug.Log ("Entered PlayerVaultState");

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
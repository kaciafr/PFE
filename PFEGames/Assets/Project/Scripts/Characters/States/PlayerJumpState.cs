using Characters.States;
using UnityEngine;

namespace Characters
{
    public class PlayerJumpState :  PlayerState
    {
        public PlayerJumpState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerJumpState"); 
        }

        public override void Tick()
        {
            //ctx.movement.Jump(ctx.inputAction.);

            if (ctx.movement.VerticalVelocity < 0f && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
            } ; 
            
        }
    }
}
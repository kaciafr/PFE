using UnityEngine;

namespace Characters.States
{
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }

        public override void Enter()
        {
            Debug.Log("Entering PlayerIdleState");
        }

        public override void Tick()
        {
            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerJumpState(ctx));
                return; 
            }

            if (ctx.inputAction.MoveValue.sqrMagnitude > 0.01f)
            {
                ctx.SwitchState(new PlayerMoveState(ctx));
            }
        }
        
    }
    
    
}
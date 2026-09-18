using Characters;
using Characters.States;
using UnityEngine;

    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerStateMachine ctx) : base(ctx) { }
        
        
        public override void Enter()
        {
            Debug.Log("Entered PlayerMoveState");
        }

        public override void Tick()
        {
            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerJumpState(ctx));
                return;
            }
            
            
            
            ctx.movement.Move(ctx.inputAction.MoveValue);
            //ctx.animator.SetDirection(ctx.inputAction.MoveValue);
            if (ctx.inputAction.MoveValue.sqrMagnitude < 0.01f)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
                Debug.Log(ctx.inputAction.MoveValue);

            }

            
        }
    }

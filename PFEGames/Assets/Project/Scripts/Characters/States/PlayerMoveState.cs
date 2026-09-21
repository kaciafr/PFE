using Characters;
using UnityEngine;

    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerStateMachine ctx) : base(ctx) { }
        
        
        public override void Enter()
        {
            Debug.Log("Entered PlayerMoveState");
            ctx.animator.Play("Walk");
        }

        public override void Tick()
        {
            if (ctx.movement.IsClimbing)
            {
                ctx.SwitchState(new PlayerClimbState(ctx));
                return;
            }

            if (ctx.inputAction.CrouchHeld)
            {
                ctx.SwitchState(new PlayerCrouchState(ctx));
                return;
            }
            if (ctx.inputAction.JumpPressed && ctx.movement.isGrounded)
            {
                ctx.SwitchState(new PlayerJumpState(ctx));
                
                return;
            }

            if (ctx.inputAction.SprintHeld)
            {
                ctx.SwitchState(new PlayerSprintState(ctx));
                return; 
            }
            
            
            ctx.movement.Move(ctx.inputAction.MoveValue);
            if (ctx.inputAction.MoveValue.sqrMagnitude < 0.01f)
            {
                ctx.SwitchState(new PlayerIdleState(ctx));
                return; 
                Debug.Log(ctx.inputAction.MoveValue);

            }

            
        }
    }

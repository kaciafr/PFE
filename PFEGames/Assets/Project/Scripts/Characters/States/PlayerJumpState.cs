using Characters;
using Characters.States;
using UnityEngine;


    public class PlayerJumpState :  PlayerState
    {
        public PlayerJumpState(PlayerStateMachine ctx) : base(ctx) { }

        public float timer;
        private float MinAirTime = 0.15f; 
        

        public override void Enter()
        {
            timer = 0f;
            ctx.movement.Jump();
            Debug.Log("Entering PlayerJumpState"); 
        }

        public override void Tick()
        {
            ctx.movement.Move(ctx.inputAction.MoveValue);
            timer += Time.deltaTime;
            //ctx.movement.Jump(ctx.inputAction.);
            
            if (timer<MinAirTime) return;

            if (ctx.movement.isGrounded && ctx.movement.VerticalVelocity <= 0.01)
            {
                if (ctx.inputAction.MoveValue.sqrMagnitude > 0.01f)
                {
                    ctx.SwitchState(new PlayerMoveState(ctx));
                }
                else
                {
                    ctx.SwitchState(new PlayerIdleState(ctx));
                }
            } ; 
            
        }
    }

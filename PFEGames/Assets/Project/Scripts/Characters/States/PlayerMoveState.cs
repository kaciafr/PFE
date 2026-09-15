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
            Debug.Log(ctx.inputAction.MoveValue);

            ctx.movement.Move(ctx.inputAction.MoveValue);
            ctx.animator.SetDirection(ctx.inputAction.MoveValue);
            if (ctx.inputAction.MoveValue.sqrMagnitude < 0.01f)          {
             ctx.SwitchState(new PlayerIdleState(ctx));
         }
        }
    }

using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerCrouchState : PlayerState
    {
        private bool isMoving;

        public override void Enter(PlayerStateMachine ctx)
        {
            ctx.Player.Crouch();

            isMoving = HasMoveInput(ctx);
            ctx.Player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle, 0.2f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {

            if (IsFalling(ctx))
            {
                ctx.SwitchState(ctx.FallState);
                return;
            }
            ctx.Player.Animator.SetMoveSpeed(ctx.Player.HorizontalSpeed / ctx.Player.Settings.Crouch.Speed);

            if (WantsToClimb(ctx))
            {
                ctx.SwitchState(ctx.ClimbState);
                return;
            }

            if (!ctx.Player.Input.CrouchHeld && ctx.Player.CanStand)
            {
                ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
                return;
            }

            if (ctx.Player.WantsJump && ctx.Player.CanStand)
            {
                ctx.SwitchState(ctx.JumpState);
                return;
            }

            bool moving = HasMoveInput(ctx);
            if (moving != isMoving)
            {
                isMoving = moving;
                ctx.Player.Animator.Play(isMoving ? AnimIds.Crouch : AnimIds.CrouchIdle, 0.2f);
            }
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            var sensor = ctx.Player.Sensor;
            Vector3 dir = ctx.Player.MoveDirection;
            float speed = ctx.Player.Settings.Crouch.Speed;

            if (!HasMoveInput(ctx) || sensor.HasGroundAhead(dir))
            {
                MoveAt(ctx, speed);
                return;
            }

            Vector3 dirX = new Vector3(dir.x, 0f, 0f);
            Vector3 dirZ = new Vector3(0f, 0f, dir.z);

            if (dirX.magnitude > 0.1f && sensor.HasGroundAhead(dirX))
                ctx.Player.MoveWorld(dirX, speed);
            else if (dirZ.magnitude > 0.1f && sensor.HasGroundAhead(dirZ))
                ctx.Player.MoveWorld(dirZ, speed);
            else
                ctx.Player.Motor.Stop();  
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnCrouch();
        }
        

    }
}
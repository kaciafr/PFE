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

            if (ctx.Player.Input.CrouchPressed && ctx.Player.CanStand)
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

            // Pas de sol devant : on essaie de glisser le long du bord.
            // On découpe la direction selon les axes de la CAMÉRA (gauche/droite et avant/arrière),
            // pas selon les axes X/Z du monde, sinon en vue de profondeur il part sur le côté.
            Vector2 input = ctx.Player.MoveInput;
            Vector3 dirSide = ctx.Player.Motor.GetMoveDirection(new Vector2(input.x, 0f));
            Vector3 dirDepth = ctx.Player.Motor.GetMoveDirection(new Vector2(0f, input.y));

            if (dirSide.magnitude > 0.1f && sensor.HasGroundAhead(dirSide))
                ctx.Player.MoveWorld(dirSide, speed);
            else if (dirDepth.magnitude > 0.1f && sensor.HasGroundAhead(dirDepth))
                ctx.Player.MoveWorld(dirDepth, speed);
            else
                ctx.Player.Motor.Stop();
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnCrouch();
        }
        

    }
}
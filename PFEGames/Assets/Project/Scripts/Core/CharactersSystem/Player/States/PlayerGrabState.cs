using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerGrabState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float PullThreshold = -0.1f;
        private float pushOrPull;
        private Vector3 moveDir;    
        private bool isPulling;
        private bool tooHeavy;

        public override void Enter(PlayerStateMachine ctx)
        {
            pushOrPull = 0f;
            moveDir = Vector3.zero;
            isPulling = false;

            Rigidbody crate = ctx.Player.Sensor.CrateInFront;
            tooHeavy = crate != null && crate.mass > ctx.Player.Settings.Grab.MaxGrabMass;

            ctx.Player.Grab();

            ctx.Player.Animator.Play(AnimIds.Grab , 0.15f);
            ctx.Player.Animator.SetGrabSpeed(0f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (!ctx.Player.Input.GrabHeld)
            {
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            if (!tooHeavy && !ctx.Player.IsGrabbing)
            {
                ctx.Player.BlockGrab();
                ctx.SwitchState(ctx.IdleState);
                return;
            }

            moveDir = Vector3.ClampMagnitude(ctx.Player.MoveDirection, 1f);
            if (moveDir.magnitude < MoveThreshold)
                moveDir = Vector3.zero;

            pushOrPull = Vector3.Dot(moveDir, ctx.Player.Motor.Forward);

            bool pulling = pushOrPull < PullThreshold;
            if (pulling != isPulling)
            {
                isPulling = pulling;
                ctx.Player.Animator.Play(isPulling ? AnimIds.Pull : AnimIds.Grab ,0.15f );
            }

            float animSpeed = tooHeavy
                ? moveDir.magnitude
                : ctx.Player.HorizontalSpeed / ctx.Player.Settings.Grab.PushPullSpeed;
            ctx.Player.Animator.SetGrabSpeed(animSpeed);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            if (tooHeavy || moveDir == Vector3.zero)
            {
                ctx.Player.Stop();
                return;
            }

            ctx.Player.MoveWorld(moveDir, ctx.Player.Settings.Grab.PushPullSpeed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnGrab();
            ctx.Player.Animator.SetGrabSpeed(0f);
        }
    }
}

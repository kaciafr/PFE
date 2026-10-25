using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerGrabState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float PullThreshold = -0.1f;
        private float pushOrPull;
        private bool isPulling;
        private bool tooHeavy;

        public override void Enter(PlayerStateMachine ctx)
        {
            pushOrPull = 0f;
            isPulling = false;

            Rigidbody crate = ctx.Player.Sensor.CrateInFront;
            tooHeavy = crate != null && crate.mass > ctx.Player.Settings.Grab.MaxGrabMass;

            ctx.Player.Grab();

            ctx.Player.Animator.Play(AnimIds.Grab);
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

            pushOrPull = Vector3.Dot(ctx.Player.MoveDirection, ctx.Player.Motor.Forward);
            if (Mathf.Abs(pushOrPull) < MoveThreshold)
                pushOrPull = 0f;

            bool pulling = pushOrPull < PullThreshold;
            if (pulling != isPulling)
            {
                isPulling = pulling;
                ctx.Player.Animator.Play(isPulling ? AnimIds.Pull : AnimIds.Grab);
            }

            // Caisse trop lourde : il force sur place, l'animation suit le joystick
            // Sinon : l'animation suit la vraie vitesse (caisse bloquée = pieds arrêtés)
            float animSpeed = tooHeavy
                ? Mathf.Abs(pushOrPull)
                : ctx.Player.HorizontalSpeed / ctx.Player.Settings.Grab.PushPullSpeed;
            ctx.Player.Animator.SetGrabSpeed(animSpeed);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            if (tooHeavy || pushOrPull == 0f)
            {
                ctx.Player.Stop();
                return;
            }

            ctx.Player.MoveWorld(ctx.Player.Facing * pushOrPull, ctx.Player.Settings.Grab.PushPullSpeed);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            ctx.Player.UnGrab();
            ctx.Player.Animator.SetGrabSpeed(0f);
        }
    }
}
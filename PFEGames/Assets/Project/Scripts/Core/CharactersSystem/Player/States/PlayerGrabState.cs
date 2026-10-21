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
            Debug.Log("Entered PlayerGrabState");
            pushOrPull = 0f;
            isPulling = false;

            // Trop lourd seulement si la masse dépasse le max (pas pour n'importe quel échec)
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

            // Le grab a raté (ou la caisse a été lâchée) : on sort au lieu de rester figé
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

            ctx.Player.Animator.SetGrabSpeed(Mathf.Abs(pushOrPull));
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
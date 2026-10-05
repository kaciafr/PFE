using UnityEngine;

namespace Characters
{
    public class PlayerGrabState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float PullThreshold = -0.1f;

        private float pushOrPull;
        private bool isPulling;

        public override void Enter(PlayerStateMachine ctx)
        {
            Debug.Log("Entering PlayerGrabState");
            pushOrPull = 0f;
            isPulling = false;
            ctx.Player.Grab();
            ctx.Player.Animator.Play(AnimIds.Grab);
            ctx.Player.Animator.SetGrabSpeed(0f);
        }

        public override void Exit(PlayerStateMachine ctx)
        {
            Debug.Log("Exiting PlayerGrabState");
            ctx.Player.UnGrab();
            ctx.Player.Animator.SetGrabSpeed(0f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            if (!ctx.Player.Input.GrabHeld || !ctx.Player.IsGrabbing)
            {
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
            if (pushOrPull != 0f)
                ctx.Player.MoveWorld(ctx.Player.Facing * pushOrPull, ctx.Player.Settings.Grab.PushPullSpeed);
            else
                ctx.Player.Stop();
        }
    }
}
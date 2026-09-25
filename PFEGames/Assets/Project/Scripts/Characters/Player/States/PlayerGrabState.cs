using UnityEngine;

namespace Characters
{
    public class PlayerGrabState : PlayerState

    {
    public PlayerGrabState(PlayerStateMachine ctx) : base(ctx) { }

    private const float MoveThreshold = 0.05f; 
    public override void Enter()
    {
        Debug.Log("Entering PlayerGrabState");
        ctx.movement.Grab();
        ctx.animator.Play(AnimIds.Grab);
    }

    public override void Exit()
    {
        Debug.Log("Exiting PlayerGrabState");
        ctx.movement.UnGrab();
        ctx.animator.SetGrabSpeed(0f);

    }

    public override void Tick()
    {
        if (!ctx.inputAction.GrabHeld || !ctx.movement.IsGrabbing)
        {
            ctx.SwitchState(new PlayerIdleState(ctx));
            return;
        }

        Vector2 grabinput = ctx.inputAction.MoveValue;
        grabinput.y = 0f; 

        if (Mathf.Abs(grabinput.x) < MoveThreshold)
            grabinput.x = 0f;

        ctx.animator.SetGrabSpeed(Mathf.Abs(grabinput.x));

        if (grabinput.x != 0f)
            ctx.movement.Move(grabinput);
        else
            ctx.movement.Stop();
    }


    }
}
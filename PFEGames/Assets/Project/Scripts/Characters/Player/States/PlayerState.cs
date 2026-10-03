

using UnityEngine;

namespace Characters
{
    public abstract class PlayerState
    {
        public virtual void Enter(PlayerStateMachine ctx) { }
        public virtual void Exit(PlayerStateMachine ctx) { }
        public virtual void Tick(PlayerStateMachine ctx) { }
        public virtual void FixedTick(PlayerStateMachine ctx) { }

        protected bool HasMoveInput(PlayerStateMachine ctx)=> ctx.Player.MoveInput.sqrMagnitude > 0.01f;
        

        protected void MoveAt(PlayerStateMachine ctx, float speed)
            => ctx.Player.Move(ctx.Player.MoveInput, speed);

        protected bool WantsToClimb(PlayerStateMachine ctx)
        {
          return !ctx.Player.Grabber.IsGrabbing
                   && ctx.Player.Sensor.LadderInFront
                   && Vector3.Dot(ctx.Player.MoveDirection, -ctx.Player.Sensor.LadderNormal) > ctx.Player.Settings.Climb.ApproachThreshold;
        }
        
    }
}
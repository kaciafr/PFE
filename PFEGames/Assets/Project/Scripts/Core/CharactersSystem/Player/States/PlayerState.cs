using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public abstract class PlayerState
    {
        public virtual void Enter(PlayerStateMachine ctx) { }
        public virtual void Exit(PlayerStateMachine ctx) { }
        public virtual void Tick(PlayerStateMachine ctx) { }
        public virtual void FixedTick(PlayerStateMachine ctx) { }

        protected bool HasMoveInput(PlayerStateMachine ctx)=> ctx.Player.MoveInput.sqrMagnitude > 0.01f;
        

        protected void MoveAt(PlayerStateMachine ctx, float speed , float rotationmultiplier = 1f)
            => ctx.Player.Move(ctx.Player.MoveInput, speed , rotationmultiplier );

        protected bool WantsToClimb(PlayerStateMachine ctx)
        {
          return ctx.Player.CanClimb
                   && !ctx.Player.Grabber.IsGrabbing
                   && ctx.Player.Sensor.LadderInFront
                   && ctx.Player.Input.InteractPressed;
        }

        protected bool IsFalling(PlayerStateMachine ctx) => !ctx.Player.IsGrounded;

        protected bool TryLand(PlayerStateMachine ctx , float maxSpeed)
        {
            if(!ctx.Player.IsGrounded) return false ;
            
            ctx.Player.Juice?.OnLand(maxSpeed);
            ctx.Player.VFX?.OnLand(maxSpeed);

            if (maxSpeed >= ctx.Player.Settings.Jump.HardLandSpeed)
                ctx.SwitchState(ctx.LandState);
            else ctx.SwitchState(HasMoveInput(ctx) ? ctx.MoveState : ctx.IdleState);
            
            return true;
        }
        
    }
}
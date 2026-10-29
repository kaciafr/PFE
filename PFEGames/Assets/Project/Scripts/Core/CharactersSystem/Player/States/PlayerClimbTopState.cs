using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.States
{
    public class PlayerClimbTopState : PlayerState
    {
        private float timer;
        private Vector3 startPos;
        private Vector3 targetPos;

        public override void Enter(PlayerStateMachine ctx)
        {
            timer = 0f;
            ctx.Player.StartClimbTop();
            
            startPos = ctx.Player.Motor.Position;
            targetPos = ctx.Player.Motor.GetClimbTopTarget(ctx.Player.LadderNormal, ctx.Player.Sensor.LadderTopY);

                
            ctx.Player.Animator.Play(AnimIds.ClimbTop , 0.1f);
        }

        public override void Tick(PlayerStateMachine ctx)
        {
            timer += Time.deltaTime;

            if (timer >= ctx.Player.Settings.Climb.TopDuration)
                ctx.SwitchState(ctx.IdleState);
        }

        public override void FixedTick(PlayerStateMachine ctx)
        {
            float t = Mathf.Clamp01(timer / ctx.Player.Settings.Climb.TopDuration);

            float tUp = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.6f));
            float tForward = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.4f) / 0.6f));

            Vector3 pos = startPos;
            pos.y = Mathf.Lerp(startPos.y, targetPos.y, tUp);
            pos.x = Mathf.Lerp(startPos.x, targetPos.x, tForward);
            pos.z = Mathf.Lerp(startPos.z, targetPos.z, tForward);

            ctx.Player.Motor.MoveKinematic(pos);

        }

        public override void Exit(PlayerStateMachine ctx)
        {            ctx.Player.EndClimbTop();
        }
    }
}
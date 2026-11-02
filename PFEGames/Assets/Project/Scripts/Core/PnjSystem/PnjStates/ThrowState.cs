using UnityEngine;
using UnityEngine.AI;

namespace Goblfin.PnjSystem
{
    public class ThrowState : IPnjStates
    {
        public StatePriority Priority => StatePriority.Chase;
        public float AgentSpeed => 0;
        
        private readonly IDetected target;
        private ThrowThingsManager thrower;
        private VisionCone vision;
        private float nextReachCheck;

        public ThrowState(IDetected target)
        {
            this.target = target;
        }

        public void EnterState(BrainPnj brainPnj)
        {
            thrower = brainPnj.GetAptitude<ThrowThingsManager>();
            vision = brainPnj.GetAptitude<VisionCone>();
            brainPnj.Agent.speed = AgentSpeed;
            nextReachCheck = Time.time + 0.5f;
        }

        public void UpdateState(BrainPnj brainPnj)
        {
            if (target == null || thrower == null || vision == null || thrower.RocksLeft == 0)
            {
                brainPnj.PnjGoTo(new SearchState(brainPnj,brainPnj.transform.position));
                return;
            }
            
            Vector3 targetPos = target.transform.position;
            if (!vision.CanSee(target))
            {
                brainPnj.PnjGoTo(new SearchState(brainPnj,brainPnj.transform.position));
                return;
            }
            
            if (!thrower.InRange(targetPos, 2.2f))              
            {
                brainPnj.PnjGoTo( new ChaseState(target));
                return;
            }

            if (Time.time >= nextReachCheck)
            {
                nextReachCheck = Time.time + 0.5f;
                if (thrower.CanReach(targetPos))
                {
                    brainPnj.PnjGoTo(new ChaseState(target));
                    return;
                }
            }
            
            Vector3 direction = targetPos - brainPnj.transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
                brainPnj.transform.rotation = Quaternion.Slerp(brainPnj.transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            
            
            thrower.TryThrow(targetPos);
        }

        public void ExitState(BrainPnj brainPnj)
        {
        }
    }
}
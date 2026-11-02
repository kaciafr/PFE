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
            
            Vector3 targetPos = brainPnj.transform.position;

            if (!vision.CanSee(target))
            {
                brainPnj.PnjGoTo(new SearchState(brainPnj,brainPnj.transform.position));
                return;
            }
            
            if (!thrower.InRange(targetPos, 1.2f))              // sortie plus large que l'entrée : pas de clignotement
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
            if (direction.sqrMagnitude > 0.001f)
                brainPnj.transform.rotation = Quaternion.Slerp(brainPnj.transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            
            
            thrower.TryThrow(targetPos);
            Debug.DrawLine(brainPnj.transform.position + Vector3.up, direction + Vector3.up, Color.darkGreen, 2f);
            Debug.Log($"cible={(target as Component)?.name} distance={Vector3.Distance(brainPnj.transform.position, direction):F1}");
            
        }

        public void ExitState(BrainPnj brainPnj)
        {
        }
    }
}
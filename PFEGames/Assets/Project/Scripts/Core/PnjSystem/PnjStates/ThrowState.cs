using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class ThrowState : IPnjStates
    {
        public StatePriority Priority => StatePriority.Chase;
        public float AgentSpeed => 0;

        private static readonly int ThrowHash = Animator.StringToHash("Throw");
        private const float ThrowInterval = 2.5f;     // durée du geste + une pause : à régler sur la longueur du clip
        private const float LostSightGrace = 5f;      // un rayon qui clignote ne le fait pas quitter l'état

        private readonly IDetected target;
        private ThrowThingsManager thrower;
        private VisionCone vision;
        private float nextReachCheck;
        private float nextThrowTime;
        private float lostSightTimer;
        private float previousSpeed;

        public ThrowState(IDetected target)
        {
            this.target = target;
        }

        public void EnterState(BrainPnj brainPnj)
        {
            Debug.Log("Throw State");
            thrower = brainPnj.GetAptitude<ThrowThingsManager>();
            vision = brainPnj.GetAptitude<VisionCone>();

            previousSpeed = brainPnj.Agent.speed;
            brainPnj.Agent.ResetPath();
            brainPnj.Agent.velocity = Vector3.zero;      
            brainPnj.Agent.speed = AgentSpeed;

            nextReachCheck = Time.time + 0.5f;
            nextThrowTime = Time.time;                    
            lostSightTimer = 0f;
        }

        public void UpdateState(BrainPnj brainPnj)
        {
            Vector3 targetPos = target.transform.position;

            if (!vision.CanSee(target))
            {
                lostSightTimer += Time.deltaTime;
                if (lostSightTimer > LostSightGrace)
                {
                    brainPnj.PnjGoTo(new SearchState(brainPnj, targetPos)); 
                    return;
                }
            }
            else
            {
                lostSightTimer = 0f;
            }
            
            if (target == null || thrower == null || vision == null
                || (thrower.RocksLeft == 0 && Time.time >= nextThrowTime))
            {
                brainPnj.PnjGoTo(new SearchState(brainPnj, brainPnj.transform.position));
                return;
            }
            
            if (!thrower.InRange(targetPos, 1.2f))
            {
                brainPnj.PnjGoTo(new ChaseState(target));
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
            {
                brainPnj.transform.rotation = Quaternion.Slerp(
                    brainPnj.transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 8f);
            }

            // un seul déclenchement par lancer
            if (Time.time >= nextThrowTime && thrower.RocksLeft > 0)
            {
                nextThrowTime = Time.time + ThrowInterval;
                thrower.SetTarget(targetPos);
                brainPnj.Animator.SetTrigger(ThrowHash);
            }
        }

        public void ExitState(BrainPnj brainPnj)
        {
            brainPnj.Animator.ResetTrigger(ThrowHash);
            brainPnj.Agent.speed = previousSpeed;
        }
    }
}
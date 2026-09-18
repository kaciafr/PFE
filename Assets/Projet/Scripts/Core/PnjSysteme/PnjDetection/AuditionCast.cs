using System;
using CharacterController.Script;
using UnityEngine;

namespace PnjDetection
{
    public class AuditionCast : DetectionSense
    {
        [Header("Réglages de la portée de l'audition")]
        [Tooltip("Rayon proche : une cible ici déclenche une alerte immédiate.")]
        [Range(1, 60)]
        [field: SerializeField] public float DangerHearingRadius { get; private set; } = 5f;

        [Tooltip("Rayon large : une cible ici est simplement entendue.")]
        [Range(1, 60)]
        [field: SerializeField] public float HearingRadius { get; private set; } = 10f;

        public event Action<Vector3 > OnHearAlerte;  
        public event Action<Vector3> OnTargetHear;  

        protected override void Scan()
        {
            detectedTargets.Clear();
           
            Transform dangerTarget = FindTargetInRadius(DangerHearingRadius);
            CharacterSetup player;
            if (dangerTarget == null)
				return;
	        OnHearAlerte?.Invoke(LastPos);
	        Remember(dangerTarget);
            Transform farTarget = FindTargetInRadius(HearingRadius);
            if (farTarget != null)
            {
                Remember(farTarget);
                OnTargetHear?.Invoke(LastPos);
            }
        }

        protected override void OnDrawGizmos()
        {
            Vector3 origin = SensorOrigin;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, HearingRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, DangerHearingRadius);

            base.OnDrawGizmos(); 
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace PnjDetection
{
    public abstract class DetectionSense : PnjAptitude
    {
        [Header("Filtrage commun")]
        [SerializeField] protected LayerMask targetMask;
        [SerializeField] protected LayerMask obstacleMask;

        [Header("Décalage du capteur")]
        [Tooltip("Décalage local du point d'origine du sens (ex: hauteur des yeux).")]
        [SerializeField] protected Vector3 sensorOffset;

        [Header("Cadence de scan")]
        [SerializeField] protected float scanInterval = 0.05f;

        [Header("Mémoire")]
        [SerializeField] protected List<Transform> detectedTargets = new List<Transform>();
        [field: SerializeField] public Vector3 LastPos { get; protected set; }

        
        public Vector3 SensorOrigin => transform.position + transform.TransformDirection(sensorOffset);

        protected virtual void OnEnable()
        {
            StartCoroutine(ScanRoutine());
        }

        private System.Collections.IEnumerator ScanRoutine()
        {
            
            yield return new WaitForSeconds(Random.Range(0f, scanInterval));

            while (true)
            {
                Scan();
                yield return new WaitForSeconds(scanInterval);
            }
        }
        
        protected abstract void Scan();
        
        protected Transform FindTargetInRadius(float radius)
        {
            Vector3 origin = SensorOrigin;
            Collider[] hits = Physics.OverlapSphere(origin, radius, targetMask);

            foreach (Collider hit in hits)
            {
                Transform target = hit.transform;
                Vector3 toTarget = target.position - origin;
                float dist = toTarget.magnitude;

                if (dist < 0.0001f)
                    continue;

                Vector3 dir = toTarget / dist; 

               
                if (!PassesFilter(dir, dist))
                    continue;
                
                if (Physics.Raycast(origin, dir, dist, obstacleMask))
                    continue;

                return target;
            }

            return null;
        }

        
        protected virtual bool PassesFilter(Vector3 dirToTarget, float distToTarget) => true;

        
        protected void Remember(Transform target)
        {
            detectedTargets.Add(target);
            LastPos = target.position;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            foreach (Transform target in detectedTargets)
            {
                if (target != null)
                    Gizmos.DrawLine(SensorOrigin, target.position);
            }
        }
    }
}
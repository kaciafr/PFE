using System;
using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    [DefaultExecutionOrder(-100)]
    public class PlayerSensor : MonoBehaviour
    {
        [SerializeField] private Transform playerObject;
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private bool drawDebug = true;

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public bool LadderInFront { get; private set; }
        
        public Rigidbody ThrowableInFront { get; private set;  }
        public Vector3 LadderNormal { get; private set; }
        public Vector3 LadderPoint { get; private set; }
        public Rigidbody CrateInFront { get; private set; }

        private void FixedUpdate()
        {
            CheckGround();
            CheckLadder();
            CheckCrate();
            CheckThrowable(); 

        }

        private void CheckGround()
        {
            var ground = settings.Ground;
            Vector3 center = transform.position + Vector3.up * ground.CheckOffset;
            float distance = ground.RaycastDistance + ground.CheckOffset;

            int hits = 0;
            Vector3 normalSum = Vector3.zero;

            // i = 0 : rayon central, puis un cercle de RayCount rayons autour
            for (int i = 0; i <= ground.RayCount; i++)
            {
                Vector3 origin = center;
                if (i > 0)
                {
                    float angle = (i - 1) * Mathf.PI * 2f / ground.RayCount;
                    origin += new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ground.RayRadius;
                }

                bool hit = Physics.Raycast(origin, Vector3.down, out RaycastHit info, distance,
                                           ground.GroundLayer, QueryTriggerInteraction.Ignore)
                           && Vector3.Angle(info.normal, Vector3.up) <= ground.MaxSlopeAngle;

                if (hit)
                {
                    hits++;
                    normalSum += info.normal;
                }

                if (drawDebug)
                    Debug.DrawRay(origin, Vector3.down * distance, hit ? Color.green : Color.red);
            }

            IsGrounded = hits > 0;
            GroundNormal = hits > 0 ? (normalSum / hits).normalized : Vector3.up;
        }

        private void CheckLadder()
        {
            var climb = settings.Climb;
            Vector3 origin = transform.position + Vector3.down * climb.RayHeight;
            Vector3 dir = playerObject.forward;

            LadderInFront = Physics.Raycast(origin, dir, out RaycastHit hit, climb.DetectDistance,
                                            climb.Layer, QueryTriggerInteraction.Ignore);
            if (LadderInFront)
            {
                LadderNormal = hit.normal;
                LadderPoint = hit.point;
            }

            if (drawDebug)
                Debug.DrawRay(origin, dir * climb.DetectDistance, LadderInFront ? Color.green : Color.red);
        }

        private void CheckCrate()
        {
            var grab = settings.Grab;
            Vector3 origin = transform.position + Vector3.up * grab.RayHeight;
            Vector3 dir = playerObject.forward;

            CrateInFront = Physics.Raycast(origin, dir, out RaycastHit hit, grab.DetectDistance,
                                           grab.Layer, QueryTriggerInteraction.Ignore)
                ? hit.rigidbody
                : null;

            if (drawDebug)
                Debug.DrawRay(origin, dir * grab.DetectDistance, CrateInFront ? Color.blue : Color.yellow);
        }


        private readonly Collider[] throwableBuffer = new Collider[8];

        private void CheckThrowable()
        {
            var throwSettings = settings.Throw;
            Vector3 origin = transform.position + Vector3.up * throwSettings.RayHeight;
            Vector3 center = origin + playerObject.forward * throwSettings.DetectRadius;

            // Un SphereCast ignore les objets déjà dans la sphère au départ : on prend le plus proche dans la zone devant
            int count = Physics.OverlapSphereNonAlloc(center, throwSettings.DetectRadius, throwableBuffer,
                                                      throwSettings.LayerObject, QueryTriggerInteraction.Ignore);

            Rigidbody closest = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Rigidbody body = throwableBuffer[i].attachedRigidbody;
                if (body == null) continue;

                float distance = (body.position - origin).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = body;
                }
            }

            ThrowableInFront = closest;

            if (drawDebug)
                Debug.DrawLine(origin, center, ThrowableInFront ? Color.magenta : Color.gray);
        }
    }
}
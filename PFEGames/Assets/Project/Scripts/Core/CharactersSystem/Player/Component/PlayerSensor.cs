using System.Numerics;
using Goblfin.CharactersSystem.Player.Data;
using Goblfin.CharactersSystem.Player.Interface;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Goblfin.CharactersSystem.Player.Component
{
    [DefaultExecutionOrder(-100)]
    public class PlayerSensor : MonoBehaviour
    {
        [SerializeField] private Transform playerObject;
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private bool drawDebugGround  = true;
        [SerializeField] private bool drawDebugLadder  = true;
        [SerializeField] private bool drawDebugCrate  = true;
        [SerializeField] private bool drawDebugThrowable = true;
        [SerializeField] private bool drawDebugGroundAhead = true;

        
        [SerializeField] private float edgeCheckDistance = 0.35f;
        [SerializeField] private float edgeMaxDrop = 0.5f;   
        
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public bool LadderInFront { get; private set; }
        public bool LadderTopReached { get; private set; }

        public Rigidbody ThrowableInFront { get; private set; }
        public Vector3 LadderNormal { get; private set; }
        public Vector3 LadderPoint { get; private set; }
        public float LadderTopY { get; private set; }
        public Rigidbody CrateInFront { get; private set; }
        public Vector3 CrateHitPoint { get; private set; }   
        public Vector3 CrateHitNormal { get; private set; }  
        public IInteractable InteractableInFront  { get; private set; }

       
        private PlayerMotor motor;
        private Vector3 Forward
        {
            get
            {
                if (motor == null) motor = GetComponent<PlayerMotor>();
                return motor != null ? motor.Forward : playerObject.forward;
            }
        }
        private void FixedUpdate()
        {
            CheckGround();
            CheckLadder();
            CheckCrate();
            CheckThrowable();
            CheckInteractable();

        }

        private void CheckGround()
        {
            var ground = settings.Ground;
            Vector3 center = transform.position + Vector3.up * ground.CheckOffset;
            float distance = ground.RaycastDistance + ground.CheckOffset;

            int hits = 0;
            Vector3 normalSum = Vector3.zero;

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

                if (drawDebugGround)
                    Debug.DrawRay(origin, Vector3.down * distance, hit ? Color.green : Color.red);
            }

            IsGrounded = hits > 0;
            GroundNormal = hits > 0 ? (normalSum / hits).normalized : Vector3.up;
        }

        private void CheckLadder()
        {
            var climb = settings.Climb;
            Vector3 origin = transform.position + Vector3.down * climb.RayHeight;
            Vector3 dir = Forward;

            LadderInFront = Physics.Raycast(origin, dir, out RaycastHit hit, climb.DetectDistance,
                                            climb.Layer, QueryTriggerInteraction.Ignore);
            if (LadderInFront)
            {
                LadderNormal = hit.normal;
                LadderTopY = hit.collider.bounds.max.y;

                Vector3 side = Vector3.Cross(Vector3.up, hit.normal).normalized;
                Vector3 toCenter = hit.collider.bounds.center - hit.point;
                LadderPoint = hit.point + side * Vector3.Dot(toCenter, side);
            }

            Vector3 topOrigin = transform.position + Vector3.up * climb.TopCheckHeight;
            LadderTopReached = LadderInFront
                && !Physics.Raycast(topOrigin, dir, climb.DetectDistance, climb.Layer, QueryTriggerInteraction.Ignore);

            if (drawDebugLadder)
            {
                Debug.DrawRay(origin, dir * climb.DetectDistance, LadderInFront ? Color.green : Color.red);
                Debug.DrawRay(topOrigin, dir * climb.DetectDistance, LadderTopReached ? Color.cyan : Color.white);
            }
        }

        private void CheckCrate()
        {
            var grab = settings.Grab;
            Vector3 origin = transform.position + Vector3.up * grab.RayHeight;
            Vector3 dir = Forward;

            CrateInFront = Physics.Raycast(origin, dir, out RaycastHit hit, grab.DetectDistance,
                                           grab.Layer, QueryTriggerInteraction.Ignore)
                ? hit.rigidbody
                : null;

            if (CrateInFront != null)
            {
                CrateHitPoint = hit.point;
                CrateHitNormal = hit.normal;
            }

            if (drawDebugCrate)
                Debug.DrawRay(origin, dir * grab.DetectDistance, CrateInFront ? Color.blue : Color.yellow);
        }


        private readonly Collider[] throwableBuffer = new Collider[8];

        private void CheckThrowable()
        {
            var throwSettings = settings.Throw;
            Vector3 origin = transform.position + Vector3.up * throwSettings.RayHeight;
            Vector3 center = origin + Forward * throwSettings.DetectRadius;

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

            if (drawDebugThrowable)
                Debug.DrawLine(origin, center, ThrowableInFront ? Color.magenta : Color.gray);
        }
        
        private void CheckInteractable()
        {
            Vector3 origin = transform.position + Vector3.up * settings.Interact.InteractHeight;
            Vector3 center = origin + Forward * settings.Interact.InteractRadius;

            Collider[] hits = Physics.OverlapSphere(center, settings.Interact.InteractRadius);
            InteractableInFront = null;
            foreach (var h in hits)
            {
                var i = h.GetComponentInParent<IInteractable>();
                if (i != null)
                {
                    InteractableInFront = i;
                    break;
                }
            }
        }


        public bool HasGroundAhead(Vector3 dir)
        {
            var ground = settings.Ground;
            // Point un peu devant, dans la direction où le joueur VEUT aller
            Vector3 origin = transform.position + dir.normalized * edgeCheckDistance + Vector3.up * ground.CheckOffset;
            float distance = ground.RaycastDistance + ground.CheckOffset + edgeMaxDrop;
            
            bool hit =  Physics.Raycast(origin, Vector3.down, distance, ground.GroundLayer, QueryTriggerInteraction.Ignore);

            if (drawDebugGroundAhead)
                Debug.DrawRay(origin, Vector3.down * distance, hit ? Color.green : Color.magenta);
            
            return hit;
        }
                
        private void OnDrawGizmos()
        {
            if (playerObject == null) return;

            Vector3 origin = transform.position + Vector3.up * settings.Interact.InteractHeight;
            Vector3 center = origin + Forward * settings.Interact.InteractRadius;

            Gizmos.color = InteractableInFront != null ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(center, settings.Interact.InteractRadius);
        }
    }
}
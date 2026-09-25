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
        public bool LadderInFront { get; private set; }
        public Vector3 LadderNormal { get; private set; }
        public Vector3 LadderPoint { get; private set; }
        public Rigidbody CrateInFront { get; private set; }

        private void FixedUpdate()
        {
            CheckGround();
            CheckLadder();
            CheckCrate();
        }

        private void CheckGround()
        {
            var ground = settings.Ground;
            Vector3 origin = transform.position + Vector3.up * ground.CheckOffset;

            IsGrounded = Physics.Raycast(origin, Vector3.down, ground.RaycastDistance + ground.CheckOffset,
                                         ground.GroundLayer, QueryTriggerInteraction.Ignore);

            if (drawDebug)
                Debug.DrawRay(origin, Vector3.down * (ground.RaycastDistance + ground.CheckOffset),
                              IsGrounded ? Color.green : Color.red);
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
    }
}
using System.Collections;
using UnityEngine;

namespace Characters.Component
{
    [RequireComponent(typeof(LineRenderer))]
    public class ThrowObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform releasePoint;

        [Header("Throw")]
        [SerializeField, Min(0f)] private float throwSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float upwardBias = 0.3f;
        [SerializeField, Min(0f), Tooltip("Temps pendant lequel l'objet lâché ne touche pas le joueur (évite qu'il le pousse en l'air).")]
        private float ignorePlayerTime = 0.3f;

        [Header("Trajectory")]
        [SerializeField, Range(10, 100)] private int linePoints = 25;
        [SerializeField, Range(0.01f, 0.25f)] private float timeBetweenPoints = 0.1f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private LineRenderer lineRenderer;
        private Rigidbody heldObject;
        private Collider[] heldColliders;
        private Collider[] playerColliders;
        private Transform initialParent;
        private bool isAiming;

        public bool IsHolding => heldObject != null;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            playerColliders = GetComponentsInChildren<Collider>();
        }

        private void LateUpdate()
        {
            if (isAiming && IsHolding)
                DrawTrajectory();
        }

        public void Hold(Rigidbody obj)
        {
            if (obj == null || IsHolding) return;

            heldObject = obj;
            heldColliders = obj.GetComponentsInChildren<Collider>();
            initialParent = obj.transform.parent;

            heldObject.isKinematic = true;
            SetHeldCollidersEnabled(false);

            heldObject.transform.SetParent(releasePoint);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
        }

        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
            lineRenderer.enabled = aiming && IsHolding;
        }

        public void Release()
        {
            if (!IsHolding) return;

            Vector3 velocity = GetThrowVelocity();

            Detach();
            heldObject.linearVelocity = velocity;

            ClearHeld();
        }

        public void Drop()
        {
            if (!IsHolding) return;

            Detach();
            ClearHeld();
        }

        private void Detach()
        {
            heldObject.transform.SetParent(initialParent);
            heldObject.isKinematic = false;

            SetPlayerCollisionIgnored(heldColliders, true);
            SetHeldCollidersEnabled(true);

            if (isActiveAndEnabled)
                StartCoroutine(RestorePlayerCollision(heldColliders));
            else
                SetPlayerCollisionIgnored(heldColliders, false);
        }

        private void ClearHeld()
        {
            heldObject = null;
            heldColliders = null;
            SetAiming(false);
        }

        private void SetHeldCollidersEnabled(bool enabled)
        {
            foreach (Collider col in heldColliders)
                if (col != null) col.enabled = enabled;
        }

        private void SetPlayerCollisionIgnored(Collider[] objectColliders, bool ignore)
        {
            foreach (Collider objectCol in objectColliders)
            foreach (Collider playerCol in playerColliders)
                if (objectCol != null && playerCol != null)
                    Physics.IgnoreCollision(objectCol, playerCol, ignore);
        }

        private IEnumerator RestorePlayerCollision(Collider[] objectColliders)
        {
            yield return new WaitForSeconds(ignorePlayerTime);
            SetPlayerCollisionIgnored(objectColliders, false);
        }

        private Vector3 GetThrowVelocity()
        {
            Vector3 direction = (releasePoint.forward + Vector3.up * upwardBias).normalized;
            return direction * throwSpeed;
        }

        private void DrawTrajectory()
        {
            Vector3 start = releasePoint.position;
            Vector3 velocity = GetThrowVelocity();
            Vector3 gravity = Physics.gravity;

            lineRenderer.positionCount = linePoints;
            lineRenderer.SetPosition(0, start);

            Vector3 previous = start;

            for (int i = 1; i < linePoints; i++)
            {
                float t = i * timeBetweenPoints;
                Vector3 point = start + velocity * t + 0.5f * gravity * t * t;

                Vector3 segment = point - previous;
                if (Physics.Raycast(previous, segment.normalized, out RaycastHit hit, segment.magnitude,
                                    collisionMask, QueryTriggerInteraction.Ignore))
                {
                    lineRenderer.SetPosition(i, hit.point);
                    lineRenderer.positionCount = i + 1;
                    return;
                }

                lineRenderer.SetPosition(i, point);
                previous = point;
            }
        }

        private void OnDisable() => Drop();
    }
}
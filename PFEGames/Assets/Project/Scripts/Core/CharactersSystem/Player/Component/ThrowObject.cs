using System.Collections;
using Goblfin.Player.Component;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    [RequireComponent(typeof(LineRenderer))]
    public class ThrowObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform releasePoint;

        [Header("Throw")]
        [SerializeField, Min(0f), Tooltip("Vitesse de départ quand on ramasse un objet.")]
        private float throwSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float upwardBias = 0.3f;

        [Header("Puissance (charge)")]
        [SerializeField, Min(0f)] private float minThrowSpeed = 5f;
        [SerializeField, Min(0f)] private float maxThrowSpeed = 15f;
        [SerializeField, Min(0.1f)] private float chargeTime = 1.5f;
        [SerializeField, Min(0f), Tooltip("Temps pendant lequel l'objet lâché ne touche pas le joueur (évite qu'il le pousse en l'air).")]
        private float ignorePlayerTime = 0.3f;


        private float  currentMaxSpeed; 
        [Header("Trajectory")]
        [SerializeField, Range(10, 100)] private int linePoints = 25;
        [SerializeField, Range(0.01f, 0.25f)] private float timeBetweenPoints = 0.1f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float scrollSpeed = 2f;

        [Header("Cercle d'atterrissage")]
        [SerializeField, Min(0f)] private float markerSize = 0.4f;
        [SerializeField, Range(0f, 0.5f)] private float markerPulse = 0.1f;

        private LineRenderer lineRenderer;
        private Material lineMaterial;
        private Transform landingMarker;
        private PlayerMotor motor;
        private Rigidbody heldObject;
        private Collider[] heldColliders;
        private Collider[] playerColliders;
        private Transform initialParent;
        private Coroutine restoreRoutine;
        private Collider[] restoreColliders;
        private bool isAiming;
        private float currentSpeed;

        public bool IsHolding => heldObject != null;
        public bool IsHoldingCorpse { get; private set; }
        public float CurrentSpeed => currentSpeed;
        public float PowerPercent => Mathf.InverseLerp(minThrowSpeed, currentMaxSpeed, currentSpeed);

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineMaterial = lineRenderer.material;
            motor = GetComponent<PlayerMotor>();
            playerColliders = GetComponentsInChildren<Collider>();
            CreateLandingMarker();
        }

        private void LateUpdate()
        {
            if (isAiming && IsHolding)
            {
                DrawTrajectory();
                lineMaterial.mainTextureOffset = new Vector2(-Time.time * scrollSpeed, 0f);
            }
        }

        private void CreateLandingMarker()
        {
            landingMarker = new GameObject("LandingMarker").transform;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(visual.GetComponent<Collider>());
            visual.name = "Visual";
            visual.transform.SetParent(landingMarker, false);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            MeshRenderer markerRenderer = visual.GetComponent<MeshRenderer>();
            markerRenderer.sharedMaterial = lineRenderer.sharedMaterial;
            markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            markerRenderer.receiveShadows = false;

            landingMarker.gameObject.SetActive(false);
        }

        private void ShowMarker(bool show, Vector3 point, Vector3 normal)
        {
            landingMarker.gameObject.SetActive(show);
            if (!show) return;

            landingMarker.position = point + normal * 0.02f;
            landingMarker.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            landingMarker.localScale = Vector3.one * markerSize * (1f + Mathf.Sin(Time.time * 6f) * markerPulse);
        }

        public void Hold(Rigidbody obj)
        {
            if (obj == null || IsHolding) return;

            heldObject = obj;
            heldColliders = obj.GetComponentsInChildren<Collider>();
            
            PlayerCorpse corpse = obj.GetComponentInParent<PlayerCorpse>();
            IsHoldingCorpse = corpse != null;
            currentMaxSpeed = corpse != null ? corpse.MaxThrowSpeed : maxThrowSpeed;
            currentSpeed = minThrowSpeed;
            
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
            if (!lineRenderer.enabled) ShowMarker(false, Vector3.zero, Vector3.up);
        }

        public void ChargePower(float deltaTime)
        {
            float speedPerSecond =  (currentMaxSpeed - minThrowSpeed) / chargeTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed,currentMaxSpeed, speedPerSecond * deltaTime);
        }

        public void ResetPower()
        {
            currentSpeed = minThrowSpeed;
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

            FinishPendingRestore();

            SetPlayerCollisionIgnored(heldColliders, true);
            SetHeldCollidersEnabled(true);

            if (isActiveAndEnabled)
            {
                restoreColliders = heldColliders;
                restoreRoutine = StartCoroutine(RestorePlayerCollision());
            }
            else
                SetPlayerCollisionIgnored(heldColliders, false);
        }

        private void FinishPendingRestore()
        {
            if (restoreRoutine == null) return;

            StopCoroutine(restoreRoutine);
            SetPlayerCollisionIgnored(restoreColliders, false);
            restoreRoutine = null;
            restoreColliders = null;
        }

        private void ClearHeld()
        {
            heldObject = null;
            IsHoldingCorpse = false;
            heldColliders = null;
            SetAiming(false);
        }

        private void SetHeldCollidersEnabled(bool enabled)
        {
            foreach (Collider col in heldColliders)
                if (col != null && !col.isTrigger) col.enabled = enabled;
        }

        private void SetPlayerCollisionIgnored(Collider[] objectColliders, bool ignore)
        {
            foreach (Collider objectCol in objectColliders)
            foreach (Collider playerCol in playerColliders)
                if (objectCol != null && playerCol != null)
                    Physics.IgnoreCollision(objectCol, playerCol, ignore);
        }

        private IEnumerator RestorePlayerCollision()
        {
            yield return new WaitForSeconds(ignorePlayerTime);
            SetPlayerCollisionIgnored(restoreColliders, false);
            restoreRoutine = null;
            restoreColliders = null;
        }

        private Vector3 GetThrowVelocity()
        {
            Vector3 forward = motor != null ? motor.Forward : transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 direction = (forward + Vector3.up * upwardBias).normalized;
            return direction * currentSpeed;
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
                    ShowMarker(true, hit.point, hit.normal);
                    return;
                }

                lineRenderer.SetPosition(i, point);
                previous = point;
            }

            ShowMarker(false, Vector3.zero, Vector3.up);
        }

        private void OnDisable()
        {
            Drop();
            FinishPendingRestore();
        }
    }
}
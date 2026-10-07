using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform playerObject;
        [SerializeField, Tooltip("Modèle visuel (enfant du Player) : reçoit l'inclinaison et le décalage pendant le climb.")]
        private Transform model;
        [SerializeField] private PlayerSettings settings;

        public Vector3 Forward => playerObject.forward;
        private Rigidbody rb;
        private Vector3 modelStartLocalPos;
        private Quaternion modelStartLocalRot;

        public Vector3 Velocity => rb.linearVelocity;
        private Vector3 lastPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            if (model != null)
            {
                modelStartLocalPos = model.localPosition;
                modelStartLocalRot = model.localRotation;
            }
            lastPos = rb.position;
        }

        private void Start()
        {
	        if (cam == null)
	        {
		        cam = FindObjectOfType<Camera>().transform;
	        }
        }

        private void FixedUpdate()
        {
            if ((rb.position - lastPos).sqrMagnitude > 9f)
            {
                Debug.LogError($"TP détecté : {lastPos} -> {rb.position}\n{System.Environment.StackTrace}");
                Debug.Break();
            }
            lastPos = rb.position;
        }

        public Vector3 GetMoveDirection(Vector2 input)
        {
            Vector3 camForward = cam.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.001f)
                orientation.forward = camForward.normalized;

            Vector3 dir = orientation.forward * input.y + orientation.right * input.x;
            return Vector3.ClampMagnitude(dir, 1f);
        }

        public void Move(Vector2 input, float speed, bool rotate = true)
        {
            Vector3 dir = GetMoveDirection(input);

            if (rotate && dir.sqrMagnitude > 0.001f)
                RotateTowards(dir);

            Vector3 velocity = dir * speed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }

        public void MoveWorld(Vector3 dir, float speed, bool rotate = true)
        {
            if (rotate && dir.sqrMagnitude > 0.001f)
                RotateTowards(dir);

            Vector3 velocity = dir * speed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }

        public void RotateTowards(Vector3 dir)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, dir.normalized,
                Time.deltaTime * settings.Movement.RotationSpeed);
        }

        public void Jump(float force)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = force;
            rb.linearVelocity = velocity;
        }

        public void Stop()
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        public void StartClimb(Vector3 ladderPoint, Vector3 ladderNormal)
        {
            ladderNormal.y = 0f;
            if (ladderNormal.sqrMagnitude < 0.001f || ladderPoint == Vector3.zero)
            {
                Debug.LogWarning($"StartClimb ignoré : ladderPoint={ladderPoint} normal={ladderNormal}");
                return;
            }
            ladderNormal.Normalize();

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            float tilt = Mathf.Clamp(settings.Climb.climbTilt, -45f, 45f);
            float offset = Mathf.Clamp(settings.Climb.climbVisualOffset, 0f, 0.5f);

            playerObject.rotation = Quaternion.LookRotation(-ladderNormal, Vector3.up);

            if (model != null)
            {
                model.localRotation = modelStartLocalRot * Quaternion.Euler(tilt, 0f, 0f);
                Vector3 localDir = model.parent.InverseTransformDirection(-ladderNormal);
                model.localPosition = modelStartLocalPos + localDir * offset;
            }

            Vector3 snap = ladderPoint + ladderNormal * settings.Climb.LadderDistance;
            snap.y = rb.position.y;
            rb.position = snap;
            
            Debug.Log($"ladderPoint={ladderPoint} normal={ladderNormal} snap={snap} dist={settings.Climb.LadderDistance} rbPos={rb.position}");
        }

        public void SnapFacing(Vector3 point, Vector3 normal, float distance)
        {
            normal.y = 0f;
            if (normal.sqrMagnitude < 0.001f) return;
            normal.Normalize();

            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            playerObject.rotation = Quaternion.LookRotation(-normal, Vector3.up);

            Vector3 snap = point + normal * distance;
            snap.y = rb.position.y;
            rb.position = snap;

            // Décalage sur le modèle uniquement : playerObject est la racine, la bouger téléporte le joueur
            if (model != null)
            {
                float offset = Mathf.Clamp(settings.Grab.GrabVisualOffset, 0f, 0.4f);
                Vector3 localDir = model.parent.InverseTransformDirection(-normal);
                model.localPosition = modelStartLocalPos + localDir * offset;
            }
        }

        public void ResetVisualOffset()
        {
            if (model != null)
                model.localPosition = modelStartLocalPos;
        }

        public void Climb(float verticalSpeed)
        {
            rb.linearVelocity = Vector3.up * verticalSpeed;
        }

        public void StopClimb()
        {
            rb.useGravity = true;

            if (model != null)
            {
                model.localRotation = modelStartLocalRot;
                model.localPosition = modelStartLocalPos;
            }
        }

        public void Freeze()
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        public void ClimbOverTop(Vector3 ladderNormal, float ladderTopY)
        {
            ladderNormal.y = 0f;
            ladderNormal.Normalize();

            Vector3 target = rb.position + Vector3.up * settings.Climb.TopHeight
                                         - ladderNormal * settings.Climb.TopForward;

            // Toujours posé au-dessus du haut de l'échelle : sinon le joueur retombe devant et relance un climb
            float standY = ladderTopY + GetComponent<CapsuleCollider>().height * 0.5f + 0.05f;
            target.y = Mathf.Max(target.y, standY);

            rb.position = target;
        }

        public void Unfreeze()
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        
    }
}
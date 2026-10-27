using System.Reflection.Metadata.Ecma335;
using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform playerObject;
        private Transform model;
        [SerializeField] private PlayerSettings settings;

        public Vector3 Forward => playerObject.forward;
        public Transform Model => model;
        private Rigidbody rb;
        private Vector3 modelStartLocalPos;
        private Quaternion modelStartLocalRot;
        private PlayerSensor playerSensor;
        private float lastjump = -1; 

        public Vector3 Velocity => rb.linearVelocity;

        private bool OnSlope => playerSensor.IsGrounded
                                && Vector3.Angle(playerSensor.GroundNormal, Vector3.up) > 1f
                                && Time.time -  lastjump >0.2f ; 
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            playerSensor = GetComponent<PlayerSensor>(); 
            rb.freezeRotation = true;
            if (model == null || model == transform)
            {
                Animator animator = GetComponentInChildren<Animator>();
                model = animator != null && animator.transform != transform ? animator.transform : null;
            }
            if (model != null)
            {
                modelStartLocalPos = model.localPosition;
                modelStartLocalRot = model.localRotation;
            }
        }

        private void Start()
        {
	        if (cam == null)
	        {
		        cam = FindObjectOfType<Camera>().transform;
	        }
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

        public void Move(Vector2 input, float speed, bool rotate = true, float rateair = 1f ,bool followSlope = true)
        {
            Vector3 dir = GetMoveDirection(input);

            if (rotate && dir.sqrMagnitude > 0.001f)
                RotateTowards(dir);

            Vector3 current = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 target = dir * speed;
            float rate = target.sqrMagnitude < current.sqrMagnitude
                ? settings.Movement.Deceleration
                : settings.Movement.Acceleration;
            
            if (OnSlope && followSlope)
            {
                target = Vector3.ProjectOnPlane(target, playerSensor.GroundNormal);
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, target, rate * rateair * Time.fixedDeltaTime);
                return; 
            }
            
            Vector3 velocity = Vector3.MoveTowards(current, target, rate * rateair *Time.fixedDeltaTime);
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

           
        }

        public void SlowDown()
        {

            if (OnSlope)
            {
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, settings.Movement.Deceleration * Time.fixedDeltaTime);
                rb.AddForce(-Physics.gravity, ForceMode.Acceleration);  
                return;
            }

            Vector3 current = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 velocity = Vector3.MoveTowards(current, Vector3.zero, settings.Movement.Deceleration * Time.fixedDeltaTime);
            
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }

        public void ApplyExtraGravity(bool jumpHeld)
        {
            if (rb.isKinematic || !rb.useGravity) return;

            float multiplier = 1f;
            if (rb.linearVelocity.y < 0f)
                multiplier = settings.Jump.FallMultiplier;
            else if (rb.linearVelocity.y > 0f && !jumpHeld)
                multiplier = settings.Jump.LowJumpMultiplier;

            rb.linearVelocity += Physics.gravity * (multiplier - 1f) * Time.fixedDeltaTime;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, -settings.Jump.MaxFallSpeed);
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

        public void Face(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                playerObject.forward = dir.normalized;
        }

        public void Jump(float force)
        {
            lastjump = Time.time;
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
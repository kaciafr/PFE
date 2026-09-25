using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Characters
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField] private Transform cam;          
        [SerializeField] private Transform orientation;  
        [SerializeField] private Transform playerObj;   
        [SerializeField] private CapsuleCollider playerCollider;
        private Rigidbody rb;                           
        
        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 6f;

        [Header("Crounch  ")]
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float crouchHeight = 1.5f;
        [SerializeField] private float crouchTransitionSpeed = 8f;
        [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.595f);
        [SerializeField] private float standHeight;
        [SerializeField] private Vector3 standCenter;
        [SerializeField] private bool isCrouching;

        [Header("Sprint")]
        [SerializeField] private bool isSprinting;

        [Header("Climb")]
        [SerializeField] private bool isClimbing;
        [SerializeField] private bool ladderInFront;
        [SerializeField] private LayerMask climbLayer;
        [SerializeField] private float rayheight = 0f;
        [SerializeField] private float climboffsetDistance = 1f;
        [SerializeField] private float climbExitPush = 2f;
        private Vector3 ladderNormal;
        [SerializeField] private float climbTopDuration = 2f;
        [SerializeField] private float climbTopHeight = 1.5f;
        [SerializeField] private float climbTopForward = 0.6f;
        private bool isClimbingTop;
        public bool IsClimbingTop => isClimbingTop;
        [SerializeField] private float ladderDistance = 0.4f;
        private Vector3 ladderPoint;
        
        [Header("Push and Object")]
        
        [SerializeField]  private LayerMask grabLayer;
        [SerializeField] private float GrabOffsetDistance = 1.2f;
        [SerializeField] private float grabRayHeight = -0.5f;
        [SerializeField] private float pushPullSpeed = 1.5f;
        private Rigidbody crateInFront;  
        private Rigidbody grabbedCrate;  
        public bool IsGrabbing => grabbedCrate != null;
        public bool CanGrab => isGrounded && crateInFront != null;
        
        
        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float raycastDistance = 1.1f;
        [SerializeField] private float groundCheckOffset = 0.1f;

        public bool CanStand => CanStandUp();

        [Header("State")]
        public bool isGrounded;

        public float VerticalVelocity => rb.linearVelocity.y;
        public bool IsClimbing => isClimbing;
        
        #endregion

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            standCenter = playerCollider.center;
            standHeight = playerCollider.height;

            crouchCenter = standCenter - Vector3.up * (standHeight - crouchHeight) * 0.5f;
        }

        private void Update()
        {
            CheckGrabObject();
            CheckGrounded();
            CheckClimbing();
            UpdateCollider();
        }

        private void FixedUpdate()
        {
                if (grabbedCrate == null) return;

                Vector3 v = rb.linearVelocity;
                grabbedCrate.linearVelocity = new Vector3(v.x, grabbedCrate.linearVelocity.y, v.z);

                if (Vector3.Distance(rb.position, grabbedCrate.position) > 3f) UnGrab();
            
        }

        #region Movement 
        public void Move(Vector2 input)
        {
            if (isClimbingTop) return;

            if (isClimbing)
            {
                rb.linearVelocity = Vector3.up * (input.x * climbSpeed);

                if (input.x < 0f && isGrounded)
                {
                    UnClimb();
                    return;
                }

                if (!ladderInFront)
                {
                    StartCoroutine(ClimbTopRoutine());
                }
                return;
            }

            Vector3 camForward = cam.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.001f)
                orientation.forward = camForward.normalized;

            Vector3 moveDir = orientation.forward * input.y + orientation.right * input.x;
            moveDir = Vector3.ClampMagnitude(moveDir, 1f);

            if (!IsGrabbing && ladderInFront && Vector3.Dot(moveDir, -ladderNormal) > 0.5f)  
            {
                StartClimb();
                return;
            }

            if (moveDir.sqrMagnitude > 0.001f && !IsGrabbing)  
                playerObj.forward = Vector3.Slerp(playerObj.forward, moveDir.normalized, Time.deltaTime * rotationSpeed);

            float currentSpeed = walkSpeed;
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }

            if (isSprinting)
            {
                currentSpeed = sprintSpeed;
            }

            if (IsGrabbing)                   
                currentSpeed = pushPullSpeed;

            Vector3 velocity = moveDir * currentSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }
        
        #endregion

     
        #region State Movement
        public void Jump()
        {
            if (!isGrounded) return;
            if (IsGrabbing) return;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
        }

        public void Crouch() => isCrouching = true;

        public void UnCrouch()
        {
            if (CanStand)
            {
                isCrouching = false;
            }
        }


        public void Sprint() => isSprinting = true;

        public void UnSprint() => isSprinting = false;


        public void Grab()
        {
            if (!isGrounded || crateInFront == null) return;

            grabbedCrate = crateInFront;
            grabbedCrate.isKinematic = false;   
        }

        public void UnGrab()
        {
            if (grabbedCrate == null) return;

            grabbedCrate.linearVelocity = Vector3.zero;
            grabbedCrate.isKinematic = true;   
            grabbedCrate = null;
        }

        
        #endregion
        
        #region Climbing

        public void UnClimb()
        {
            isClimbing = false;
            rb.useGravity = true;
        }

        public void StartClimb()
        {
            isClimbing = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            playerObj.forward = -ladderNormal;
            
            Vector3 snap = ladderPoint + ladderNormal * ladderDistance;
            snap.y = rb.position.y;
            rb.position = snap;
        }

        private IEnumerator ClimbTopRoutine()
        {
            isClimbing = false;
            isClimbingTop = true;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            yield return new WaitForSeconds(climbTopDuration);

            rb.position = rb.position + Vector3.up * climbTopHeight - ladderNormal * climbTopForward;
            rb.isKinematic = false;
            rb.useGravity = true;
            isClimbingTop = false;
        }
        #endregion
       
        #region Check 
        public void CheckGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
            isGrounded = Physics.Raycast(origin, Vector3.down, raycastDistance + groundCheckOffset, groundLayer);
        }
        
        private void CheckClimbing()
        {
            Vector3 originClimb = transform.position + Vector3.down * rayheight;
            Vector3 dir = playerObj.forward;

            ladderInFront = Physics.Raycast(originClimb, dir, out RaycastHit hit, climboffsetDistance, climbLayer);
            if (ladderInFront)
            {
                ladderNormal = hit.normal;
                ladderPoint = hit.point;
            }

            Debug.DrawRay(originClimb, dir * climboffsetDistance, ladderInFront ? Color.green : Color.red);
        }


        private void CheckGrabObject()
        {
            Vector3 origin = transform.position + Vector3.up * grabRayHeight;
            crateInFront = null;
            if (Physics.Raycast(origin, playerObj.forward, out RaycastHit hit, GrabOffsetDistance, grabLayer))
                crateInFront = hit.rigidbody;
            Debug.DrawRay(origin, playerObj.forward * GrabOffsetDistance, crateInFront ? Color.blue : Color.darkOrange);
        }
        
        #endregion

        private void UpdateCollider()
        {
            Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;
            float targetHeight = isCrouching ? crouchHeight : standHeight;

            float t = Time.deltaTime * crouchTransitionSpeed;
            playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, t);
            playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, t);
        }

        private bool CanStandUp()
        {
            float r = playerCollider.radius * 0.5f;
            Vector3 feet = transform.TransformPoint(standCenter) - Vector3.up * (standHeight * 0.5f);
            Vector3 origin = feet + Vector3.up * r;
            float distance = standHeight - r;

            return !Physics.SphereCast(origin, r, Vector3.up, out _, distance,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }
        #region Stop
        public void Stop()
        {
            if (isClimbingTop) return;

            if (isClimbing)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
        public void StopStateMovement()
        {
            StopAllCoroutines();
            isCrouching = false;
            isClimbing = false; 
            isClimbing = false; 
            isSprinting = false;
            rb.useGravity = true;
        }
        
        #endregion 
        
        

       
    }
}
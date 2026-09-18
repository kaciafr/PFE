using UnityEngine;

namespace Characters
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cam;          // Main Camera
        [SerializeField] private Transform orientation;  // Orientation
        [SerializeField] private Transform playerObj;    // MSH_Capsule
        private Rigidbody rb;                            // plus de static

        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 6f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float raycastDistance = 1.1f;   
        [SerializeField] private float groundCheckOffset = 0.1f; 

        [Header("State")]
        public bool isGrounded;

        public float VerticalVelocity => rb.linearVelocity.y;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            CheckGrounded();   
        }

        public void Move(Vector2 input)
        {
            Vector3 camForward = cam.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.001f)
                orientation.forward = camForward.normalized;

            Vector3 moveDir = orientation.forward * input.y + orientation.right * input.x;
            moveDir = Vector3.ClampMagnitude(moveDir, 1f);

            if (moveDir.sqrMagnitude > 0.001f)
                playerObj.forward = Vector3.Slerp(playerObj.forward, moveDir.normalized, Time.deltaTime * rotationSpeed);

            Vector3 velocity = moveDir * walkSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }

        public void Stop()
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        public void Jump()
        {
            Debug.Log($"Jump() | grounded = {isGrounded} | mass = {rb.mass} | kinematic = {rb.isKinematic} | constraints = {rb.constraints}");

            if (!isGrounded) return;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;         
            rb.linearVelocity = velocity;
        }

        public void Crouch() { }
        public void Vault() { }
        public void Sprint() { }

        public void CheckGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
            isGrounded = Physics.Raycast(origin, Vector3.down, raycastDistance + groundCheckOffset, groundLayer);
        }
        
    }
}
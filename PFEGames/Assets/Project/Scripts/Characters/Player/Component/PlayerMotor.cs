using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform playerObject;
        [SerializeField] private PlayerSettings settings;

        private Rigidbody rb;

        public Vector3 Velocity => rb.linearVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
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
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            playerObject.forward = -ladderNormal;

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
        }

        public void Freeze()
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        public void ClimbOverTop(Vector3 ladderNormal)
        {
            rb.position += Vector3.up * settings.Climb.TopHeight
                         - ladderNormal * settings.Climb.TopForward;
        }

        public void Unfreeze()
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
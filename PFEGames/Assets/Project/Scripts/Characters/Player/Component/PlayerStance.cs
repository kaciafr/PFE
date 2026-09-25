using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    public class PlayerStance : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider playerCollider;

        private PlayerSettings settings;

        private float standHeight;
        private Vector3 standCenter;
        private Vector3 crouchCenter;

        public bool IsCrouched { get; private set; }

        public bool CanStand
        {
            get
            {
                float radius = playerCollider.radius * 0.5f;
                Vector3 feet = transform.TransformPoint(standCenter) - Vector3.up * (standHeight * 0.5f);
                Vector3 origin = feet + Vector3.up * radius;
                float distance = standHeight - radius;

                return !Physics.SphereCast(origin, radius, Vector3.up, out _, distance,
                    settings.Crouch.CeilingLayer, QueryTriggerInteraction.Ignore);
            }
        }

        private void Awake()
        {
            settings = GetComponent<PlayerManager>().Settings;

            standHeight = playerCollider.height;
            standCenter = playerCollider.center;
            crouchCenter = standCenter - Vector3.up * (standHeight - settings.Crouch.Height) * 0.5f;
        }

        public void SetCrouched(bool crouched) => IsCrouched = crouched;

        private void FixedUpdate()
        {
            Vector3 targetCenter = IsCrouched ? crouchCenter : standCenter;
            float targetHeight = IsCrouched ? settings.Crouch.Height : standHeight;

            float t = Time.fixedDeltaTime * settings.Crouch.TransitionSpeed;
            playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, t);
            playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, t);
        }
    }
}
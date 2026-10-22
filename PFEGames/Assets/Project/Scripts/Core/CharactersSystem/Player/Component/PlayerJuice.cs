using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class PlayerJuice : MonoBehaviour
    {
        [SerializeField, Tooltip("Modèle visuel (enfant avec l'Animator), pas le Player lui-même.")]
        private Transform model;
        [SerializeField, Min(0.01f)] private float squashDuration = 0.25f;
        [SerializeField] private Vector3 jumpStretch = new Vector3(0.75f, 1.3f, 0.75f);
        [SerializeField, Range(0f, 0.6f)] private float landSquash = 0.4f;
        [SerializeField, Min(0.1f), Tooltip("Vitesse de chute (m/s) qui donne l'effet maximum à l'atterrissage.")]
        private float maxLandSpeed = 6f;

        private Vector3 baseScale;
        private Vector3 squashFactor = Vector3.one;
        private float squashTimer;

        private void Start()
        {
            if (model == null && TryGetComponent(out PlayerMotor motor))
                model = motor.Model;

            if (model == null)
            {
                Animator animator = GetComponentInChildren<Animator>();
                if (animator != null && animator.transform != transform)
                    model = animator.transform;
            }

            if (model != null)
                baseScale = model.localScale;
        }

        public void OnJump()
        {
            Squash(jumpStretch);
        }

        public void OnLand(float fallSpeed)
        {
            float strength = Mathf.Clamp01(fallSpeed / maxLandSpeed);
            float squash = landSquash * strength;
            Squash(new Vector3(1f + squash, 1f - squash, 1f + squash));
        }

        private void Squash(Vector3 factor)
        {
            squashFactor = factor;
            squashTimer = squashDuration;
        }

        private void LateUpdate()
        {
            if (model == null) return;

            if (squashTimer > 0f)
            {
                squashTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(squashTimer / squashDuration);
                float eased = t * t * t;
                model.localScale = Vector3.Scale(baseScale, Vector3.Lerp(squashFactor, Vector3.one, eased));
            }
            else
            {
                model.localScale = baseScale;
            }
        }

        private void OnDisable()
        {
            if (model != null) model.localScale = baseScale;
        }
    }
}

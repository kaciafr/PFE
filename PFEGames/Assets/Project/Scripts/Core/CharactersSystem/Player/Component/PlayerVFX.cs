using Goblfin.CharactersSystem.Player;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class PlayerVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem dusttrail;
        [SerializeField] private float minSpeed = 0.5f;
        [SerializeField] private float walkMultiplier = 0.3f;
        [SerializeField] private float sprintMultiplier = 2f;
        [SerializeField] private float walkSize = 0.7f;
        [SerializeField] private float sprintSize = 1.2f;

        [SerializeField] private int jumpPuff = 6;
        [SerializeField] private int maxLandPuff = 20;
        [SerializeField, Min(0.1f)] private float maxLandSpeed = 10f;

        [SerializeField] private PlayerManager _playerManager;

        private ParticleSystem.MinMaxCurve baseRate;
        private ParticleSystem.MinMaxCurve baseSize;

        private void Awake()
        {
            if (_playerManager == null)
                _playerManager = GetComponent<PlayerManager>();

            if (dusttrail != null)
            {
                baseRate = dusttrail.emission.rateOverDistance;
                baseSize = dusttrail.main.startSize;
            }
        }

        void Update()
        {
            if (dusttrail == null) return;

            float speed = _playerManager.HorizontalSpeed;
            bool grounded = _playerManager.IsGrounded;
            bool crouched = _playerManager.Stance.IsCrouched;

            var emission = dusttrail.emission;

            emission.enabled = grounded && !crouched && speed > minSpeed;

            float t = Mathf.InverseLerp(_playerManager.Settings.Movement.WalkSpeed, _playerManager.Settings.Movement.SprintSpeed, speed);

            float rate = Mathf.Lerp(walkMultiplier, sprintMultiplier, t);
            emission.rateOverDistance = new ParticleSystem.MinMaxCurve(baseRate.constantMin * rate, baseRate.constantMax * rate);

            float size = Mathf.Lerp(walkSize, sprintSize, t);
            var main = dusttrail.main;
            main.startSize = new ParticleSystem.MinMaxCurve(baseSize.constantMin * size, baseSize.constantMax * size);
        }

        public void OnJump()
        {
            dusttrail.Emit(jumpPuff);
        }

        public void OnLand(float fallSpeed)
        {
            float strenght = Mathf.Clamp01(fallSpeed / maxLandSpeed);

            int count = Mathf.RoundToInt(maxLandPuff * strenght);
            dusttrail.Emit(count);
        }
    }
}

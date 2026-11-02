using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class NoiseRadius : MonoBehaviour
	{
		[SerializeField] private SphereCollider noiseCollider;
		[SerializeField] private GameObject noiseEffect;
		
		private IDetected target;

		[Range(0f, 10f)]
		[field: SerializeField]
		public float NoiseEffect { get; private set; } = 2f; 

        [Header("Réglages")]
        [SerializeField] private float minSpeed = 0.1f;                         
        [SerializeField, Range(0f, 10f)] private float radiusPerSpeed = 1f;    
        
        public bool IsMakingNoise { get; private set; }

        private void Awake()
        {
            target = GetComponentInParent<IDetected>();
            SetNoisy(false);
        }

        private void Update()
        {
            if (target == null) return;

            bool noisy = target.speed > minSpeed;

            if (noisy != IsMakingNoise)
                SetNoisy(noisy);

            if (noisy)
                noiseCollider.radius = target.speed * radiusPerSpeed;
        }

        private void SetNoisy(bool noisy)
        {
            IsMakingNoise = noisy;
            noiseCollider.enabled = noisy;     
            noiseEffect.SetActive(noisy);
        }
    }
}
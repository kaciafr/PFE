using UnityEngine;
namespace PlayerSound
{
	public class NoiseRadius : MonoBehaviour
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private GameObject noiseEffect;
		
		private IDetected target;
		[SerializeField] private float minSpeed;
		
		[Range(0f, 10f)]
		[field:SerializeField] public float NoiseEffect{get; private set;}

		private void Start()
		{
			target = GetComponentInParent<IDetected>();
		}
		private void Update()
		{
			if (target.speed <= minSpeed)
			{
				noiseEffect.SetActive(false);
			}
			else
			{
				noiseEffect.SetActive(true);
				collider.radius = target.speed / NoiseEffect;
			}
			
		}
	}
}
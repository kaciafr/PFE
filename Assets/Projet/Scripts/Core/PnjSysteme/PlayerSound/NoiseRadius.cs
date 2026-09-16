using UnityEngine;

namespace PlayerSound
{
	public class NoiseRadius : MonoBehaviour
	{
		[Range(0f, 10f)]
		[SerializeField] private SphereCollider collider;
		[SerializeField] private Rigidbody rd;
		[field:SerializeField] public float NoiseRad{get; private set;}

		private void Start()
		{
			rd = GetComponent<Rigidbody>();
			collider = GetComponent<SphereCollider>();
		}

		private void Update()
		{
			collider.radius = NoiseRad + rd.linearVelocity.magnitude;
		}
	}
}
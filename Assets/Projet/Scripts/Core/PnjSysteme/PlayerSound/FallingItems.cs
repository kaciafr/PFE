using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace PlayerSound
{
	public class FallingItems :  MonoBehaviour,ISondDetected
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private ItemsData items;
		[SerializeField] private LayerMask groundMask;
		[SerializeField] private float maxDetectionRadius;
		[SerializeField] private float distanceMaxOfDetection;
		public Transform Transform => transform;
		
		private const float Gravity = 9.81f;
		
		private float height;
		[SerializeField] private bool wasFalling;
		private bool IsFalling => !MathGame.RayCastCheck( transform.position,Vector3.down,
			distanceMaxOfDetection, groundMask,
			out RaycastHit hit, Color.chartreuse);

		private void Start()
		{
			collider = GetComponent<SphereCollider>();
		}

		private void Update()
		{
			bool isFallingNow = IsFalling;

			if (isFallingNow && !wasFalling)
			{
				height = transform.position.y;
			}
			else if (!isFallingNow && wasFalling)
			{
				float fallDistance = height - transform.position.y;
				float impactForce = MathGame.CalculateImpactForce(fallDistance, Gravity, items.Mass, items.StoppingDistance);
				TriggerSoundDetection(impactForce);
			}

			wasFalling = isFallingNow;
		}
		private void TriggerSoundDetection(float force)
		{
			if (collider == null)
				return;

			float t = Mathf.InverseLerp(0f, items.MaxImpactForce, force);
			float radius = Mathf.Lerp(0, maxDetectionRadius, t);

			StartCoroutine(PulseCollider(radius));
		}

		private IEnumerator PulseCollider(float radius)
		{
			collider.radius = radius;
			collider.isTrigger = true;

			yield return new WaitForSeconds(1);
			
			collider.radius = 0f;

		}


	}
}
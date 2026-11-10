using System;
using System.Collections;
using Goblfin.Data;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class FallingItems : MonoBehaviour, ISondDetected,IPrey
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private ItemsData items;
		[SerializeField] private LayerMask groundMask;
		[SerializeField] private float maxDetectionRadius;
		[SerializeField] private float distanceMaxOfDetection;
		[SerializeField] private bool wasFalling;
		public Transform Transform => transform;
		private const float Gravity = 9.81f;
		private float height;
		[SerializeField] ActiveRagdoll ragdoll;
		[SerializeField] KillZone killZone;

		public event Action FallImpact;
		
		private bool IsFalling => !MathGame.RayCastCheck( transform.position,Vector3.down,
			distanceMaxOfDetection, groundMask,
			out RaycastHit hit, Color.chartreuse);
		

		private void Update()
		{
			if (ragdoll != null)
				ragdoll.enabled = wasFalling;
			if (killZone != null)
				killZone.enabled = wasFalling;
			collider.enabled = false;
			
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
			FallImpact?.Invoke();
			if (collider == null)
				return;
			
			float radius = Mathf.Lerp(0, force, 0.3f);

			StartCoroutine(PulseCollider(radius));
		}

		private IEnumerator PulseCollider(float radius)
		{
			collider.radius = radius;
			//noiseGO.SetActive(true);
			collider.isTrigger = true;

			yield return new WaitForSeconds(1);
			
			//noiseGO.SetActive(false);
			collider.radius = 0f;
		}


		public void Catches()
		{
			return;
		}
	}
}
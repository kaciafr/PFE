using System.Collections;
using Goblfin.Data;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class FallingItems : MonoBehaviour, ISondDetected
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
		private ActiveRagdoll ragdoll;
		private KillZone killZone;
		
		private bool IsFalling => !MathGame.RayCastCheck( transform.position,Vector3.down,
			distanceMaxOfDetection, groundMask,
			out RaycastHit hit, Color.chartreuse);

		private void Start()
		{
			TryGetComponent(out ragdoll);
			if (ragdoll != null)
				ragdoll.enabled = false;

			TryGetComponent(out killZone);
			if (killZone != null)
				killZone.enabled = false;
		}

		private void Update()
		{
			
			if (collider.radius <= 0.02f)
			{
				if (ragdoll != null)
					ragdoll.enabled = false;
				if (killZone != null)
					killZone.enabled = false;
				collider.enabled = false;

			}
			else
			{
				if (ragdoll != null)
					ragdoll.enabled = true;
				if (killZone != null)
					killZone.enabled = true;
				collider.enabled = true;

			}
				
			
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
			//noiseGO.SetActive(true);
			collider.isTrigger = true;

			yield return new WaitForSeconds(1);
			
			//noiseGO.SetActive(false);
			collider.radius = 0f;
		}


	}
}
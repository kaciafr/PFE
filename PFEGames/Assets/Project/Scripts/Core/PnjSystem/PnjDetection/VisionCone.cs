using System;
using Goblfin.PnjSystem.PnjDetection;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class VisionCone : DetectionSense
	{
		[Header("Field of view settings")]
		[field: SerializeField] public float ViewRadius { get; private set; } = 10f;
		[field: SerializeField][Range(0, 360)] public float ViewAngle { get; private set; } = 90f;
		[SerializeField] private float contactDistance = 0.2f;

		
		[Header("Détection progressive")]
		[SerializeField] private float fillSpeed = 0.6f;
		[SerializeField] private float decaySpeed = 0.25f;
		[SerializeField, Range(0f, 1f)] private float minFactor = 0.5f;
		private VisionMeter meter;
		private float lastScanTime;
		private IDetected currentTarget;
		[field: SerializeField] public event Action<Vector3, IDetected> OnTargetSeen;
		[field: SerializeField] public event Action<Vector3> Target;
		[field: SerializeField] public event Action LostTarget;
		

		protected override void OnEnable()
		{
			meter ??= new VisionMeter(fillSpeed, decaySpeed);   
			meter.Reset();                                          
			currentTarget = null;
			lastScanTime = Time.time;
			base.OnEnable();
		}
		public bool CanSee(IDetected target)
		{
			return CanSeePoint(target.transform.position);
		}

		public bool SeeFriendChasing(BrainPnj friend)
		{
			if (friend == null || friend.PnjStates is not ChaseState) return false;
			return CanSeePoint(friend.transform.position + Vector3.up * 1.2f, friend.transform);
		}

		public bool CanSeePoint(Vector3 targetPos, Transform ignore = null)
		{
			Vector3 origin = SensorOrigin;
			Vector3 toTarget = targetPos - origin;
			float dist = toTarget.magnitude;

			if (dist > ViewRadius) return false;
			if (dist < 0.0001f) return true;

			Vector3 dir = toTarget / dist;
			if (!PassesFilter(dir, dist)) return false;

			if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
			{
				LostTarget?.Invoke();
				if (ignore == null || !hit.transform.IsChildOf(ignore))
					return false;
			}
			Target?.Invoke(targetPos);
			return true;
		}
		protected override void Scan()
		{
			float dt = Mathf.Min(Time.time - lastScanTime, 0.25f);
			lastScanTime = Time.time;

			detectedTargets.Clear();

			Transform targetTransform = FindTargetInRadius(ViewRadius);
			IDetected target = targetTransform != null ? targetTransform.GetComponent<IDetected>() : null;
			if (target != null)
			{
				Remember(targetTransform);
				currentTarget = target;

				meter.IncreaseFromDetection(Mathf.Lerp(minFactor, 1f, MathGame.GetDistanceFactor(target.transform, SensorOrigin,ViewRadius)),
					Mathf.Lerp(minFactor, 1f, MathGame.GetAngleFactor(target.transform,transform,SensorOrigin,ViewAngle)),
					dt* target.VisibilityState);
			}
			else
			{
				meter.Decrease(dt);
				if (meter.Value <= 0f) currentTarget = null;
			}
			
			if ( meter.Value >= 1f)
				OnTargetSeen?.Invoke(LastPos, target);
		}
		
		protected override bool PassesFilter(Vector3 dirToTarget, float distToTarget)
		{
			Vector3 flatDir = new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized;
			Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

			return Vector3.Angle(flatForward, flatDir) < ViewAngle / 2f;
		}
		protected override void OnDrawGizmos()
		{
			Vector3 origin = SensorOrigin;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(origin, ViewRadius);

			Vector3 leftBoundary = MathGame.DirFromAngle(-ViewAngle / 2f,this.transform);
			Vector3 rightBoundary = MathGame.DirFromAngle(ViewAngle / 2f,this.transform);

			Gizmos.DrawLine(origin, origin + leftBoundary * ViewRadius);
			Gizmos.DrawLine(origin, origin + rightBoundary * ViewRadius);

			Gizmos.color = Color.red;
			foreach (Transform target in detectedTargets)
			{
				if (target != null)
					Gizmos.DrawLine(origin, target.position);
			}
		}
		
		public void VisionNerf() => minFactor = 0;
		public void VisionNormal() => minFactor = 0.7f;
		public void PersonalVisionValue(float value) => minFactor = value;

	}
}
using System;
using UnityEngine;

namespace PnjDetection
{
	public class VisionCone : DetectionSense
	{
		[Header("Réglages du cône de vision")]
		[field: SerializeField] public float ViewRadius { get; private set; } = 10f;
		[Range(0, 360)]
		[field: SerializeField] public float ViewAngle { get; private set; } = 90f;

		public event Action<Vector3,IDetected> OnTargetSeen;

		public bool CanSee(IDetected target)
		{
			if (target == null)
				return false;
			Vector3 origin = SensorOrigin;
			Vector3 targetPos = target.transform.position;
			Vector3 toTarget = targetPos - origin;
			float dist = toTarget.magnitude;
			

			if (dist > ViewRadius)
				return false;

			if (dist < 0.0001f)
				return true; 

			Vector3 dir = toTarget / dist;

			if (!PassesFilter(dir, dist))
				return false;

			if (Physics.Raycast(origin, dir, dist, obstacleMask))
				return false;
			return true;
		}
		protected override void Scan()
		{
			detectedTargets.Clear();

			
			Transform transform = FindTargetInRadius(ViewRadius);
			if (transform == null)
				return;
			IDetected target = transform.GetComponentInParent<IDetected>();
			
			OnTargetSeen?.Invoke(LastPos, target);
			Remember(transform);
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

			Vector3 leftBoundary = DirFromAngle(-ViewAngle / 2f);
			Vector3 rightBoundary = DirFromAngle(ViewAngle / 2f);

			Gizmos.DrawLine(origin, origin + leftBoundary * ViewRadius);
			Gizmos.DrawLine(origin, origin + rightBoundary * ViewRadius);

			Gizmos.color = Color.red;
			foreach (Transform target in detectedTargets)
			{
				if (target != null)
					Gizmos.DrawLine(origin, target.position);
			}
		}
		Vector3 DirFromAngle(float angleInDegrees)
		{
			angleInDegrees += transform.eulerAngles.y;
			return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}

		
	}
}
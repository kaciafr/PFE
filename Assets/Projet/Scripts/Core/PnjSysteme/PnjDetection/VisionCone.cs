using System;
using CharacterController.Script;
using UnityEngine;

namespace PnjDetection
{
	public class VisionCone : DetectionSense
	{
		[Header("Réglages du cône de vision")]
		[field: SerializeField] public float ViewRadius { get; private set; } = 10f;
		[Range(0, 360)]
		[field: SerializeField] public float ViewAngle { get; private set; } = 90f;

		public event Action<Vector3,CharacterSetup> OnTargetSeen;

		protected override void Scan()
		{
			detectedTargets.Clear();

			Transform target = FindTargetInRadius(ViewRadius);
			if (target == null)
				return;
			CharacterSetup player = target.GetComponent<CharacterSetup>();
			
			OnTargetSeen?.Invoke(LastPos, player);
			Remember(target);
		}
		
		protected override bool PassesFilter(Vector3 dirToTarget, float distToTarget)
		{
			return Vector3.Angle(transform.forward, dirToTarget) < ViewAngle / 2f;
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
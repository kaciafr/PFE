using UnityEngine;

namespace Goblfin.PnjSystem.Utilities
{
	public static class MathGame
	{
		public static float CalculateImpactForce(float fallDistance,float gravity,float massOfObj,float stoppingDistance)
		{
			if (fallDistance <= 0f)
				return 0f;
			
			float velocity = Mathf.Sqrt(2f * gravity * fallDistance);
			
			float force = (0.5f * massOfObj * velocity) / stoppingDistance;

			return force;
		}
		
		public static bool RayCastCheck(Vector3 origin, Vector3 direction, float distance, LayerMask mask, out RaycastHit hit, Color debugColor)
		{
			Vector3 dir = direction.normalized;
			Debug.DrawRay(origin, dir * distance, debugColor, 0.1f);
			return Physics.Raycast(origin, dir, out hit, distance, mask);
		}
		
		public static Vector3 DirFromAngle(float angleInDegrees,Transform subject)
		{
			angleInDegrees += subject.transform.eulerAngles.y;
			return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}
		
		public static float GetDistanceFactor(Transform target,Vector3 sensorOrigin,float viewRadius)
		{
			float dist = Vector3.Distance(sensorOrigin, target.transform.position);
			return Mathf.Clamp01(1f - (dist / viewRadius));
		}
		public static float GetAngleFactor(Transform target,Transform source,Vector3 sensorOrigin,float viewAngle)
		{
			Vector3 dirToTarget = (target.transform.position - sensorOrigin).normalized;
			float angle = Vector3.Angle(source.transform.forward, dirToTarget);
			return Mathf.Clamp01(1f - (angle / (viewAngle / 2f)));
		}
		
	}
}
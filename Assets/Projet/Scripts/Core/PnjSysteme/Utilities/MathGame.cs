using UnityEngine;

namespace Utilities
{
	public static class MathGame
	{
		public static float CalculateImpactForce(float fallDistance,float gravity,float massOfObj,float stoppingDistance)
		{
			if (fallDistance <= 0f)
				return 0f;
			
			float velocity = Mathf.Sqrt(2f * gravity * fallDistance);
			
			float force = (0.5f * massOfObj * velocity * velocity) / stoppingDistance;

			return force;
		}
		
		public static bool RayCastCheck(Vector3 origin, Vector3 direction, float distance, LayerMask mask, out RaycastHit hit, Color debugColor)
		{
			Vector3 dir = direction.normalized;
			Debug.DrawRay(origin, dir * distance, debugColor, 0.1f);
			return Physics.Raycast(origin, dir, out hit, distance, mask);
		}
	}
}
using UnityEngine;

namespace CharacterController.Script.PlayerControllers.Utilities
{
	public static class RaycastChecker
	{
		public static bool Check(Vector3 origin, Vector3 direction, float distance, LayerMask mask, out RaycastHit hit,
			Color debugColor)
		{
			Debug.DrawRay(origin, direction.normalized * distance, debugColor, 0.1f);
			return Physics.Raycast(origin, direction.normalized, out hit, distance, mask);
		}
	}
}
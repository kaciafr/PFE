using Unity.Cinemachine;
using UnityEngine;

namespace Script.PlayerControllers
{
	public class CharacterCamera : MonoBehaviour
	{
		public CinemachineCamera cam;
		public Quaternion targetRotation;

		private void LateUpdate()
		{
			var camForward = cam.transform.forward;
			camForward.y = 0;
			camForward.Normalize();

			targetRotation = Quaternion.LookRotation(camForward);
		}
	}
}
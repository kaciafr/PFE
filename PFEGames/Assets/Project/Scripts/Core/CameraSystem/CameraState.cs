using UnityEngine;

namespace Goblfin
{
	[CreateAssetMenu(menuName = "Camera/Camera State")]
	public class CameraState : ScriptableObject
	{
		[Header("Position")]
		public Vector3 followOffset = new Vector3(0, 1.2f, -6f);

		[Header("Lentille")]
		public float fov = 55f;

		[Header("Cadrage")]
		public float screenY = -0.2f;

		[Header("Transition")]
		public float smooth = 3f;
	}
}
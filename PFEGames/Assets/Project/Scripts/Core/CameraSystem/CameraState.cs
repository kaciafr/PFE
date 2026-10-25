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
		[Tooltip("Position du joueur a l'ecran : -0.5 = tout a gauche, 0 = centre, 0.5 = tout a droite")]
		public float screenX = 0f;
		[Tooltip("Position du joueur a l'ecran : -0.5 = tout en bas, 0 = centre, 0.5 = tout en haut")]
		public float screenY = -0.2f;
		[Tooltip("Coché : le joueur passe du côté opposé à sa direction pour voir devant lui (Screen X sert de distance au centre)")]
		public bool followDirection = false;

		[Header("Transition")]
		public float smooth = 3f;
	}
}
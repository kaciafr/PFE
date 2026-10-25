using Goblfin.CharactersSystem.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Goblfin
{
	public class CameraController : MonoBehaviour
	{
		public static CameraController Instance { get; private set; }

		[SerializeField] CinemachineCamera vcam;
		[SerializeField] CameraState defaultState;

		CinemachineFollow follow;
		CinemachineRotationComposer composer;
		CameraState current;
		PlayerManager player;
		float lookSide = -1f;

		void Awake()
		{
			Instance = this;
			player = FindFirstObjectByType<PlayerManager>();

			follow = vcam.GetComponent<CinemachineFollow>();
			composer = vcam.GetComponent<CinemachineRotationComposer>();

			if (follow == null || composer == null)
			{
				Debug.LogError("La CinemachineCamera doit avoir Position Control = Follow et Rotation Control = Rotation Composer.", this);
				enabled = false;
				return;
			}

			current = defaultState;

			// Placement immédiat au démarrage
			follow.FollowOffset = current.followOffset;
			SetFov(current.fov);
			SetScreen(ScreenX(), current.screenY);
		}

		[Header("Debug")]
		[SerializeField, Tooltip("Vue active en ce moment (lecture seule).")]
		string activeState;

		public void Request(CameraState state) => current = state;
		public void Release() => current = defaultState;

		void Update()
		{
			activeState = current != null ? current.name : "aucune";

			float t = 1f - Mathf.Exp(-current.smooth * Time.deltaTime);

			follow.FollowOffset = Vector3.Lerp(follow.FollowOffset, current.followOffset, t);
			SetFov(Mathf.Lerp(vcam.Lens.FieldOfView, current.fov, t));
			Vector2 screen = composer.Composition.ScreenPosition;
			SetScreen(Mathf.Lerp(screen.x, ScreenX(), t), Mathf.Lerp(screen.y, current.screenY, t));
		}

		float ScreenX()
		{
			if (!current.followDirection || player == null || player.Motor == null) return current.screenX;

			// 1. Le joueur regarde vers la droite de l'écran (> 0) ou vers la gauche (< 0) ?
			float facing = Vector3.Dot(player.Facing, vcam.transform.right);

			// 2. On change de côté seulement s'il va vraiment à gauche ou à droite (pas vers la caméra)
			if (Mathf.Abs(facing) > 0.3f)
				lookSide = facing > 0f ? -1f : 1f;

			// 3. Il va à droite : on le place à gauche pour voir devant lui, et inversement
			return Mathf.Abs(current.screenX) * lookSide;
		}

		void SetFov(float value)
		{
			var lens = vcam.Lens;          
			lens.FieldOfView = value;
			vcam.Lens = lens;
		}

		void SetScreen(float x, float y)
		{
			var comp = composer.Composition;
			comp.ScreenPosition = new Vector2(x, y);
			composer.Composition = comp;
		}
	}
}
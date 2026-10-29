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
		[SerializeField] private float sprintFovBonus = 4f;
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
			float targetFov = current.fov + sprintFovBonus * SprintAmount();
			SetFov(Mathf.Lerp(vcam.Lens.FieldOfView, targetFov, t));
			Vector2 screen = composer.Composition.ScreenPosition;
			SetScreen(Mathf.Lerp(screen.x, ScreenX(), t), Mathf.Lerp(screen.y, current.screenY, t));
		}

		float ScreenX()
		{
			if (!current.followDirection || player == null || player.Motor == null) return current.screenX;

			float facing = Vector3.Dot(player.Facing, vcam.transform.right);

			if (Mathf.Abs(facing) > 0.3f)
				lookSide = facing > 0f ? -1f : 1f;

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

		float SprintAmount()
		{
			if (player == null) return 0f;
			var move = player.Settings.Movement;
			return Mathf.InverseLerp(move.WalkSpeed, move.SprintSpeed, player.HorizontalSpeed);
		}
	}
}
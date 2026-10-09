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

		void Awake()
		{
			Instance = this;

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
			SetScreenY(current.screenY);
		}

		public void Request(CameraState state) => current = state;
		public void Release() => current = defaultState;

		void Update()
		{
			float t = 1f - Mathf.Exp(-current.smooth * Time.deltaTime);

			follow.FollowOffset = Vector3.Lerp(follow.FollowOffset, current.followOffset, t);
			SetFov(Mathf.Lerp(vcam.Lens.FieldOfView, current.fov, t));
			SetScreenY(Mathf.Lerp(composer.Composition.ScreenPosition.y, current.screenY, t));
		}

		void SetFov(float value)
		{
			var lens = vcam.Lens;          
			lens.FieldOfView = value;
			vcam.Lens = lens;
		}

		void SetScreenY(float value)
		{
			var comp = composer.Composition;   
			comp.ScreenPosition.y = value;
			composer.Composition = comp;
		}
	}
}
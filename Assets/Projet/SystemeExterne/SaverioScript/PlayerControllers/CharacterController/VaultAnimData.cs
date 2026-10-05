using UnityEngine;

namespace CharacterController.Script
{
	[CreateAssetMenu(fileName = "VaultAnimData", menuName = "Game/Vault Animation Data")]
	public class VaultAnimData : ScriptableObject
	{
		[Header("Identification")] public string animStateName;

		public float speedMin;

		[Header("Detection")] public float forwardCheckDistance;

		public float maxObstacleHeight;
		public float minObstacleHeight;

		[Header("Match Target")] public AvatarTarget matchBodyPart;

		public Vector3 matchPosWeight = new(0, 1, 0);
		public float matchStartTime = 0.1f;
		public float matchTargetTime = 0.4f;
	}
}
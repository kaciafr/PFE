using System.Collections.Generic;
using UnityEngine;

namespace CharacterController.Script
{
	[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData/MainCharacterData")]
	public class CharacterData : ScriptableObject
	{
		[Header("MovementStats")] public float playerVelocity;

		public float playerRunVelocity;
		public float playerAcceleration;
		public float playerDeceleration;
		public float rotationSpeed = 10f;
		public float jumpForce;
		public float standHeight = 1.8f;
		public float standCenter = 0.9f;

		[Header("Physics")] public float cruchCenter = 0.45f;

		public float cruchHeight = 0.9f;

		[Header("Vault Speed Thresholds")] public float vaultAutoSpeedThreshold = 4f;

		[Header("Vault")] public float vaultOrigin = 0.4f;

		[Header("VaultAnimation")] public VaultAnimData[] vaultAnimations;

		[Header("Gravity")] public float gravity = -9.81f;
		[Header("ComboAttack")] public AttackComboData firstAttack;
	}
}
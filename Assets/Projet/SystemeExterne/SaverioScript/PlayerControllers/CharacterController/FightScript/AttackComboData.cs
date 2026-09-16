using UnityEngine;

namespace CharacterController.Script
{
	[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData/AttackComboData")]
	public class AttackComboData : ScriptableObject
	{
		[Header("Animation")] public string animStateName;

		[Header("Combo Window")] [Range(0f, 1f)]
		public float comboWindowStart = 0.1f;

		[Range(0f, 1f)] public float comboWindowEnd = 1f;

		[Header("Transition Retour")] public float exitCrossFadeDuration = 0.15f;

		[Header("Chaîne")] public AttackComboData nextAttack;
		[Header("HitBox")] public HitBoxData hitBox;
	}
}
using UnityEngine;

namespace CharacterController.Script
{
	public class HitBoxData : ScriptableObject
	{
		[Header("Attache")] public string boxName;
		[Header("Timing")] [Range(0f, 1f)] public float startTime = 0.2f;
		[Range(0f, 1f)] public float endTime = 0.35f;

		[Header("Dégâts")] public int damage = 10;
		public float knockback = 2f;
	}
}
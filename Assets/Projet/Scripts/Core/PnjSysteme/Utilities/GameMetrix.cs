using UnityEngine;

namespace Utilities
{
	public static class GameMetrix
	{
		[Header("GardeSettings")]
		[field: SerializeField]
		public static float GardePatrolSpeed { get; private set; } = 0.6f;
		[field: SerializeField] public static float GardeChaseSpeed { get; private set; } = 2;
		[field: SerializeField] public static float TimeToSearch { get; private set; } = 5;
		[field: SerializeField] public static float TimeToBeSurprise { get; private set; } = 1f;

	}
}
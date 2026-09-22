using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Utilities
{
	public static class GameMetrix
	{
		[Header("GardeSettings")]
		[field: SerializeField]
		public static float GuardPatrolSpeed { get; private set; } = 0.6f;
		[field: SerializeField] public static float GuardChaseSpeed { get; private set; } = 2;
		[field: SerializeField] public static float TimeToSearch { get; private set; } = 5;
		[field: SerializeField] public static float TimeToBeSurprise { get; private set; } = 1f;
		
		public static int SearchPointCount = 4;
		public static float SearchRadius = 6f;        
		public static float SearchWaitPerPoint = 1.5f;

		public static List<Vector3> SearchPointsGenerated(Vector3 center, float radius, int count)
		{
			List<Vector3> points = new List<Vector3>();

			for (int i = 0; i < count; i++)
			{
				Vector3 randomOffset = Random.insideUnitSphere * radius;
				randomOffset.y = 0;

				Vector3 candidate = center + randomOffset;

				if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
				{
					points.Add(hit.position);
				}
			}
			return points;
		}

	}
}
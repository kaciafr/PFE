using UnityEngine;
using UnityEngine.AI;

namespace Utilities
{
	public static class Direction
	{
		public static Vector3 GetDirection(NavMeshAgent agent, Vector3 target)
		{
			agent.SetDestination(target);
			
			Vector3 direction = (target - agent.transform.position).normalized;

			return direction;
		}
	}
}
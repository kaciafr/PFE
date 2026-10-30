using UnityEngine;
 namespace Goblfin.PnjSystem
{
	public class FollowFriend : IPnjStates
	{
		private BrainPnj friend;
		public StatePriority Priority => StatePriority.Following;
		public float AgentSpeed => GameMetrix.GuardChaseSpeed;

		public FollowFriend(BrainPnj friend)
		{
			this.friend = friend;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.material.color = Color.cyan;
			brainPnj.Agent.speed = AgentSpeed;
			brainPnj.Agent.SetDestination(friend.transform.position);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
			if(friend.PnjStates is not ChaseState)
				brainPnj.PnjGoTo(new PatrolState(brainPnj));
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
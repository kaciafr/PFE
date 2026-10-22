using UnityEngine;
 namespace Goblfin.PnjSystem
{
	public class FollowFriend : IPnjStates
	{
		private BrainPnj friend;
		public StatePriority Priority => StatePriority.Following;

		public FollowFriend(BrainPnj friend)
		{
			this.friend = friend;
		}

		public void EnterState(BrainPnj brainPnj)
		{
			brainPnj.material.color = Color.cyan;
			brainPnj.Agent.speed = GameMetrix.GuardChaseSpeed;
			brainPnj.Agent.SetDestination(friend.transform.position);
		}

		public void UpdateState(BrainPnj brainPnj)
		{
		}

		public void ExitState(BrainPnj brainPnj)
		{
		}
	}
}
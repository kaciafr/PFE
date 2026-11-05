using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay.Animation
{
	public class PlayAnimation : MonoBehaviour
	{
		[SerializeField] private BrainPnj brainPnj;
		[SerializeField] private Animator animator;
		private float currentSpeed;
		private ThrowThingsManager thrower;
		private KillZone killZone;

		private void Start()
		{
			killZone = brainPnj.GetAptitude<KillZone>();
			thrower = brainPnj.GetAptitude<ThrowThingsManager>();
		}
		
		private void Update()
		{
			currentSpeed = brainPnj.Agent.speed;
			animator.SetFloat("Speed", currentSpeed);
		}
		
		public void AnimationEvent_ThrowStone()
		{
			if (thrower != null)
			{
				thrower.TryThrow(thrower.TargetPos);
			}
		}
		
	}
}

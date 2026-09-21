using System;
using PnjDetection;
using UnityEngine;

namespace GamePlay.Animation
{
	public class PlayAnimation : MonoBehaviour
	{
		[SerializeField] private BrainPnj brainPnj;
		[SerializeField] private Animator animator;
		private float currentSpeed;
		private KillZone killZone;

		private void Start()
		{
			killZone = brainPnj.GetAptitude<KillZone>();
		}
		
		private void Update()
		{
			UpdateAnimation();
		}
		
		private void UpdateAnimation()
		{
			currentSpeed = brainPnj.Agent.speed;
			animator.SetFloat("Speed", currentSpeed);
		}
		
		private void Penalty()
		{
			animator.SetTrigger("Penalty");
		}
	}
}

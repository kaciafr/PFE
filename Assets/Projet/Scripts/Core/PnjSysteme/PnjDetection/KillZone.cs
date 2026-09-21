using System;
using GamePlay;
using UnityEngine;

namespace PnjDetection
{
	public class KillZone : DetectionSense
	{
		[SerializeField] private float killRadius;
		public event Action OnDeath;
		
		protected override void Scan()
		{
			detectedTargets.Clear();
			
			Transform dangerTarget = FindTargetInRadius(killRadius);
			if (dangerTarget == null)
				return;
			
			Debug.Log("Dead");
			OnDeath?.Invoke();
		}
		
		protected override void OnDrawGizmos()
		{
			Vector3 origin = SensorOrigin;

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(origin, killRadius);
			
			base.OnDrawGizmos(); 
		}

		
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace PnjDetection
{
	public class VisionCone : PnjAptitude
	{
		[Header("Réglages du cône de vision")]
		public float viewRadius = 10f;          
		[Range(0, 360)]
		public float viewAngle = 90f;           

		[Header("Filtrage")]
		[SerializeField] private LayerMask targetMask;            
		[SerializeField] private LayerMask obstacleMask;          

		[Header("Mémoire du NPC")]
		[SerializeField] private List<Transform> visibleTargets = new List<Transform>();

		[field:SerializeField] public Vector3 lastPos { get; private set; }

		void Start()
		{
			InvokeRepeating(nameof(FindVisibleTargets), 0f, 0.2f);
		}

		void FindVisibleTargets()
		{
			visibleTargets.Clear();

        
			Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

			foreach (Collider targetCollider in targetsInRadius)
			{
				Transform target = targetCollider.transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;

            
				if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
				{
					float distToTarget = Vector3.Distance(transform.position, target.position);

                
					bool isBlocked = Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask);

					if (!isBlocked)
					{
						visibleTargets.Add(target);
					}
				}
				else
				{
					lastPos = target.position;
				}
			}
		}

    
		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, viewRadius);

			Vector3 leftBoundary = DirFromAngle(-viewAngle / 2f);
			Vector3 rightBoundary = DirFromAngle(viewAngle / 2f);

			Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
			Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

			Gizmos.color = Color.red;
			foreach (Transform target in visibleTargets)
			{
				Gizmos.DrawLine(transform.position, target.position);
			}
		}

		Vector3 DirFromAngle(float angleInDegrees)
		{
			angleInDegrees += transform.eulerAngles.y;
			return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}
	}
}
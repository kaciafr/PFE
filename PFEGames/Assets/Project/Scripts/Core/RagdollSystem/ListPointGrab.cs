using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Project.Scripts.Core.RagdollSysteme
{
	public class ListPointGrab : MonoBehaviour
	{
		[field:SerializeField] public List<Rigidbody> PointGrab{get;private set;}

		private void Awake()
		{
			foreach (Rigidbody rb in PointGrab)
			{
				rb.mass = GameMetrix.MassNoGrab;
			}
		}
	}
}

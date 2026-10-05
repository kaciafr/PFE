using System.Collections.Generic;
using UnityEngine;

namespace RagdollSysteme
{
	public class ListPointGrab : MonoBehaviour
	{
		[field:SerializeField] public List<Rigidbody> PointGrab{get;private set;}

		private void Awake()
		{
			foreach (Rigidbody rb in PointGrab)
			{
				rb.mass = 1;
			}
		}
	}
}

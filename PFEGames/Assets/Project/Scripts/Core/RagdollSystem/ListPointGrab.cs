using System.Collections.Generic;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.RagdollSystem
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

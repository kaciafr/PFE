using System.Collections.Generic;
using UnityEngine;

namespace RagdollSysteme
{
	public class ListPointGrab : MonoBehaviour
	{
		[field:SerializeField] public List<Rigidbody> PointGrab{get;private set;}
	}
}

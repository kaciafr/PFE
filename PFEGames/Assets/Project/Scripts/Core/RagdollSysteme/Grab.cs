using System.Collections.Generic;
using Project.Scripts.Core.RagdollSysteme;
using UnityEngine;
using Utilities;

namespace RagdollSysteme
{
	public class Grab : MonoBehaviour
	{
		[SerializeField] private SpringJoint joint;
		[field:SerializeField] public List<Rigidbody> pointJoint { get; private set; }= new List<Rigidbody>();
		private ListPointGrab currentTarget;
		private bool isGrabbing;

		private void OnTriggerEnter(Collider other)
		{
			if (isGrabbing) return;

			ListPointGrab pointGrab = other.GetComponentInParent<ListPointGrab>();
			if (pointGrab == null)
				return;

			currentTarget = pointGrab;
			foreach (Rigidbody rb in currentTarget.PointGrab)
				pointJoint.Add(rb);
		}

		private void OnTriggerExit(Collider other)
		{
			if (isGrabbing) return;
			currentTarget = null;
			pointJoint.Clear();
		}

		public void TryGrab()
		{
			if (pointJoint[0].mass > GameMetrix.MaxMassGrab)
			{
				Debug.Log("Trop lourd");
				return;
			}
			
			if (pointJoint.Count == 0)
				return;
			joint.connectedBody = pointJoint[0];
			
		}

		public void Release()
		{
			joint.connectedBody = null;
		}
	}
}
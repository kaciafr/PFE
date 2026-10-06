using System.Collections.Generic;
using Project.Scripts.Core.RagdollSysteme;
using UnityEngine;
using Utilities;

namespace RagdollSysteme
{
	public class Grab : MonoBehaviour
	{
		[SerializeField] private GameObject theGrabber;
		[SerializeField] private SpringJoint joint;
		[SerializeField] private List<Rigidbody> pointJoint = new List<Rigidbody>();
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
			if (pointJoint[0].mass > GameMetrix.MaxMassPool)
			{
				Debug.Log("Trop lourd");
				return;
			}
			if (isGrabbing)
			{
				Release();
			}
			else
			{
				if (pointJoint.Count == 0)
					return;
				joint.connectedBody = pointJoint[0];
				isGrabbing = true;
			}
		}

		private void Release()
		{
			joint.connectedBody = null;
			isGrabbing = false;
		}
	}
}
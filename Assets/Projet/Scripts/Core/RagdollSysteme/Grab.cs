using System.Collections.Generic;
using UnityEngine;

namespace RagdollSysteme
{
	public class Grab : MonoBehaviour
	{
		[SerializeField] private SpringJoint joint;
		[SerializeField] private List<Rigidbody> pointJoint = new List<Rigidbody>();
		private ListPointGrab currentTarget;
		private bool isGrabbing;

		private void OnTriggerStay(Collider other)
		{
			if (isGrabbing) return;

			ListPointGrab pointGrab = other.GetComponentInParent<ListPointGrab>();
			if (pointGrab == null)
				return;

			currentTarget = pointGrab;

			pointJoint.Clear();
			foreach (Rigidbody rb in currentTarget.pointGrab)
				pointJoint.Add(rb);
		}

		private void OnTriggerExit(Collider other)
		{
			if (isGrabbing) return;
			pointJoint.Clear();
			currentTarget = null;
		}

		public void TryGrab()
		{
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
using System.Collections.Generic;
using Goblfin.PnjSystem;
using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.RagdollSystem
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

		public bool IsHolding => joint != null && joint.connectedBody != null;

		// Vrai si un corps ragdoll est à portée (rempli par OnTriggerEnter)
		public bool HasTarget => pointJoint.Count > 0;

		public void TryGrab()
		{
			// D'abord verifier qu'il y a quelque chose a attraper (sinon pointJoint[0] plante)
			if (pointJoint.Count == 0)
				return;

			// Pas de SpringJoint assigné dans l'Inspector : on ne peut rien attraper
			if (joint == null)
			{
				Debug.LogWarning("Grab : le SpringJoint n'est pas assigné dans l'Inspector", this);
				return;
			}

			if (pointJoint[0].mass > GameMetrix.MaxMassGrab)
			{
				Debug.Log("Trop lourd");
				return;
			}

			joint.connectedBody = pointJoint[0];
		}

		public void Release()
		{
			// Sans joint, il n'y a rien à lâcher (et ça évite l'erreur qui bloquait le changement d'état)
			if (joint == null)
				return;

			joint.connectedBody = null;
		}
	}
}
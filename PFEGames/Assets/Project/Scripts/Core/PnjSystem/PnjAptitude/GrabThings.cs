using System.Collections.Generic;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class GrabThings : PnjAptitude
    {
        [SerializeField] private Transform socketTransform;
        [SerializeField] private string grabableLayer = "Grabable";

        private readonly HashSet<Collider> inZone = new HashSet<Collider>();
        private int layer = -1;
        private Rigidbody heldBody;
        private bool heldWasKinematic;

        private GameObject objectGrab;

        private void OnEnable()
        {
            layer = LayerMask.NameToLayer(grabableLayer);
            inZone.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"zone de grab : {other.name} | layer={LayerMask.LayerToName(other.gameObject.layer)}", other);
            if (other.gameObject.layer == layer) inZone.Add(other);
        }
        
        private void OnTriggerExit(Collider other) => inZone.Remove(other);
        
        public bool Grab(GameObject wanted)
        {
            objectGrab = wanted;

            heldBody = wanted.GetComponent<Rigidbody>();
            if (heldBody != null)
            {
                heldWasKinematic = heldBody.isKinematic;
                heldBody.isKinematic = true; 
            }

            wanted.transform.SetPositionAndRotation(socketTransform.position, socketTransform.rotation);
            wanted.transform.SetParent(socketTransform);
            return true;
        }

        public void UnGrab()
        {
            if (objectGrab != null)
            {
                objectGrab.transform.SetParent(null);
                if (heldBody != null) heldBody.isKinematic = heldWasKinematic;
            }
            objectGrab = null;
            heldBody = null;
        }
    }
}
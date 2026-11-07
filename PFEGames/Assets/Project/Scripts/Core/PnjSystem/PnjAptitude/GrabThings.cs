using System;
using System.Collections.Generic;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class GrabThings : PnjAptitude
    {
	    [SerializeField] private Transform socketTransform;
	    private bool grab =  false;
	    private HashSet<GameObject> grabbedObjects = new ();
	    private GameObject objectGrab;

	    private void OnTriggerStay(Collider other)
	    {
		    if (other.gameObject.layer == LayerMask.NameToLayer("Grabable"))
		    {
			    grabbedObjects.Add(other.gameObject);
		    }
	    }

	    private void OnTriggerExit(Collider other)
	    {
		    if (other.gameObject.layer == LayerMask.NameToLayer("Grabable"))
		    {
			    grabbedObjects.Remove(other.gameObject);
		    }
	    }

	    public void Grab()
	    {
		    if(grabbedObjects.Count == 0 || socketTransform ==null) return;

		    foreach (GameObject obj in grabbedObjects)
		    {
			    obj.transform.SetParent(socketTransform);
			    objectGrab = obj;
		    }
	    }
	    public void UnGrab()
	    {
		    objectGrab.transform.SetParent(null);
		    objectGrab = null;
	    }
    }
}

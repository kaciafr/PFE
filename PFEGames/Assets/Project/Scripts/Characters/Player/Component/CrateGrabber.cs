using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody))]
    public class CrateGrabber : MonoBehaviour
    {
        private Rigidbody rb;
        private PlayerSettings settings;
        private Rigidbody grabbedCrate;

        public bool IsGrabbing => grabbedCrate != null;
        public Rigidbody GrabbedCrate => grabbedCrate;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            settings = GetComponent<PlayerManager>().Settings;
        }

        public void Grab(Rigidbody crate)
        {
            if (crate == null || IsGrabbing) return;

            grabbedCrate = crate;
            grabbedCrate.isKinematic = false;
        }

        public void UnGrab()
        {
            if (grabbedCrate == null) return;

            grabbedCrate.linearVelocity = Vector3.zero;
            grabbedCrate.isKinematic = true;
            grabbedCrate = null;
        }

        private void FixedUpdate()
        {
            if (grabbedCrate == null) return;

            Vector3 v = rb.linearVelocity;
            grabbedCrate.linearVelocity = new Vector3(v.x, grabbedCrate.linearVelocity.y, v.z);

            if (Vector3.Distance(rb.position, grabbedCrate.position) > settings.Grab.BreakDistance)
                UnGrab();
        }

        private void OnDisable() => UnGrab();
    }
}
using System.Collections.Generic;
using Characters.Data;
using UnityEngine;

namespace Characters.Component
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody))]
    public class CrateGrabber : MonoBehaviour
    {
        private const RigidbodyConstraints LockXZ = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        private const float SettleDelay = 0.1f;
        private const float SettleSpeed = 0.05f;

        private Rigidbody rb;
        private PlayerSettings settings;
        private Rigidbody grabbedCrate;
        private readonly List<(Rigidbody crate, float releaseTime)> releasedCrates = new();

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
            
            if (crate.mass > settings.Grab.MaxGrabMass) return;

            grabbedCrate = crate;
            grabbedCrate.isKinematic = false;
            grabbedCrate.constraints &= ~LockXZ;
        }

        public void UnGrab()
        {
            if (grabbedCrate == null) return;

            grabbedCrate.linearVelocity = new Vector3(0f, grabbedCrate.linearVelocity.y, 0f);
            grabbedCrate.constraints |= LockXZ;
            releasedCrates.Add((grabbedCrate, Time.time));
            grabbedCrate = null;
        }

        private void FixedUpdate()
        {
            SettleReleasedCrates();

            if (grabbedCrate == null) return;

            Vector3 v = rb.linearVelocity;
            grabbedCrate.linearVelocity = new Vector3(v.x, grabbedCrate.linearVelocity.y, v.z);

            if (Vector3.Distance(rb.position, grabbedCrate.position) > settings.Grab.BreakDistance)
                UnGrab();
        }

        private void SettleReleasedCrates()
        {
            for (int i = releasedCrates.Count - 1; i >= 0; i--)
            {
                var (crate, releaseTime) = releasedCrates[i];

                if (crate == null || crate == grabbedCrate)
                {
                    releasedCrates.RemoveAt(i);
                    continue;
                }

                if (Time.time - releaseTime < SettleDelay || Mathf.Abs(crate.linearVelocity.y) > SettleSpeed)
                    continue;

                crate.linearVelocity = Vector3.zero;
                crate.isKinematic = true;
                releasedCrates.RemoveAt(i);
            }
        }

        private void OnDisable() => UnGrab();
    }
}
using System.Collections.Generic;
using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class CrateGrabber : MonoBehaviour
    {
        private const RigidbodyConstraints LockXZ = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        private const float SettleDelay = 0.1f;
        private const float SettleSpeed = 0.05f;
        private Rigidbody rb;
        private PlayerMotor motor;
        private PlayerSettings settings;

        private Rigidbody grabbedCrate;
        private Vector3 grabOffset;
        private Vector3 localGrabPoint;
        private Vector3 localGrabNormal;
        private readonly List<(Rigidbody crate, float releaseTime)> releasedCrates = new();

        public bool IsGrabbing => grabbedCrate != null;
        public Rigidbody Objectrb => grabbedCrate;

        public Vector3 GrabPoint => grabbedCrate.transform.TransformPoint(localGrabPoint);
        public Vector3 GrabNormal => grabbedCrate.transform.TransformDirection(localGrabNormal);

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            motor = GetComponent<PlayerMotor>();
            settings = GetComponent<PlayerManager>().Settings;
        }

        public void Grab(Rigidbody crate, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (crate == null || IsGrabbing) return;
            if (crate.mass > settings.Grab.MaxGrabMass) return;
            hitNormal.y = 0f;
            if (hitNormal.sqrMagnitude < 0.0001f) return;
            hitNormal.Normalize();

            grabbedCrate = crate;
            grabbedCrate.isKinematic = false;
            grabbedCrate.constraints &= ~LockXZ;

            localGrabPoint = crate.transform.InverseTransformPoint(hitPoint);
            localGrabNormal = crate.transform.InverseTransformDirection(hitNormal);

            SnapPoint snapPoint = SnapPoint.FindForFace(crate, hitPoint, hitNormal);
            if (snapPoint != null)
                PlacePlayer(snapPoint.transform.position, snapPoint.transform.forward);
            else
                PlacePlayer(hitPoint + hitNormal * settings.Grab.GrabDistance, -hitNormal);

            grabOffset = grabbedCrate.position - rb.position;
            grabOffset.y = 0f;
        }

        public void UnGrab()
        {
            if (grabbedCrate == null) return;

            grabbedCrate.linearVelocity = new Vector3(0f, grabbedCrate.linearVelocity.y, 0f);
            grabbedCrate.constraints |= LockXZ;
            releasedCrates.Add((grabbedCrate, Time.time));
            grabbedCrate = null;
        }

        private void PlacePlayer(Vector3 position, Vector3 lookDirection)
        {
            position.y = rb.position.y;
            rb.position = position;
            if (motor != null) motor.Face(lookDirection);
        }

        private void FixedUpdate()
        {
            SettleReleasedCrates();

            if (grabbedCrate == null) return;

            Vector3 velocity = (rb.position + grabOffset - grabbedCrate.position) / Time.fixedDeltaTime;
            grabbedCrate.linearVelocity = new Vector3(velocity.x, grabbedCrate.linearVelocity.y, velocity.z);

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

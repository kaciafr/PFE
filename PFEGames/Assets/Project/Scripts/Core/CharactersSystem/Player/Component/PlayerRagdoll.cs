using System.Collections.Generic;
using Goblfin.RagdollSystem;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class PlayerRagdoll : RagDoll
    {
        private Animator animator;
        private float deathPush;
        private Rigidbody playerBody;
        private Collider playerCollider;
        private readonly List<Rigidbody> bones = new();
        private readonly List<Collider> boneColliders = new();

        private Transform[] skeleton;
        private Vector3[] skeletonPositions;
        private Quaternion[] skeletonRotations;

        private void Awake()
        {
            PlayerManager player = GetComponentInParent<PlayerManager>();
            playerBody = player.GetComponent<Rigidbody>();
            playerCollider = player.GetComponent<Collider>();
            animator = FindCharacterAnimator(player.transform);
            deathPush = player.Settings.Death.DeathPush;

            if (animator == null) return;

            foreach (Rigidbody body in animator.GetComponentsInChildren<Rigidbody>())
            {
                if (body == playerBody) continue;
                bones.Add(body);
                boneColliders.AddRange(body.GetComponents<Collider>());
            }

            SetBonesPhysics(false);
            SaveSkeletonPose();
        }

        private void SaveSkeletonPose()
        {
            if (animator == null) return;

            skeleton = animator.GetComponentsInChildren<Transform>();
            skeletonPositions = new Vector3[skeleton.Length];
            skeletonRotations = new Quaternion[skeleton.Length];

            for (int i = 0; i < skeleton.Length; i++)
            {
                skeletonPositions[i] = skeleton[i].localPosition;
                skeletonRotations[i] = skeleton[i].localRotation;
            }
        }

        private void RestoreSkeletonPose()
        {
            if (skeleton == null) return;

            for (int i = 1; i < skeleton.Length; i++)
            {
                if (skeleton[i] == null) continue;
                skeleton[i].localPosition = skeletonPositions[i];
                skeleton[i].localRotation = skeletonRotations[i];
            }
        }

        private void OnEnable() => OnRagdoll += BecomeRagdoll;
        private void OnDisable() => OnRagdoll -= BecomeRagdoll;

        public void Kill()
        {
            if (animator == null)
            {
                return;
            }

            Ragdool(animator);
        }

        private void BecomeRagdoll()
        {
            Vector3 velocity = playerBody.linearVelocity - animator.transform.forward * deathPush;

            playerBody.linearVelocity = Vector3.zero;
            playerBody.isKinematic = true;
            if (playerCollider != null) playerCollider.enabled = false;

            HandIK handIK = animator.GetComponent<HandIK>();
            if (handIK != null) handIK.enabled = false;

            SetBonesPhysics(true);
            foreach (Rigidbody bone in bones)
                bone.linearVelocity = velocity;
        }

        private void SetBonesPhysics(bool enabled)
        {
            foreach (Rigidbody bone in bones)
                bone.isKinematic = !enabled;

            foreach (Collider boneCollider in boneColliders)
                boneCollider.enabled = enabled;
        }

        private static Animator FindCharacterAnimator(Transform root)
        {
            foreach (Animator candidate in root.GetComponentsInChildren<Animator>())
            {
                if (candidate.avatar != null) return candidate;
            }

            return null;
        }



        public void Revive()
        {
            SetBonesPhysics(false);
            RestoreSkeletonPose();
            playerBody.isKinematic = false;
            if (playerCollider != null) playerCollider.enabled = true;
            HandIK handIK = animator.GetComponent<HandIK>();
            if (handIK != null) handIK.enabled = true;

            WakeUp(animator);
        }
    }

}

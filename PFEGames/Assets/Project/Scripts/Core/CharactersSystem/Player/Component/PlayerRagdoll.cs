using System.Collections.Generic;
using Goblfin.Player;
using Goblfin.Player.Component;
using Goblfin.RagdollSystem;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class PlayerRagdoll : RagDoll
    {
        [Header("Cadavre")]
        [SerializeField] private PlayerCorpse corpsePrefab;
        [SerializeField, Min(1), Tooltip("Nombre max de cadavres dans la scène, le plus vieux retourne au pool.")]
        private int maxCorpses = 5;
        private readonly Queue<PlayerCorpse> corpses = new();

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



        private void LeaveCorpse()
        {
            if (corpsePrefab == null) return;

            if (corpses.Count >= maxCorpses)
                PrefabPool.Release(corpses.Dequeue().gameObject);

            GameObject corpseObject = PrefabPool.Get(corpsePrefab.gameObject, animator.transform.position, animator.transform.rotation);
            PlayerCorpse corpse = corpseObject.GetComponent<PlayerCorpse>();
            corpse.CopyPose(animator.transform);
            corpses.Enqueue(corpse);
        }

        public void Revive()
        {
            LeaveCorpse();

            SetBonesPhysics(false);
            RestoreSkeletonPose();
            playerBody.isKinematic = false;
            if (playerCollider != null) playerCollider.enabled = true;

            WakeUp(animator);
        }
    }

}

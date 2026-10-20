using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{

    [RequireComponent(typeof(Animator))]
    public class HandIK : MonoBehaviour
    {
        [SerializeField] private PlayerManager player;

        [Header("Mains")]
        [SerializeField, Min(0f), Tooltip("Demi-ecartement entre les mains (m).")]
        private float handSpacing = 0.25f;
        [SerializeField, Tooltip("Monte (+) ou descend (-) les mains (m).")]
        private float handHeightOffset = 0f;
        [SerializeField, Tooltip("Ecarte les mains de la caisse (+) ou les rentre dedans (-) (m).")]
        private float handDepthOffset = 0f;
        [SerializeField, Range(0f, 1f), Tooltip("Tourne aussi les paumes vers la caisse (0 = rotation de l'animation).")]
        private float rotationWeight = 0f;

        [Header("Transition et corps")]
        [SerializeField, Min(0f), Tooltip("Vitesse d'apparition / disparition de l'effet.")]
        private float blendSpeed = 6f;
        [SerializeField, Min(0f), Tooltip("De combien le modele peut avancer au maximum vers la caisse (m).")]
        private float maxBodyForward = 0.5f;

        private Animator animator;
        private CrateGrabber grabber;

        private float weight;
        private float bodyForward;
        private Vector3 startLocalPosition;
        private Vector3 crateNormal;

        private Vector3 leftTarget, rightTarget;
        private bool hasTargets;

        private Transform leftShoulder, rightShoulder;
        private float leftArmLength, rightArmLength;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (player == null) player = GetComponentInParent<PlayerManager>();
            grabber = GetComponentInParent<CrateGrabber>();
            startLocalPosition = transform.localPosition;

            leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftArmLength = ArmLength(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
            rightArmLength = ArmLength(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
        }

        private bool IsGrabbing => grabber != null && grabber.IsGrabbing;

        private void Update()
        {
            weight = Mathf.MoveTowards(weight, IsGrabbing ? 1f : 0f, blendSpeed * Time.deltaTime);

            if (IsGrabbing) crateNormal = grabber.GrabNormal;
            if (weight <= 0f) bodyForward = 0f;

            Vector3 offset = -crateNormal * (bodyForward * weight);
            if (transform.parent != null) offset = transform.parent.InverseTransformDirection(offset);
            transform.localPosition = startLocalPosition + offset;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            hasTargets = IsGrabbing && weight > 0f;
            if (hasTargets) ComputeHandTargets();

            float w = hasTargets ? weight : 0f;
            Quaternion palmRotation = hasTargets ? Quaternion.LookRotation(-crateNormal, Vector3.up) : Quaternion.identity;
            SetHand(AvatarIKGoal.LeftHand, leftTarget, palmRotation, w);
            SetHand(AvatarIKGoal.RightHand, rightTarget, palmRotation, w);
        }

        private void LateUpdate()
        {
            if (!hasTargets || leftShoulder == null || rightShoulder == null) return;

            float missingLeft = Vector3.Distance(leftShoulder.position, leftTarget) - leftArmLength * 0.95f;
            float missingRight = Vector3.Distance(rightShoulder.position, rightTarget) - rightArmLength * 0.95f;

            bodyForward = Mathf.Clamp(bodyForward + Mathf.Max(missingLeft, missingRight), 0f, maxBodyForward);
        }

        private void ComputeHandTargets()
        {
            Vector3 right = Vector3.Cross(Vector3.up, -crateNormal);
            Vector3 center = grabber.GrabPoint + Vector3.up * handHeightOffset + crateNormal * handDepthOffset;

            leftTarget = center - right * handSpacing;
            rightTarget = center + right * handSpacing;
        }

        private void SetHand(AvatarIKGoal goal, Vector3 position, Quaternion rotation, float positionWeight)
        {
            animator.SetIKPositionWeight(goal, positionWeight);
            animator.SetIKRotationWeight(goal, positionWeight * rotationWeight);
            if (positionWeight <= 0f) return;

            animator.SetIKPosition(goal, position);
            animator.SetIKRotation(goal, rotation);
        }

        private float ArmLength(HumanBodyBones upper, HumanBodyBones lower, HumanBodyBones hand)
        {
            Transform u = animator.GetBoneTransform(upper);
            Transform l = animator.GetBoneTransform(lower);
            Transform h = animator.GetBoneTransform(hand);
            if (u == null || l == null || h == null) return 0f;
            return Vector3.Distance(u.position, l.position) + Vector3.Distance(l.position, h.position);
        }

        private void OnDrawGizmosSelected()
        {
            if (!hasTargets) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftTarget, 0.04f);
            Gizmos.DrawWireSphere(rightTarget, 0.04f);
        }
    }
}

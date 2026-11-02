using Goblfin.CharactersSystem.Player;
using UnityEngine;

namespace Goblfin
{
    public class PlayerFootIK : MonoBehaviour
    {
        [SerializeField] LayerMask GroundLayer;
        [SerializeField] private float footOffset = 0.1f;
        private float raystart = 0.5f;
        private float raydistance = 1.2f;
        private Animator animator;
        [SerializeField] private PlayerManager playerManager;
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            float weight;
            if (!playerManager.IsGrounded || playerManager.IsClimbing)
                weight = 0;
            else
                weight = 1f - Mathf.Clamp01(playerManager.HorizontalSpeed / playerManager.Settings.Movement.WalkSpeed);

            float left = PlaceFoot(AvatarIKGoal.LeftFoot, weight);
            float right = PlaceFoot(AvatarIKGoal.RightFoot, weight);

            float lowest = Mathf.Min(left, right, 0f);
            animator.bodyPosition += Vector3.up * lowest * weight;
        }


        private float PlaceFoot(AvatarIKGoal foot, float weight)
        {
            Vector3 footPos = animator.GetIKPosition(foot);
            Vector3 origin = footPos + Vector3.up * raystart;
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raydistance, GroundLayer)) return 0f;
            Vector3 target = hit.point + Vector3.up * footOffset;
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKPositionWeight(foot, weight);
            animator.SetIKRotationWeight(foot, weight);
            animator.SetIKPosition(foot, target);
            animator.SetIKRotation(foot, rotation);

            return target.y - footPos.y;
        }


    }
}

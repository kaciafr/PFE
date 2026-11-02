using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class PlayerCorpse : MonoBehaviour
    {
        [Header("Porté au-dessus de la tête")]
        [SerializeField, Tooltip("Hauteur du corps au-dessus des mains.")]
        private float carryHeight = 0.1f;
        [SerializeField, Tooltip("Décale le corps sur le côté pour le centrer (les hanches ne sont pas au milieu du corps).")]
        private float carrySideShift = 0.15f;
        [SerializeField]
        private float maxThrowSpeed = 5f;
        
        public float MaxThrowSpeed => maxThrowSpeed;



        private Rigidbody[] bones;
        private bool isHeld;
        private CarryArms carrier;

        private void Awake() => bones = GetComponentsInChildren<Rigidbody>();

        public void CopyPose(Transform playerSkeleton)
        {
            Rigidbody[] source = playerSkeleton.GetComponentsInChildren<Rigidbody>();
            isHeld = false;

            for (int i = 0; i < Mathf.Min(source.Length, bones.Length); i++)
            {
                bones[i].isKinematic = false;
                bones[i].transform.SetPositionAndRotation(source[i].transform.position, source[i].transform.rotation);

                bones[i].linearVelocity = Vector3.zero;
                bones[i].angularVelocity = Vector3.zero;

                bones[i].mass = source[i].mass;
                bones[i].solverIterations = 20;
                bones[i].solverVelocityIterations = 10;
                bones[i].linearDamping = 0.5f;
                bones[i].angularDamping = 2f;
                bones[i].maxDepenetrationVelocity = 1f;
            }
        }

        private void LateUpdate()
        {
            Rigidbody hips = bones[0];
            if (!hips.isKinematic)
            {
                carrier = null;
                return;
            }

            if (carrier == null) carrier = hips.GetComponentInParent<CarryArms>();
            if (carrier == null) return;

            Vector3 position = carrier.HandsCenter + Vector3.up * carryHeight - carrier.Right * carrySideShift;
            Quaternion rotation = Quaternion.LookRotation(Vector3.down, carrier.Right);
            hips.transform.SetPositionAndRotation(position, rotation);
        }

        private void FixedUpdate()
        {
            Rigidbody hips = bones[0];
            if (hips.isKinematic == isHeld) return;
            isHeld = hips.isKinematic;

            foreach (Rigidbody bone in bones)
            {
                if (bone == hips) continue;
                bone.isKinematic = isHeld;

                if (!isHeld) bone.linearVelocity = hips.linearVelocity;
            }
        }
    }
}

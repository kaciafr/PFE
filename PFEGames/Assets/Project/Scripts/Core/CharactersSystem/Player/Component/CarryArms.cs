using Goblfin.Player;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Goblfin.CharactersSystem.Player.Component
{
    // Lève les deux mains au-dessus de la tête quand le joueur porte quelque chose (IK des bras) :
    // paumes vers le haut sous l'objet, coudes vers l'extérieur
    public class CarryArms : MonoBehaviour
    {
        [Header("Rigs des bras (RigBrasDroit / RigBrasGauche)")]
        [SerializeField] private Rig rightArmRig;
        [SerializeField] private Rig leftArmRig;

        [Header("Cibles des mains (MainDroiteTarget / MainGaucheTarget)")]
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform head;

        [Header("Position des mains")]
        [SerializeField, Tooltip("Hauteur des mains au-dessus de la tête.")]
        private float aboveHead = 0.2f;
        [SerializeField, Tooltip("Mains un peu devant la tête.")]
        private float forward = 0.15f;
        [SerializeField, Tooltip("Écart de chaque main par rapport au centre (assez grand pour sortir de la tête).")]
        private float handSpread = 0.4f;

        [Header("Orientation")]
        [SerializeField, Range(0f, 1f), Tooltip("0 = doigts vers l'autre main, 1 = doigts vers l'avant.")]
        private float fingersForward = 0.3f;
        [SerializeField, Tooltip("Coudes écartés vers l'extérieur.")]
        private float elbowOut = 0.35f;
        [SerializeField, Tooltip("Coudes un peu vers l'arrière.")]
        private float elbowBack = 0.1f;

        [Header("Transition")]
        [SerializeField, Min(0f), Tooltip("Vitesse à laquelle les bras montent / descendent.")]
        private float weightSpeed = 3f;

        private PlayerManager player;
        private Transform rightHand;
        private Transform leftHand;
        private Transform rightHint;
        private Transform leftHint;

        // Repère "doigts / paume" de chaque main, dans l'espace local de l'os (ne dépend pas de la pose)
        private Quaternion rightHandFrame;
        private Quaternion leftHandFrame;

        // Hauteurs prises au départ : plus stables que la tête qui bouge avec l'animation
        private float headHeight;
        private float shoulderHeight;
        private float blend;

        // Point au-dessus de la tête et direction "droite" du joueur
        public Vector3 CarryCenter { get; private set; }
        public Vector3 Right { get; private set; }

        // Milieu des vraies mains (après l'IK) : le cadavre porté se pose dessus
        public Vector3 HandsCenter => (rightHand.position + leftHand.position) * 0.5f;

        private void Awake()
        {
            player = GetComponent<PlayerManager>();

            // 1. Les os utiles, via l'Animator qui contient la tête
            Animator animator = head.GetComponentInParent<Animator>();
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            headHeight = head.position.y - transform.position.y;
            shoulderHeight = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position.y - transform.position.y;

            // 2. Les coudes (hints) sont rangés dans les contraintes IK des cibles
            rightHint = rightHandTarget.GetComponentInParent<TwoBoneIKConstraint>().data.hint;
            leftHint = leftHandTarget.GetComponentInParent<TwoBoneIKConstraint>().data.hint;

            // 3. Où sont les doigts et la paume dans chaque main
            rightHandFrame = HandFrame(animator, true);
            leftHandFrame = HandFrame(animator, false);
        }

        private void Update()
        {
            // 1. Porter = bras levés, sinon on rend les bras à l'animation (SmoothStep = départ et arrivée en douceur)
            blend = Mathf.MoveTowards(blend, player.IsHolding ? 1f : 0f, weightSpeed * Time.deltaTime);
            float weight = Mathf.SmoothStep(0f, 1f, blend);
            rightArmRig.weight = weight;
            leftArmRig.weight = weight;

            // 2. Le centre au-dessus de la tête, et les directions du joueur
            Vector3 facing = player.Facing;
            Right = Vector3.Cross(Vector3.up, facing);
            CarryCenter = transform.position + Vector3.up * (headHeight + aboveHead) + facing * forward;

            if (weight <= 0f) return;

            // 3. Les mains de chaque côté du centre, paumes vers le haut
            rightHandTarget.SetPositionAndRotation(CarryCenter + Right * handSpread, HandRotation(rightHandFrame, -Right, facing));
            leftHandTarget.SetPositionAndRotation(CarryCenter - Right * handSpread, HandRotation(leftHandFrame, Right, facing));

            // 4. Les coudes à hauteur d'épaule, écartés et un peu en arrière
            Vector3 shoulders = transform.position + Vector3.up * shoulderHeight - facing * elbowBack;
            rightHint.position = shoulders + Right * elbowOut;
            leftHint.position = shoulders - Right * elbowOut;
        }

        // Rotation de la main : doigts vers l'intérieur (un peu vers l'avant), paume vers le haut
        private Quaternion HandRotation(Quaternion handFrame, Vector3 inward, Vector3 facing)
        {
            Vector3 fingers = Vector3.Lerp(inward, facing, fingersForward).normalized;
            return Quaternion.LookRotation(fingers, Vector3.up) * Quaternion.Inverse(handFrame);
        }

        // Repère local de la main, calculé avec les os des doigts :
        // doigts = vers le majeur, paume = perpendiculaire à la ligne index -> auriculaire
        private static Quaternion HandFrame(Animator animator, bool isRight)
        {
            Transform hand = animator.GetBoneTransform(isRight ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
            Vector3 middle = animator.GetBoneTransform(isRight ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal).position;
            Vector3 index = animator.GetBoneTransform(isRight ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal).position;
            Vector3 pinky = animator.GetBoneTransform(isRight ? HumanBodyBones.RightLittleProximal : HumanBodyBones.LeftLittleProximal).position;

            Vector3 fingers = hand.InverseTransformDirection(middle - hand.position);
            Vector3 across = hand.InverseTransformDirection(index - pinky);

            // Main droite et main gauche sont en miroir : on inverse le produit vectoriel
            Vector3 palm = isRight ? Vector3.Cross(fingers, across) : Vector3.Cross(across, fingers);
            return Quaternion.LookRotation(fingers, palm);
        }
    }
}

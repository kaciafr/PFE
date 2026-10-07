using UnityEngine;

namespace Characters
{
    public class PlayerAnimator : MonoBehaviour
    {
        public Animator animator;

        [Header("Correction Grab")]
        [SerializeField, Tooltip("Os des hanches (mixamorig:Hips). Vide = recherché automatiquement.")]
        private Transform hips;
        [SerializeField, Tooltip("Os de la colonne (mixamorig:Spine). Vide = recherché automatiquement.")]
        private Transform spine;
        [SerializeField, Range(0f, 60f), Tooltip("Redresse le buste pendant le grab : l'anim Push penche trop, la tête de Jammo rentre dans la caisse.")]
        private float grabSpineStraighten = 40f;
        [SerializeField, Min(0f), Tooltip("Vitesse d'entrée/sortie de la correction (évite un à-coup).")]
        private float grabCorrectionBlendSpeed = 8f;

        // Les anims Mixamo Push/Pull ne sont pas "In Place" et penchent beaucoup : le modèle rentre dans la caisse
        public bool GrabCorrection { get; set; }
        private float grabCorrectionWeight;
        private Vector3 hipsRestOffset;

        private void Awake()
        {
            if (animator != null)
            {
                if (hips == null)  hips  = FindChild(animator.transform, "mixamorig:Hips");
                if (spine == null) spine = FindChild(animator.transform, "mixamorig:Spine");
            }

            if (hips != null)
                hipsRestOffset = animator.transform.InverseTransformPoint(hips.position);

            if (hips == null || spine == null)
                Debug.LogWarning($"PlayerAnimator : os introuvable (hips={hips}, spine={spine}) : la correction du grab ne marchera pas. Assigne-les dans l'Inspector.", this);
        }

        private void LateUpdate()
        {
            grabCorrectionWeight = Mathf.MoveTowards(grabCorrectionWeight, GrabCorrection ? 1f : 0f,
                                                     grabCorrectionBlendSpeed * Time.deltaTime);
            if (grabCorrectionWeight <= 0f) return;

            // Hanches : on garde la hauteur de l'anim, mais elles restent au-dessus de leur position de repos
            if (hips != null)
            {
                Vector3 rest = animator.transform.TransformPoint(hipsRestOffset);
                Vector3 current = hips.position;
                hips.position = Vector3.Lerp(current, new Vector3(rest.x, current.y, rest.z), grabCorrectionWeight);
            }

            // Buste : pivote vers l'arrière autour de l'axe droit du perso, la tête et les bras reculent avec
            if (spine != null)
            {
                Quaternion back = Quaternion.AngleAxis(-grabSpineStraighten * grabCorrectionWeight, animator.transform.right);
                spine.rotation = back * spine.rotation;
            }
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                if (child.name == childName) return child;
            return null;
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            direction = direction.normalized;
            animator.SetFloat("DirX", direction.x);
            animator.SetFloat("DirY", direction.y);
        }

        public void Play(int stateId)
        {
            animator.CrossFadeInFixedTime(stateId, 0.15f);
        }
        
        public void SetClimbSpeed(float speed) 
        {
            animator.SetFloat(AnimsParams.ClimbSpeed, speed); 
        }

        public void SetGrabSpeed(float speed) => animator.SetFloat(AnimsParams.GrabSpeed, speed);        
        
        
    }
}
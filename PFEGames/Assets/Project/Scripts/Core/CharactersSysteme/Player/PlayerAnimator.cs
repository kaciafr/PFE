using UnityEngine;

namespace Characters
{
    public class PlayerAnimator : MonoBehaviour
    {
        public Animator animator;

        [Header("Correction Grab")]
        [SerializeField, Tooltip("Os de la tête (mixamorig:Head). Vide = recherché automatiquement.")]
        private Transform head;
        [SerializeField, Tooltip("Os de la colonne (mixamorig:Spine). Vide = recherché automatiquement.")]
        private Transform spine;
        [SerializeField, Min(0f), Tooltip("Distance minimale entre l'os de la tête et la face de la caisse (≈ demi-largeur du casque de Jammo).")]
        private float headClearance = 0.4f;
        [SerializeField, Range(0f, 60f), Tooltip("Redresse un peu le buste pendant le grab pour que les mains restent près de la caisse.")]
        private float grabSpineStraighten = 15f;
        [SerializeField, Min(0f), Tooltip("Vitesse d'entrée/sortie de la correction (évite un à-coup).")]
        private float grabCorrectionBlendSpeed = 8f;

        // L'anim Push penche le corps et la grosse tête de Jammo passe dans la caisse :
        // à chaque frame on mesure la tête et on recule le modèle juste assez
        public bool GrabCorrection => grabCrate != null;
        private float grabCorrectionWeight;
        private Transform grabCrate;
        private Vector3 grabFaceLocalPoint;
        private Vector3 grabFaceLocalNormal;
        private Vector3 modelBaseLocalPos;
        private float pushBack;

        private void Awake()
        {
            if (animator != null)
            {
                if (head == null)  head  = FindChild(animator.transform, "mixamorig:Head");
                if (spine == null) spine = FindChild(animator.transform, "mixamorig:Spine");
            }

            if (head == null || spine == null)
                Debug.LogWarning($"PlayerAnimator : os introuvable (head={head}, spine={spine}) : la correction du grab ne marchera pas. Assigne-les dans l'Inspector.", this);
        }

        // Face de la caisse stockée en local : elle suit la caisse quand on la pousse ou la tire
        public void BeginGrabCorrection(Transform crate, Vector3 facePoint, Vector3 faceNormal)
        {
            faceNormal.y = 0f;
            grabCrate = crate;
            grabFaceLocalPoint = crate.InverseTransformPoint(facePoint);
            grabFaceLocalNormal = crate.InverseTransformDirection(faceNormal.normalized);
            modelBaseLocalPos = animator.transform.localPosition;
            pushBack = 0f;
        }

        public void EndGrabCorrection()
        {
            if (grabCrate == null) return;
            grabCrate = null;
            animator.transform.localPosition = modelBaseLocalPos;
        }

        private void LateUpdate()
        {
            grabCorrectionWeight = Mathf.MoveTowards(grabCorrectionWeight, GrabCorrection ? 1f : 0f,
                                                     grabCorrectionBlendSpeed * Time.deltaTime);
            if (!GrabCorrection) return;

            // Buste : pivote vers l'arrière autour de l'axe droit du perso
            if (spine != null)
            {
                Quaternion back = Quaternion.AngleAxis(-grabSpineStraighten * grabCorrectionWeight, animator.transform.right);
                spine.rotation = back * spine.rotation;
            }

            if (head == null) return;

            // On mesure depuis la position de base du modèle, puis on recule de ce qui manque
            Transform model = animator.transform;
            model.localPosition = modelBaseLocalPos;

            Vector3 facePoint = grabCrate.TransformPoint(grabFaceLocalPoint);
            Vector3 faceNormal = grabCrate.TransformDirection(grabFaceLocalNormal);
            float headDistance = Vector3.Dot(head.position - facePoint, faceNormal);

            float target = Mathf.Max(0f, headClearance - headDistance) * grabCorrectionWeight;
            pushBack = Mathf.Lerp(pushBack, target, 15f * Time.deltaTime);

            Vector3 offset = faceNormal * pushBack;
            model.localPosition = modelBaseLocalPos + (model.parent != null ? model.parent.InverseTransformDirection(offset) : offset);
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
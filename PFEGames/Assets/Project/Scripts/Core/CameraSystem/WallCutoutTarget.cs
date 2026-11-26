using UnityEngine;

namespace Goblfin
{
    [ExecuteInEditMode]
    public class WallCutoutTarget : MonoBehaviour
    {
        static readonly int CenterId     = Shader.PropertyToID("_CutoutCenter");
        static readonly int RadiusId     = Shader.PropertyToID("_CutoutRadius");
        static readonly int SmoothnessId = Shader.PropertyToID("_CutoutSmoothness");

        [Tooltip("Taille de la sphère autour du joueur (unités monde).")]
        [SerializeField, Min(0f)] float radius = 1f;

        [Tooltip("Largeur de la transition : plus c'est grand, plus le dither progresse doucement vers la transparence totale.")]
        [SerializeField, Min(0.001f)] float smoothness = 0.5f;

        [Tooltip("Décalage du centre de la sphère par rapport au pivot du joueur.")]
        [SerializeField] Vector3 offset = new(0f, 0.5f, 0f);

        void LateUpdate()
        {
            Shader.SetGlobalVector(CenterId, transform.position + offset);
            Shader.SetGlobalFloat(RadiusId, radius);
            Shader.SetGlobalFloat(SmoothnessId, smoothness);
        }

        void OnDisable() => Shader.SetGlobalFloat(RadiusId, 0f);
    }
}
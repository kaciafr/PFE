using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class VisualPoint : MonoBehaviour
    {
        [SerializeField] private InteractPoint interactPoint;
        
        [Header("Color")]
        [SerializeField] private Material material;
        [SerializeField] private Color baseColor;
        [SerializeField] private Color boycottColor;

        private void Awake()
        {
            material.color = baseColor;
        }

        private void OnEnable()
        {
            interactPoint.IsBoycotted += BoycottVisual;
        }

        private void OnDisable()
        {
            interactPoint.IsBoycotted -= BoycottVisual;
        }

        private void BoycottVisual()
        {
            material.color = boycottColor;
        }
    }
}

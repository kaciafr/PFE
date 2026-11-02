using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Goblfin
{
    [RequireComponent(typeof(Outline))]
    public class UIButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler , ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private Vector2 outlineDistance = new Vector2(3f, -3f);
        
        private Vector3 baseScale;
        private Outline outline;


        void Awake()
        {
             baseScale = transform.localScale;
             outline = GetComponent<Outline>();
             outline.effectColor = outlineColor;
             outline.effectDistance = outlineDistance;
             outline.enabled = false;
        }


        public void OnPointerEnter(PointerEventData eventData) => Grow();

        private void Grow()
        {
            Tween.StopAll(onTarget: transform);
            Tween.Scale(transform, baseScale * hoverScale, duration , Ease.OutBack , useUnscaledTime: true);
            outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData) => Shrink();

        private void Shrink()
        {
            Tween.StopAll(onTarget: transform);
            Tween.Scale(transform, baseScale, duration , Ease.OutQuad , useUnscaledTime: true);
            outline.enabled = false ;
        }

        public void OnSelect(BaseEventData eventData) =>Grow();
       

        public void OnDeselect(BaseEventData eventData)=> Shrink();

        void OnDisable()
        {
            Tween.StopAll(onTarget: transform);
            transform.localScale = baseScale;
            outline.enabled = false;
            
        }
    }
}
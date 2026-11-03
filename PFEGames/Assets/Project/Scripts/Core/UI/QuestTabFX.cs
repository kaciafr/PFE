using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goblfin
{
    public class QuestTabFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private RectTransform visual;      
        [SerializeField] private float slideDistance = 40f;  
        [SerializeField] private float duration = 0.15f;

        private float baseX;

        void Awake()
        {
            baseX = visual.anchoredPosition.x;
        }

        public void OnPointerEnter(PointerEventData eventData) => SlideOut();
        public void OnPointerExit(PointerEventData eventData) => SlideIn();
        public void OnSelect(BaseEventData eventData) => SlideOut();
        public void OnDeselect(BaseEventData eventData) => SlideIn();

        void SlideOut()
        {
            Tween.StopAll(onTarget: visual);
            Tween.UIAnchoredPositionX(visual, baseX + slideDistance, duration, Ease.OutBack, useUnscaledTime: true);
        }

        void SlideIn()
        {
            Tween.StopAll(onTarget: visual);
            Tween.UIAnchoredPositionX(visual, baseX, duration, Ease.OutQuad, useUnscaledTime: true);
        }

        void OnDisable()
        {
            Tween.StopAll(onTarget: visual);
            visual.anchoredPosition = new Vector2(baseX, visual.anchoredPosition.y);
        }
    }
}
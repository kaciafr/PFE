using PrimeTween;
using UnityEngine;

namespace Goblfin
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        [SerializeField] private float fadeInDuration = 0.5f;
        
        protected CanvasGroup canvasGroup;
        public bool isOpen { get; private set; }
        public virtual bool CloseOnEscape => true;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            HideInstant(); 
        }


        public virtual void Show()
        {
            isOpen = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Tween.Alpha(canvasGroup, 1, fadeInDuration , useUnscaledTime:true);
        }

        public virtual void Hide()
        {
            isOpen = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Tween.Alpha(canvasGroup,0 , fadeInDuration , useUnscaledTime:true);

        }
        
        
        public void HideInstant()
        {
            isOpen  = false;
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false ;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
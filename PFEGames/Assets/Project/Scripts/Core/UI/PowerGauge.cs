using Goblfin.Player;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PowerGauge : MonoBehaviour
    {
        [SerializeField] private PlayerManager player;
        [SerializeField] private Image fill;
        [SerializeField] private Gradient colors;               // vert (faible) → rouge (fort)
        [SerializeField] private float heightOffset = 0.8f;     // au-dessus du gobelin (mètres)
        [SerializeField] private Vector2 screenOffset = new Vector2(60f, 0f);   // décalage à côté (pixels)


        private CanvasGroup canvasGroup; 
        private RectTransform rect;
        private Camera cam;
        private bool visible;
        private float lastPercent = -1f;




        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rect = (RectTransform)transform;
            cam = Camera.main;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
        }




        void LateUpdate()
        {
            bool show = player.IsHolding && Time.timeScale > 0f;
            SetVisible(show);
            if (!show) return; 
            
            Vector3 screen = cam.WorldToScreenPoint(player.transform.position  + Vector3.up * heightOffset);
            rect.position = screen +  (Vector3)screenOffset;

            float percent = player.Thrower.PowerPercent;
            if (!Mathf.Approximately(percent, lastPercent))
            {
                lastPercent = percent;
                Tween.UIFillAmount(fill, percent , 0.12f, Ease.OutQuad);
                Tween.PunchScale(transform, Vector3.one * 0.15f,  0.2f);
                fill.color = colors.Evaluate(percent);
            }
        }
        
        
        
        void SetVisible(bool value)
        {
            if (visible == value) return;
            visible = value;
            Tween.StopAll(onTarget: canvasGroup);
            Tween.Alpha(canvasGroup, value ? 1f : 0f, 0.15f);
            if (value) lastPercent = -1f;   
        }
    }
}
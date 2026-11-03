using Goblfin.CharactersSystem.Player.Input;
using Goblfin.Player;
using TMPro;
using UnityEngine;

namespace Goblfin
{
    public class InterractPrompt: MonoBehaviour
    {
        [SerializeField] private PlayerManager player;
        [SerializeField] private InputReader  inputReader;
        [SerializeField] private TextMeshProUGUI txtKey;
        
        [SerializeField] private float heightOffset = 0.35f; 
        [SerializeField] private float bobAmplitude = 6f;
        [SerializeField] private float bobSpeed = 4f;
        [SerializeField] private float fadeDuration = 0.15f;
        
        private CanvasGroup canvasGroup;
        private RectTransform rect;
        private Camera cam;
        private bool visible;


        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rect = (RectTransform)transform;
            cam = Camera.main;
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;  

        }

     /*   void LateUpdate()
        {
            if (Time.timeScale > 0f && FindTarget(out Vector3 worldPos, out string key))
            {
                Vector3 screen = cam.WorldToScreenPoint(worldPos + Vector3.up * heightOffset);

                if (screen.z > 0f)
                {
                    screen.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;   // flotte
                    rect.position = screen;
                    txtKey.text = key;
                    SetVisible(true);
                    return;
                }
            }

            SetVisible(false);
        }

        private bool FindTarget(out Vector3 vector3, out string s)
        {
            throw new System.NotImplementedException();
        }*/
    }
}
using Goblfin.CharactersSystem.Player.Input;
using Goblfin.Player;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goblfin
{


    [RequireComponent(typeof(CanvasGroup))]
    public class InteractPrompt : MonoBehaviour
    {
        [SerializeField] private PlayerManager player;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private TextMeshProUGUI txtKey;

        [Header("Placement")]
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

        void LateUpdate()
        {
            if (Time.timeScale > 0f && FindTarget(out Vector3 worldPos, out string key))
            {
                Vector3 screen = cam.WorldToScreenPoint(worldPos + Vector3.up * heightOffset);

                // z > 0 = devant la caméra
                if (screen.z > 0f)
                {
                    screen.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;   
                    rect.position = screen;
                    txtKey.text = key;
                    SetVisible(true);
                    return;
                }
            }

            SetVisible(false);
        }

        bool FindTarget(out Vector3 pos, out string key)
        {
            PlayerAction actions = inputReader.Actions;
            key = KeyName(actions.Player.Interact);
            pos = Vector3.zero;

            if (player.CanInteract)
            {
                pos = (player.Sensor.InteractableInFront as Component).transform.position;
                return true;
            }

            if (player.CanPickUp)
            {
                pos = player.ThrowableInFront.position;
                return true;
            }

            if (player.LadderInFront && !player.IsClimbing)
            {
                pos = player.Sensor.LadderPoint;
                return true;
            }

            if (player.CanGrab && !player.IsGrabbing)
            {
                pos = player.Sensor.CrateHitPoint;
                key = KeyName(actions.Player.Grab);
                return true;
            }

            return false;
        }

        string KeyName(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                string path = action.bindings[i].effectivePath;
                if (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))
                    return action.GetBindingDisplayString(i);
            }
            return "?";
        }

      
        
      
        
        
        

        void SetVisible(bool value)
        {
            if (visible == value) return;
            visible = value;

            Tween.StopAll(onTarget: canvasGroup);
            Tween.Alpha(canvasGroup, value ? 1f : 0f, fadeDuration);

            if (value)
            {
                Tween.StopAll(onTarget: transform);
                transform.localScale = Vector3.one * 0.6f;
                Tween.Scale(transform, Vector3.one, 0.25f, Ease.OutBack);
            }
        }
    }
}

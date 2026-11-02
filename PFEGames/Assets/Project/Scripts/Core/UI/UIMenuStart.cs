using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Goblfin
{
    public class UIMenuStart : MonoBehaviour
    {

        public Image Logo;
        public TextMeshProUGUI text;
        public Button btnPlay;
        public Button btnSettings;
        public Button btnQuit;
        public Image FadeTransition;

        private Tween Tweenlogo;
        private Tween TweenFloat ;
        private Tween TweenSway ;
        private Tween TweenText;
        public float smoothMultiplier = 1.04f;
        public float floatHeight = 10f;
        public float swayAngle = 2f;

        public float logoUpOffset = 150f;
        public float logoFinalScale = 0.7f;

        private RectTransform logoRect;
        private float startY;
        private Vector3 baseScale;

        private bool started;

        public float hoverScale = 1.1f;
        public Color outlineColor = Color.white;
        public Vector2 outlineDistance = new Vector2(3f, -3f);


        void Start()
        {
            btnPlay.onClick.AddListener(Play);
            btnSettings.onClick.AddListener(OpenSettings);
            btnQuit.onClick.AddListener(Quit);

            FadeTransition.color = new Color(0f, 0f, 0f, 0f);
            FadeTransition.raycastTarget = false;

            logoRect = Logo.rectTransform;
            startY = logoRect.anchoredPosition.y;
            baseScale = Logo.transform.localScale;

            Logo.raycastTarget = false;
            text.raycastTarget = false;

            SetupHover(btnPlay);
            SetupHover(btnSettings);
            SetupHover(btnQuit);

            btnPlay.gameObject.SetActive(false);
            btnSettings.gameObject.SetActive(false);
            btnQuit.gameObject.SetActive(false);

            Tweenlogo = Tween.Scale(Logo.transform, baseScale * smoothMultiplier, 3.0f, cycles: -1, cycleMode: CycleMode.Yoyo, ease: Ease.InOutQuad);

            TweenFloat = Tween.UIAnchoredPositionY(logoRect, startY + floatHeight, 2.6f, cycles: -1, cycleMode: CycleMode.Yoyo, ease: Ease.InOutQuad);

            TweenSway = Tween.LocalRotation(Logo.transform,
                startValue: Quaternion.Euler(0, 0, -swayAngle),
                endValue:   Quaternion.Euler(0, 0, swayAngle),
                duration: 3.4f, cycles: -1, cycleMode: CycleMode.Yoyo, ease: Ease.InOutSine);

            TweenText = Tween.Alpha(text, 0.2f, 0.8f, cycles: -1, cycleMode: CycleMode.Yoyo, ease: Ease.InOutSine);
        }

        void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !started)
            {
                started = true;

                Tweenlogo.Stop();
                TweenFloat.Stop();
                TweenSway.Stop();
                TweenText.Stop();

                Sequence.Create()
                    .Group(Tween.Scale(Logo.transform, baseScale, 0.2f))
                    .Group(Tween.LocalRotation(Logo.transform, Quaternion.identity, 0.2f))
                    .Group(Tween.UIAnchoredPositionY(logoRect, startY, 0.2f))
                    .Group(Tween.Alpha(text, 0f, 0.2f))
                    .ChainCallback(() => ExecuteAnimationLogo());
            }
        }

        void ExecuteAnimationLogo()
        {
            Sequence.Create()
                .Chain(Tween.Scale(Logo.transform, baseScale * 1.15f, 0.12f, Ease.OutQuad))
                .Chain(Tween.Scale(Logo.transform, baseScale, 0.12f, Ease.InQuad))
                .Chain(Tween.UIAnchoredPositionY(logoRect, startY + logoUpOffset, 0.4f, Ease.OutCubic))
                .Group(Tween.Scale(Logo.transform, baseScale * logoFinalScale, 0.4f, Ease.OutCubic))
                .ChainCallback(() => ShowButtons());
        }

        void ShowButtons()
        {
            Button[] boutons = { btnPlay, btnSettings, btnQuit };

            for (int i = 0; i < boutons.Length; i++)
            {
                Transform bouton = boutons[i].transform;
                Vector3 tailleBouton = bouton.localScale;
                bouton.gameObject.SetActive(true);
                bouton.localScale = Vector3.zero;
                Tween.Scale(bouton, tailleBouton, 0.3f, Ease.OutBack, startDelay: i * 0.1f);
            }
        }

        void SetupHover(Button bouton)
        {
            Transform t = bouton.transform;
            Vector3 taille = t.localScale;

            Outline outline = bouton.GetComponent<Outline>();
            if (outline == null) outline = bouton.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDistance;
            outline.enabled = false;

            EventTrigger trigger = bouton.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, () =>
            {
                Tween.StopAll(onTarget: t);
                Tween.Scale(t, taille * hoverScale, 0.15f, Ease.OutBack);
                outline.enabled = true;
            });

            AddEvent(trigger, EventTriggerType.PointerExit, () =>
            {
                Tween.StopAll(onTarget: t);
                Tween.Scale(t, taille, 0.15f, Ease.OutQuad);
                outline.enabled = false;
            });
        }

        void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }


        void Play()
        {
            FadeTransition.raycastTarget = true;

            Tween.Color(FadeTransition, Color.black, 3f)
                .OnComplete(() => SceneManager.LoadScene("KaciScene"));
        }

        void OpenSettings()
        {
            Debug.Log("Options");
        }

        void Quit()
        {
            Debug.Log("Quitter");
            Application.Quit();
        }
        
        
    }
}

using System.Collections;
using Goblfin.DialogueView.Enumeration;
using Goblfin.QuestSystem;
using Goblfin.Scripts.Data;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager instance;
        public static bool IsOpen => instance != null && instance.open;

        [SerializeField] private GameObject PanelUI;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private float speedText = 0.03f;
        [SerializeField] private string refuseText = "Refuser";   // le choix qui "tombe" (gag du GDD)

        private DSDialogueSO currentDialogue;
        private Coroutine typingCoroutine;
        private bool open, isTyping;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            PanelUI.SetActive(false);
        }

        public void StartDialogue(DSDialogueSO startingDialogue, string speakerName)
        {
            if (open || startingDialogue == null) return;

            open = true;
            PanelUI.SetActive(true);
            speakerText.text = speakerName;
            ShowDialogue(startingDialogue);
        }

        public void Continue()
        {
            if (!open) return;

            if (isTyping)
            {
                FinishTyping();
                return;
            }

            if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
            {
                ShowDialogue(currentDialogue.Choices[0].NextDialogue);
            }
        }

        private void ShowDialogue(DSDialogueSO dialogue)
        {
            if (dialogue == null)
            {
                EndDialogue();
                return;
            }

            currentDialogue = dialogue;
            ApplyQuestAction(dialogue);
            ClearChoices();

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(dialogue.Text));
        }

        private void ApplyQuestAction(DSDialogueSO dialogue)
        {
            if (dialogue.Quest == null || QuestManager.instance == null) return;

            switch (dialogue.QuestAction)
            {
                case QuestAction.AcceptQuest:
                    QuestManager.instance.Accept(dialogue.Quest);
                    break;
                case QuestAction.TurnInQuest:
                    QuestManager.instance.TurnInQuest(dialogue.Quest);
                    break;
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            bodyText.text = "";

            foreach (char letter in text)
            {
                bodyText.text += letter;
                yield return new WaitForSeconds(speedText);
            }

            typingCoroutine = null;
            FinishTyping();
        }

        private void FinishTyping()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;

            bodyText.text = currentDialogue.Text;

            if (currentDialogue.DialogueType == DSDialogueType.MultipleChoice)
            {
                CreateChoiceButtons();
            }
        }

        private void CreateChoiceButtons()
        {
            foreach (DSDialogueChoiceData choice in currentDialogue.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.GetComponentInChildren<TMP_Text>().text = choice.Text;

                // Gag du GDD : le choix qui COMMENCE par refuseText ("Non", "Refuser"...) tombe hors de l'écran
                if (choice.Text.Trim().ToLower().StartsWith(refuseText.Trim().ToLower()))
                    button.onClick.AddListener(() => FallOff(button));
                else
                    button.onClick.AddListener(() => ShowDialogue(choice.NextDialogue));
            }
        }

        // Le bouton hésite, puis tombe en tournant
        private void FallOff(Button button)
        {
            button.interactable = false;

            // Le Layout Group ne doit plus le replacer
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null) layout = button.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;

            RectTransform rect = (RectTransform)button.transform;
            Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.LocalRotation(rect, Quaternion.Euler(0f, 0f, -15f), 0.15f, Ease.OutQuad))     // il hésite...
                .Chain(Tween.UIAnchoredPositionY(rect, rect.anchoredPosition.y - 1500f, 0.7f, Ease.InQuad)) // ...et tombe
                .Group(Tween.LocalRotation(rect, Quaternion.Euler(0f, 0f, 120f), 0.7f));                  // en tournant
        }

        private void ClearChoices()
        {
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndDialogue()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            open = false;
            isTyping = false;
            currentDialogue = null;
            ClearChoices();
            PanelUI.SetActive(false);
        }
    }
}

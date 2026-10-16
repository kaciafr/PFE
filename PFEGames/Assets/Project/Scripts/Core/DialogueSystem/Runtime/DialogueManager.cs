using System.Collections;
using Goblfin.DialogueView.Enumeration;
using Goblfin.QuestSystem;
using Goblfin.Scripts.Data;
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
            Debug.Log($"[Dialogue] Continue | open={open} isTyping={isTyping} type={currentDialogue?.DialogueType} choices={currentDialogue?.Choices?.Count} next={(currentDialogue != null && currentDialogue.Choices.Count > 0 ? currentDialogue.Choices[0].NextDialogue : null)}");
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
                button.onClick.AddListener(() => ShowDialogue(choice.NextDialogue));
            }
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

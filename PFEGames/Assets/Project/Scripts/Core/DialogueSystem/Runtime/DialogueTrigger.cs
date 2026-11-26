using System;
using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.QuestSystem;
using JetBrains.Annotations;
using UnityEngine;

namespace Goblfin
{
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string speakerName;
        [SerializeField] private DSDialogueSO startingDialogue;

        [SerializeField] private QuestSO quest;
        [SerializeField] private DSDialogueSO questActiveDialogue;    
        [SerializeField] private DSDialogueSO questReadyDialogue;     
        [SerializeField] private DSDialogueSO questCompletedDialogue;  

        public bool CanInteract => !DialogueManager.IsOpen;
        [SerializeField] private int priority = 30;
        public int Priority => priority;


        [SerializeField] [CanBeNull] private Animator animatorPnj;
        private static readonly int ReactionHash = Animator.StringToHash("Reaction");


        private void Start()
        {
            if (animatorPnj == null) return;
            animatorPnj.Play("Idle");
        }


        public void Interact()
        {
            Debug.Log("Interact");
            DialogueManager.instance.StartDialogue(GetDialogueToStart(), speakerName);
            PlayReaction();
        }

        private DSDialogueSO GetDialogueToStart()
        {
            if (quest == null || QuestManager.instance == null) return startingDialogue;

            DSDialogueSO dialogue = null;

            switch (QuestManager.instance.GetState(quest))
            {
                case QuestState.Inactive:
                    dialogue = startingDialogue;
                    break;
                case QuestState.Active:
                    dialogue = QuestManager.instance.CanTurnIn(quest) ? questReadyDialogue : questActiveDialogue;
                    break;
                case QuestState.Completed:
                    dialogue = questCompletedDialogue;
                    break;
            }

            return dialogue != null ? dialogue : startingDialogue;
        }
        
        
        public void PlayReaction()
        {
            if (animatorPnj == null) return;
            animatorPnj.SetTrigger(ReactionHash);
        }
    }
}

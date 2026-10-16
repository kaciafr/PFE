using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.QuestSystem;
using UnityEngine;

namespace Goblfin
{
    // A mettre sur un PNJ : quand le joueur interagit, lance le dialogue
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string speakerName;
        [SerializeField] private DSDialogueSO startingDialogue;

        [Header("Quete (optionnel)")]
        [SerializeField] private QuestSO quest;
        [SerializeField] private DSDialogueSO questActiveDialogue;     // quete en cours, objets manquants
        [SerializeField] private DSDialogueSO questReadyDialogue;      // objets reunis
        [SerializeField] private DSDialogueSO questCompletedDialogue;  // quete deja rendue

        public bool CanInteract => !DialogueManager.IsOpen;

        public void Interact()
        {
            DialogueManager.instance.StartDialogue(GetDialogueToStart(), speakerName);
        }

        // Choisit quoi dire selon l'etat de la quete
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

            // Si un champ n'est pas rempli, on retombe sur le dialogue de depart
            return dialogue != null ? dialogue : startingDialogue;
        }
    }
}

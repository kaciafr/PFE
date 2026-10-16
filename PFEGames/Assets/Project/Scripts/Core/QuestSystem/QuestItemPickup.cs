using Goblfin.CharactersSystem.Player.Interface;
using UnityEngine;

namespace Goblfin.QuestSystem
{
    public class QuestItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private QuestItemSO item;
        [SerializeField, Min(1)] private int amount = 1;
        [SerializeField] private QuestSO quest;
        public bool CanInteract => item != null
                                   && quest != null
                                   && QuestManager.instance != null
                                   && QuestManager.instance.GetState(quest) == QuestState.Active;

        public void Interact()
        {
            if (QuestManager.instance == null)
            {
                Debug.LogWarning("[Quest] Pas de QuestManager dans la scene.");
                return;
            }

            QuestManager.instance.AddItem(item, amount);
            Destroy(gameObject);
        }
    }
}

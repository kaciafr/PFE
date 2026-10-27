using UnityEngine;

namespace Goblfin.QuestSystem
{
    public class QuestCompleter : MonoBehaviour
    {
        [SerializeField] private QuestSO quest;

        public void Complete()
        {
            QuestManager.instance.CompleteQuest(quest);
        }
    }
}

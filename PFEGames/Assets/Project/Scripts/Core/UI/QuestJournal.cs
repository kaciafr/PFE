using System.Collections.Generic;
using Goblfin.QuestSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class QuestJournal : MonoBehaviour
    {
        [SerializeField] private Transform listParent;        
        [SerializeField] private Button questButtonPrefab;    
        [SerializeField] private GameObject emptyText;        
        [SerializeField] private QuestDetailScreen detailScreen;

        void Awake()
        {
            questButtonPrefab.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            foreach (Transform child in listParent)
            {
                if (child != questButtonPrefab.transform) Destroy(child.gameObject);
            }

            int count = 0;
            foreach (KeyValuePair<QuestSO, QuestState> pair in QuestManager.instance.Quests)
            {
                if (pair.Value == QuestState.Inactive) continue;

                QuestSO quest = pair.Key;
                Button button = Instantiate(questButtonPrefab, listParent);
                button.gameObject.SetActive(true);

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                label.text = pair.Value == QuestState.Completed
                    ? $"<color=#888888>{quest.QuestName} - Missions Accomplies</color>"
                    : quest.QuestName;

                button.onClick.AddListener(() => detailScreen.Open(quest));
                count++;
            }

            emptyText.SetActive(count == 0);
        }
    }
}

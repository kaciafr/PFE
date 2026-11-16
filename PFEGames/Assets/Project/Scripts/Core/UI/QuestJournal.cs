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

        [Header("Détail")]
        [SerializeField] private TextMeshProUGUI detailTitle;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private TextMeshProUGUI detailMissions;

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

            QuestSO first = null;
            int count = 0;
            foreach (KeyValuePair<QuestSO, QuestState> pair in QuestManager.instance.Quests)
            {
                if (pair.Value == QuestState.Inactive) continue;

                QuestSO quest = pair.Key;
                count++;
                if (first == null) first = quest;

                Button button = Instantiate(questButtonPrefab, listParent);
                button.gameObject.SetActive(true);

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                label.text = pair.Value == QuestState.Completed
                    ? $"<color=#888888>Quête {count}</color>"
                    : $"Quête {count}";

                button.onClick.AddListener(() => ShowDetail(quest));
            }

            emptyText.SetActive(count == 0);

            if (first != null) ShowDetail(first);
            else ClearDetail();
        }

        private void ShowDetail(QuestSO quest)
        {
            detailTitle.text = quest.QuestName;
            detailDescription.text = quest.Description;
            detailMissions.text = QuestDetailScreen.BuildMissions(quest);
        }

        private void ClearDetail()
        {
            detailTitle.text = "";
            detailDescription.text = "";
            detailMissions.text = "";
        }
    }
}

using System.Text;
using Goblfin.QuestSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class QuestDetailScreen: UIScreen
    {
        [SerializeField] private TextMeshProUGUI TxtTitle;
        [SerializeField] private TextMeshProUGUI TxtDescription;
        [SerializeField] private TextMeshProUGUI txtMissions;
        [SerializeField] private Button btnClose;

        void Start()
        {
            btnClose.onClick.AddListener(UIManager.Instance.Pop);

          
        }
        
        public void Open(QuestSO quest)
        {
            TxtTitle.text = quest.QuestName;
            TxtDescription.text = quest.Description;
            txtMissions.text = BuildMissions(quest);
            UIManager.Instance.Push(this );
        }

        string BuildMissions(QuestSO quest)
        {
            if (QuestManager.instance.GetState(quest) == QuestState.Completed)
                return "<b>Missions Accomplies</b>";

            StringBuilder text = new StringBuilder();
            foreach (QuestRequirement req in quest.Requirements)
            {
                int count = Mathf.Min(QuestManager.instance.GetItemCount(req.Item), req.Amount);
                string line = $"- {req.Item.itemName} {count}/{req.Amount}";

                if (count >= req.Amount) line = $"<s>{line}</s>";

                text.AppendLine(line);
            }
            return text.ToString();
        }
    }
}
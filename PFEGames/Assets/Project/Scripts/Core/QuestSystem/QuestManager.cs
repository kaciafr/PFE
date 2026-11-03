using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace Goblfin.QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager instance;
        public TMP_Text questTrackerText;
        
        private Dictionary<QuestSO, QuestState> questsStatess = new Dictionary<QuestSO, QuestState>();
        private Dictionary<QuestItemSO, int  > invetory = new Dictionary<QuestItemSO, int>();

        public event Action<QuestSO> OnQuestCompleted;
        public event Action<QuestSO> OnQuestAccepted;
        public event Action<QuestSO,QuestRequirement> OnMissionDone;
        public event Action<QuestSO> OnQuestReady;
        

        public IReadOnlyDictionary<QuestSO, QuestState> Quests => questsStatess;


        void Awake()
        {
            if (instance == null)
            {
                instance = this; 
            }

            RefreshTracker();
        }

        private void RefreshTracker()
        {
            if(questTrackerText== null)return;
            StringBuilder text =  new StringBuilder();
            foreach (KeyValuePair<QuestSO, QuestState> questState in questsStatess)
            {
                if (questState.Value != QuestState.Active )continue;

                QuestSO quest = questState.Key;
                text.AppendLine (quest.QuestName );
                
                foreach (QuestRequirement requirement in quest.Requirements)
                {
                    int count = Mathf.Min(GetItemCount(requirement.Item), requirement.Amount);
                    text.AppendLine($" - {requirement.Item.itemName} {count}/{requirement.Amount}");
                }
            }
            questTrackerText.text = text.ToString();
        }


        public QuestState GetState(QuestSO quest)
        {
            if (questsStatess.TryGetValue(quest, out QuestState state))
            {
                return state;
            }

            return QuestState.Inactive; 
        }

        public int GetItemCount (QuestItemSO  item )
        {
            if (invetory.TryGetValue(item, out int count))
            {
                return count;
            }

            return 0;
        }


        
        public bool CanTurnIn(QuestSO quest)
        {
            if (GetState(quest) != QuestState.Active) return false;

            return quest.Requirements.All(requirement => GetItemCount(requirement.Item) >= requirement.Amount);
        }

        public void Accept(QuestSO quest)
        {
            if (GetState(quest) != QuestState.Inactive) return;
            questsStatess[quest] = QuestState.Active;
            OnQuestAccepted?.Invoke(quest);
            Debug.Log($"[Quest] Acceptée : {quest.QuestName}");
            RefreshTracker();
        }

        public void AddItem(QuestItemSO item, int amount)
        {
            int oldCount = GetItemCount(item);
            invetory[item] = oldCount + amount;
            Debug.Log($"[Quest] +{amount} {item.itemName} (total : {invetory[item]})");
            CheckMission(item, oldCount, invetory[item]);
            RefreshTracker();
            
 
        }

        private void CheckMission(QuestItemSO item, int oldCount, int newCount)
        {
            foreach (KeyValuePair<QuestSO, QuestState> pair in questsStatess)
            {
                if (pair.Value != QuestState.Active) continue;
                foreach (QuestRequirement req in pair.Key.Requirements)
                {
                    if (req.Item != item) continue;
                    if (oldCount < req.Amount && newCount >= req.Amount)
                    {
                        OnMissionDone?.Invoke(pair.Key,req);
                        if (CanTurnIn(pair.Key)) OnQuestReady?.Invoke(pair.Key);
                    } 
                }
            }
        }

        public void TurnInQuest(QuestSO quest)
        {
            if (!CanTurnIn(quest)) return;

            if (quest.RemoveItemsOnTurnIn)
            {
                foreach (QuestRequirement requirement in quest.Requirements)
                {
                    invetory[requirement.Item] -= requirement.Amount;
                }
            }

            CompleteQuest(quest);
        }

        // Termine une quête active, peu importe la façon (objet rendu, zone, puzzle...)
        public void CompleteQuest(QuestSO quest)
        {
            if (GetState(quest) != QuestState.Active) return;

            questsStatess[quest] = QuestState.Completed;
            OnQuestCompleted?.Invoke(quest);
            Debug.Log($"[Quest] Terminée : {quest.QuestName}");
            RefreshTracker();
        }
    }
}
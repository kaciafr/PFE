using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Goblfin.QuestSystem
{
    public class QuestCompletEvent : MonoBehaviour
    {
        [SerializeField] private QuestSO quest;
        [SerializeField] private UnityEvent onCompleted;


        private void Start()
        {
            QuestManager.instance.OnQuestCompleted += HandleCompleted;
        }

        
        private void OnDestroy()
        {
            if (QuestManager.instance != null)
            {
                QuestManager.instance.OnQuestCompleted -= HandleCompleted; 
            }
        }
        
        private void HandleCompleted(QuestSO completedQuest)
        {
            if (completedQuest == quest)
            {
                onCompleted.Invoke();
            }
        }




       

    }
    
    
    
}
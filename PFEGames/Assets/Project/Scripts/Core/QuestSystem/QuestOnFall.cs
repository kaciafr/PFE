using System;
using Goblfin.QuestSystem;
using UnityEngine;

namespace Goblfin
{
    public class QuestOnFall : MonoBehaviour
    {

        [SerializeField] private QuestSO quest;
        [SerializeField] private QuestItemSO fallItem;
        
        [SerializeField]private RagDoll ragDoll;
        private bool counted;

        private void OnEnable() => ragDoll.OnRagdoll += HandleFall;

        
        private void OnDisable() => ragDoll.OnRagdoll -= HandleFall;
        private void  HandleFall()
        {
            
            if (counted) return;
            
            if (QuestManager.instance.GetState(quest) != QuestState.Active) return;
            QuestManager.instance.AddItem(fallItem, 1 );
            counted = true;
        }

        
    }
}

using System;
using System.Collections.Generic;
using Goblfin.QuestSystem;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Goblfin
{
    public class QuestNotification : MonoBehaviour 
    {
        [SerializeField] private TextMeshProUGUI txtMessage;
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float displayDuration = 2.5f;
        
        private CanvasGroup canvasGroup;
        private Queue<string> queue = new Queue<string>();   // messages en attente
        private bool isShowing;


        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        void Start()
        {
            QuestManager.instance.OnQuestAccepted += HandleAccepted;
            QuestManager.instance.OnMissionDone    += HandleMissionDone;
            QuestManager.instance.OnQuestReady     += HandleReady;
            QuestManager.instance.OnQuestCompleted += HandleCompleted;

        }

        private void HandleAccepted(QuestSO quest) => Notify("Une quête a été ajoutée dans mon carnet");
        private void HandleMissionDone(QuestSO quest, QuestRequirement req) => Notify($"Mission accomplie : {req.Item.itemName}");
        
        private void HandleReady(QuestSO quest) => Notify($"Retourne voir la créature ! ({quest.QuestName})");
        
        void HandleCompleted(QuestSO quest) => Notify($"Quête terminée : {quest.QuestName}");

        private void Notify(string message)
        {
            queue.Enqueue(message);
            if (!isShowing) ShowNext();

        }

        private void ShowNext()
        {
            if (queue.Count == 0)
            {
                isShowing = false;
                return;
            }
            
            isShowing = true;
            txtMessage.text = queue.Dequeue();
            
            Sequence.Create()
                .Chain(Tween.Alpha(canvasGroup, 1f, fadeDuration))
                .ChainDelay(displayDuration)
                .Chain(Tween.Alpha(canvasGroup, 0f, fadeDuration))
                .ChainCallback(ShowNext);
        }

        void OnDestroy()
        {
            if (QuestManager.instance == null) return;
            QuestManager.instance.OnQuestAccepted  -= HandleAccepted;
            QuestManager.instance.OnMissionDone    -= HandleMissionDone;
            QuestManager.instance.OnQuestReady     -= HandleReady;
            QuestManager.instance.OnQuestCompleted -= HandleCompleted;
        }
        
    }
}
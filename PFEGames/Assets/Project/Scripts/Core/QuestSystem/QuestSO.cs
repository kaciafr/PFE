using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestRequirement
{
    [field: SerializeField] public QuestItemSO Item { get; private set; }
    [field: SerializeField, Min(1)] public int Amount { get; private set; } = 1;
}

[CreateAssetMenu(menuName = "Quest System/Quest")]
public class QuestSO : ScriptableObject
{
    [field: SerializeField] public string QuestName { get; private set; }
    [field: SerializeField, TextArea(2, 4)] public string Description { get; private set; }
    [field: SerializeField] public List<QuestRequirement> Requirements { get; private set; } = new();
    [field: SerializeField] public bool RemoveItemsOnTurnIn { get; private set; } = true;
}

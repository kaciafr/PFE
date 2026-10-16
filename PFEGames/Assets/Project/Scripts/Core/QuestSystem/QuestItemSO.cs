using UnityEngine;

[CreateAssetMenu(menuName = "Quest System/Quest Item")]
public class QuestItemSO : ScriptableObject
{
    [field: SerializeField] public string itemName { get; private set; }

    [field: SerializeField] public Sprite itemIcon { get; private set; }
}

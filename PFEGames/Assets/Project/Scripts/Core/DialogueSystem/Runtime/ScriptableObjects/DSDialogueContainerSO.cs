using System.Collections.Generic;
using Goblfin.DialogueView.Utilities;
using UnityEngine;

namespace Goblfin
{
    public class DSDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DSDialogueGroupSO , List<DSDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DSDialogueSO> UngroupedDialogue { get; set; }

        public void Initialize(string filename)
        {
            FileName = filename;
            
            DialogueGroups = new SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>>();
            UngroupedDialogue = new List<DSDialogueSO>();
        }
    }
}
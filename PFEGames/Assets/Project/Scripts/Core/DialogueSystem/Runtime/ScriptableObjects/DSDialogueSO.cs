using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using Goblfin.Scripts.Data;
using UnityEngine;

namespace Goblfin
{
    public class DSDialogueSO : ScriptableObject
    {
       [field: SerializeField] public string DialogueName { get; set; }
       [field: SerializeField] public string Text { get; set; }
       [field: SerializeField] public List<DSDialogueChoiceData>Choices { get; set; }
       [field: SerializeField] public DSDialogueType DialogueType { get; set; }
       [field: SerializeField] public bool IsStartingDialogue { get; set; }

       public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType,
           bool isStartingDialogue)
       {
           DialogueName = dialogueName;
           Text = text;
           Choices = choices;
           DialogueType = dialogueType;
           IsStartingDialogue = isStartingDialogue;
       }
    }
}

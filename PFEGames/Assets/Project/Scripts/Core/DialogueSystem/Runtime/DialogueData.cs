using System;
using UnityEngine;

public enum ChoiceAction { None, AcceptQuest, TurnInQuest }

[Serializable]
public class DialogueChoice
{
    public string text;
    public int next = -1;              
    public ChoiceAction action;        
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 5)] public string text;
    public int next = -1;              
    public DialogueChoice[] choices;  
}

[CreateAssetMenu(menuName = "Game/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
#if UNITY_EDITOR
using Goblfin.DialogueView;
using Goblfin.DialogueView.Data.Save;
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Goblfin.Elements
{
    public class DSSingleChoiceNode : DSNode
    {
        public override void Draw()
        {
            base.Draw();

            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);
                
                choicePort.userData = choice;
                
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        public override void Initialize(string nodeName , DSGraphView dsGraphView ,  Vector2 position)
        {
            base.Initialize(nodeName, dsGraphView, position);

            DialogueType = DSDialogueType.SingleChoice;
            DSChoiceSaveData choiceSaveData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue ",
            };
            Choices.Add(choiceSaveData); 
            
            
        }
    }
}
#endif

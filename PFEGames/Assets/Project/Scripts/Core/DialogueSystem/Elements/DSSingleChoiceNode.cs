#if UNITY_EDITOR
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

            foreach (string choice in Choices)
            {
                Port choicePort = this.CreatePort(choice);
                
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            DialogueType = DSDialogueType.SingleChoice;
            
            Choices.Add("Next Dialogue"); 
            
            
        }
    }
}
#endif

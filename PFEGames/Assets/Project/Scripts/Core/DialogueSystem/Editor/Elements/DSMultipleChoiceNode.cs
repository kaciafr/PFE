#if UNITY_EDITOR
using Goblfin.DialogueView;
using Goblfin.DialogueView.Data.Save;
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.Elements
{
    public class DSMultipleChoiceNode :  DSNode
    {
        public override void Initialize(string nodename,  DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodename, dsGraphView, position);
            DialogueType = DSDialogueType.MultipleChoice;
            DSChoiceSaveData choiceSaveData = new DSChoiceSaveData()
            {
                Text = "New Choice"
            };
            Choices.Add(choiceSaveData); 
        }

        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice" , () =>
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice"
                };
                Choices.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                
                outputContainer.Add(choicePort);
                RefreshPorts();
            });
                
            addChoiceButton.AddToClassList("ds-node_button");
            
            mainContainer.Insert(1,addChoiceButton);

            foreach (DSChoiceSaveData choice in Choices)
            {
               Port choicePort = CreateChoicePort(choice);
               
               outputContainer.Add(choicePort);
                    
            }
            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData )
        {
            Port choicePort = this.CreatePort();
            
            choicePort.userData = userData;
            
            DSChoiceSaveData choiceData = (DSChoiceSaveData)userData;

            Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("ds-node_button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.Text, null,evt =>
            {
                choiceData.Text = evt.newValue;
            });
            
            choiceTextField.AddClasses("ds-node_textfield", "ds-node_choice-textfield" ,  "ds-node_textfield_hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}
#endif

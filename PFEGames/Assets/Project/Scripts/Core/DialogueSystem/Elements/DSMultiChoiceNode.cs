#if UNITY_EDITOR
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.Elements
{
    public class DSMultiChoiceNode :  DSNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            DialogueType = DSDialogueType.MultipleChoice;
            Choices.Add("New Choice"); 
        }

        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice" , () =>
            {
                Port choicePort = CreateChoicePort("New Choice");

                Choices.Add("New Choice");

                outputContainer.Add(choicePort);
                RefreshPorts();
            });
                
            addChoiceButton.AddToClassList("ds-node_button");
            
            mainContainer.Insert(1,addChoiceButton);

            foreach (string choice in Choices)
            {
               Port choicePort = CreateChoicePort(choice);
               
               outputContainer.Add(choicePort);
                    
                    
                    
            }
            RefreshExpandedState();
        }

        private Port CreateChoicePort(string choice )
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    GetFirstAncestorOfType<GraphView>()?.DeleteElements(choicePort.connections);
                }

                Choices.RemoveAt(outputContainer.IndexOf(choicePort));
                outputContainer.Remove(choicePort);
                RefreshPorts();
            });

            deleteChoiceButton.AddToClassList("ds-node_button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choice, evt =>
            {
                Choices[outputContainer.IndexOf(choicePort)] = evt.newValue;
            });
            
            choiceTextField.AddClasses("ds-node_textfield", "ds-node_choice-textfield");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}
#endif

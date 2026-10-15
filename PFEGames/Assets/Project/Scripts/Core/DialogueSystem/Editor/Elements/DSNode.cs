#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Goblfin.DialogueView;
using Goblfin.DialogueView.Data.Save;
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.Elements
{

    public class DSNode : Node
    {
        public string ID { get; set; }

        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }
        
        private Color defaultBackgroundColor; 
        protected DSGraphView graphView;

        public virtual void Initialize(string nodeName , DSGraphView dsGraphView,  Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue Text";
            
            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f /255f,29f/255f,30/255f);
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node_main-container");
            extensionContainer.AddToClassList("ds-node_extension-container");
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Port", actionEvent => DisconnectInputPort());
            evt.menu.AppendAction("Disconnect Output Port", actionEvent => DisconnectOutputPort());

            base.BuildContextualMenu(evt);
        }

        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null,callback =>
            {
                TextField target =  (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
                if(string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorAmount;
                    }
                }
                
                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;

                    graphView.AddUngroupedNode(this);

                    return;
                }
                
                DSGroup currentGroup = Group;
                graphView.RemoveGroupedNode(this, Group);

                DialogueName = target.value;

                graphView.AddGroupedNode(this, currentGroup);
            });
            
            
            dialogueNameTextField.AddClasses("ds-node_textfield", "ds-node_filename-textfield","ds-node_textfield_hidden" );
            

            titleContainer.Insert(0, dialogueNameTextField);
            
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal , Direction.Input , Port.Capacity.Multi); 
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddToClassList("ds-node_custom-data-container");

            Foldout dialogueFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null , callback =>
            {
                Text = callback.newValue;
            });
            textTextField.AddClasses("ds-node_textfield", "ds-node_quote-textfield" );

            
            dialogueFoldout.Add(textTextField);
            
            customDataContainer.Add(dialogueFoldout);
            
            extensionContainer.Add(customDataContainer);
            
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color; 
        }
        
        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor; 
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        
        private void DisconnectPorts (VisualElement container )
        {
            foreach (Port port  in container.Children())
            {
                if (!port.connected)
                {
                    continue; 
                }
                graphView.DeleteElements(port.connections);
            }
        }

        private void DisconnectInputPort()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPort()
        {
            DisconnectPorts(outputContainer);
        }
        
        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();
            return !inputPort.connected;
        }
    }
}
#endif

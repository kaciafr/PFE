#if UNITY_EDITOR
using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.Elements
{
    public class DSNode : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }


        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "Dialogue Name";
            Choices = new List<string>();
            Text = "Dialogue Text";
            
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node_main-container");
            extensionContainer.AddToClassList("ds-node_extension-container");
        }

        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, evt => DialogueName = evt.newValue);
            dialogueNameTextField.AddClasses("ds-node_textfield", "ds-node_filename-textfield","ds-node_textfield_hidden" );
            

            titleContainer.Insert(0, dialogueNameTextField);
            
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal , Direction.Input , Port.Capacity.Multi); 
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddToClassList("ds-node_custom-data-container");

            Foldout dialogueFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text, evt => Text = evt.newValue);
            textTextField.AddClasses("ds-node_textfield", "ds-node_quote-textfield" );

            
            dialogueFoldout.Add(textTextField);
            
            customDataContainer.Add(dialogueFoldout);
            
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }
    }
}
#endif

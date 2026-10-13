#if UNITY_EDITOR
using Goblfin.DialogueView.Enumeration;
using Goblfin.DialogueView.Utilities;
using Goblfin.DialogueView.Window;
using Goblfin.Elements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Goblfin.DialogueView
{
    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;

        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            AddStyles();
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initialize(this);
            }

            // Ouvre la fenêtre de recherche avec la touche Espace
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                // La search window donne une position écran : on la ramène dans la fenêtre
                worldMousePosition -= editorWindow.position.position;
            }

            return contentViewContainer.WorldToLocal(worldMousePosition);
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            // Ordre important : SelectionDragger (déplacer les nodes) avant RectangleSelector
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateContextualMenu("Add Node (SingleChoice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateContextualMenu("Add Node (MultiChoice)" , DSDialogueType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());

        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return; 
                }

                if (startPort.node == port.node)
                {
                    return; 
                }

                if (startPort.direction == port.direction)
                {
                    return; 
                }
                 compatiblePorts.Add(port);
            });
            
            return compatiblePorts;
        }
        
        private IManipulator CreateContextualMenu(string actionTitle , DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent=>menuEvent.menu.AppendAction(actionTitle , actionEvent=>AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
            return contextualMenuManipulator;
        }
        
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent=>menuEvent.menu.AppendAction("Add Group", actionEvent=>AddElement(CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
            return contextualMenuManipulator;
        }

        public GraphElement CreateGroup(string title, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = title,
            };
            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "Assets/Project/Scripts/Core/DialogueSystem/DialogueView/DialogueView.uss",
                "Assets/Project/Scripts/Core/DialogueSystem/DialogueView/DSNodeStyle.uss");
        }

        public DSNode CreateNode(DSDialogueType dialogueType, Vector2 position)
        {
            DSNode dsNode;
            switch (dialogueType)
            {
                case DSDialogueType.SingleChoice:
                    dsNode = new DSSingleChoiceNode();
                    break;
                case DSDialogueType.MultipleChoice:
                    dsNode = new DSMultiChoiceNode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogueType), dialogueType, null);
            }
            dsNode.Initialize(position);
            dsNode.Draw();
            return dsNode; 
        } 
    }
    
    
}
#endif

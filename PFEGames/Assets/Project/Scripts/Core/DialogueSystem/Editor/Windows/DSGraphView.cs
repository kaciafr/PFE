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
using Goblfin.DialogueView.Data.Save;

namespace Goblfin.DialogueView
{
    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;
        
        private SerializableDictionary <string , DSNodeErrorData > ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> groups;
        private SerializableDictionary <Group , SerializableDictionary<string , DSNodeErrorData>> groupedNodes;
        private int nameError;

        public int NameErrorAmount
        {
            get
            {
                return nameError;
            } 
            set
            {
                nameError = value;
                if (nameError == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (nameError == 1)
                {
                    editorWindow.DisableSaving();
                }
            } 
        }

        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;
            
            ungroupedNodes =  new SerializableDictionary <string , DSNodeErrorData>();
            groupedNodes = new SerializableDictionary <Group , SerializableDictionary<string , DSNodeErrorData>>();
            groups = new  SerializableDictionary <string , DSGroupErrorData>();
            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            OnElementDeleted();
            OnGroupElementAdded();
            OnGroupElementRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();
            AddStyles();
            
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

       

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
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
                menuEvent=>menuEvent.menu.AppendAction(actionTitle , actionEvent=>AddElement(CreateNode("DialogueName",dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
            return contextualMenuManipulator;
        }
        
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent=>menuEvent.menu.AppendAction("Add Group", actionEvent=>CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))));
            return contextualMenuManipulator;
        }

        public DSGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            DSGroup group = new DSGroup(title, localMousePosition);
            AddGroup(group);
            
            AddElement(group);
            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }
                DSNode node = (DSNode)selectedElement;
                group.AddElement(node);
            }
            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        private void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();
            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();
                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);
                return; 
            }
            List <DSGroup> groupsList = groups[groupName].Groups;
            groupsList.Add(group);
            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);
            if (groupsList.Count == 2)
            {
                ++NameErrorAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
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
                $"{DSStyleUtilities.StylesFolderPath}/DSGraphViewStyle.uss",
                $"{DSStyleUtilities.StylesFolderPath}/DSNodeStyle.uss");
        }

        public DSNode CreateNode(string nodeName , DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true )
        {
           Type nodeType = Type.GetType($"Goblfin.Elements.DS{dialogueType}Node");
           DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            node.Initialize(nodeName ,this , position);

            if (shouldDraw)
            {
                node.Draw();
            }
            AddUngroupedNode(node);

            return node;
        }

        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            List<DSNode> ungroupeNodesList = ungroupedNodes[nodeName].Nodes;
            ungroupeNodesList.Add(node);
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (ungroupedNodes[nodeName].Nodes.Count == 2)
            {
                ++NameErrorAmount;
                ungroupedNodes[nodeName].Nodes[0].SetErrorStyle(errorColor);
            } 
        }

        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            ungroupedNodes[nodeName].Nodes.Remove(node);
            node.ResetStyle();
            if (ungroupedNodes[nodeName].Nodes.Count == 1)
            {
                --NameErrorAmount;
                ungroupedNodes[nodeName].Nodes[0].ResetStyle();
                return;
            }

            if (ungroupedNodes[nodeName].Nodes.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            return contentViewContainer.WorldToLocal(worldMousePosition);
        }

        private void OnElementDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);
                
                List<DSGroup> groupsToDelete = new List<DSGroup>();
               List <DSNode> nodesToDelete = new List<DSNode>();
               List<Edge> edgesToDelete = new List<Edge>();
                foreach (GraphElement element in selection)
                {
                    if (element is DSNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = (Edge)element;
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if (element.GetType() != groupType)
                    {
                        continue;
                    }
                    DSGroup group = (DSGroup)element;
                    groupsToDelete.Add(group);
                }

                foreach (DSGroup group in groupsToDelete)
                {
                    List<DSNode> groupNodes = new List<DSNode>();
                    foreach (GraphElement groupElement  in group.containedElements)
                    {
                        if (!(groupElement is DSNode node))
                        {
                            continue; 
                        }
                        DSNode groupNode = (DSNode)groupElement;
                        groupNodes.Add(groupNode);
                    }
                    group.RemoveElements(groupNodes);
                    RemoveGroup(group);
                    RemoveElement(group);
                }
                DeleteElements(edgesToDelete);

                foreach (DSNode node in nodesToDelete)
                {
                    if (node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }
                    RemoveUngroupedNode(node);
                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }
            };
        }

        private void RemoveGroup(DSGroup group)
        {
            string oldgroupName = group.oldTitle.ToLower(); 
            List<DSGroup> groupsList = groups[oldgroupName].Groups;
            groups[oldgroupName].Groups.Remove(group);
            group.ResetStyle();
            if (groupsList.Count == 1)
            {
                --NameErrorAmount;
                groupsList[0].ResetStyle();
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldgroupName);
            }
        }

        private void OnGroupElementAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup nodeGroup = (DSGroup)group; 
                    DSNode node = (DSNode)element; 
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup );
                }
            };
        }

        private void OnGroupElementRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSNode node = (DSNode)element;
                    RemoveGroupedNode(node , group);
                    AddUngroupedNode(node);
                    
                }
            };
        }

        public void AddGroupedNode(DSNode node , DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();
            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());

            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                groupedNodes[group].Add(nodeName, nodeErrorData);
                return; 
            }
            List<DSNode> groupedNodeList = groupedNodes[group][nodeName].Nodes;
            groupedNodeList.Add(node);
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);
            if (groupedNodeList.Count == 2)
            {
                ++NameErrorAmount;
                groupedNodeList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;
            groupedNodesList.Remove(node);
            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorAmount;
                groupedNodesList[0].ResetStyle();
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        public void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup)group;
                
                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();
                if(string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.oldTitle))
                    {
                        ++NameErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.oldTitle))
                    {
                        --NameErrorAmount;
                    }
                }
                RemoveGroup(dsGroup);
                
                dsGroup.oldTitle = dsGroup.title;
                
                AddGroup(dsGroup);
            }; 
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;
                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }
                        Edge edge = (Edge)element;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;
                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }


        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();
            NameErrorAmount = 0;
        }
    }
    
    
}
#endif

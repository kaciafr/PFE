#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Goblfin.DialogueView.Data.Save;
using Goblfin.Elements;
using Goblfin.Scripts.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Goblfin.DialogueView.Utilities
{
    public static class DSIOUtility
    {
        public const string graphsFolderPath ="Assets/Project/Editor/DialogueSystem/Graphs";
        private const string dialoguesFolderPath = "Assets/Project/Dialogues";

        private static string graphFileName;
        public static string containerFolderPath;
        private static DSGraphView graphView;
        private static List<DSGroup> groups;
        private static List<DSNode> nodes;
        public static Dictionary<string,DSDialogueGroupSO> createdDialogueGroups;
        public static Dictionary<string,DSDialogueSO> createdDialogue;
        
        private static Dictionary<string, DSGroup> loadedGroups;
        private static Dictionary<string, DSNode> loadedNodes;
        public static void Initialize( DSGraphView dsGraphView,string graphName)
        {
            graphView = dsGraphView;
            graphFileName = graphName;
            containerFolderPath = $"{dialoguesFolderPath}/{graphFileName}";
            groups = new List<DSGroup>();
            nodes = new List<DSNode>();
            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogue = new Dictionary<string, DSDialogueSO>();
            loadedGroups = new Dictionary<string, DSGroup>();
            loadedNodes = new Dictionary<string, DSNode>();
        }


        #region Save Methods
        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();
            DSGraphSaveDataSO graphData =  CreateAsset<DSGraphSaveDataSO>(graphsFolderPath , $"{graphFileName}Graph");

            graphData.Initialize(graphFileName);

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);

            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            
            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodesNames = new SerializableDictionary<string, List<string>>();            List<string> ungroupedNodesNames = new List<string>();
            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodesNames.AddItem(node.Group.title, node.DialogueName );
                    continue;
                }
                ungroupedNodesNames.Add(node.DialogueName);
            }

            UpdateDialogueChoicesConnections();
            
            UpdateOldGroupedNodes(groupedNodesNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodesNames, graphData); 
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentgroupedNodesNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentgroupedNodesNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentgroupedNodesNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues",  nodeToRemove );
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentgroupedNodesNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldUnGroupedNodeNames != null && graphData.OldUnGroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUnGroupedNodeNames.Except(currentUngroupedNodeNames).ToList();
                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues",  nodeToRemove );
                }
            }

            graphData.OldUnGroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static void UpdateDialogueChoicesConnections()
        {
            foreach (DSNode node in nodes)
            {
                DSDialogueSO dialogue = createdDialogue[node.ID];
                for (int choicesIndex = 0; choicesIndex < node.Choices.Count; choicesIndex++)
                {
                    DSChoiceSaveData nodechoice = node.Choices[choicesIndex];
                    if (string.IsNullOrEmpty(nodechoice.NodeID))
                    {
                        continue; 
                    }
                    dialogue.Choices[choicesIndex].NextDialogue =  createdDialogue[nodechoice.NodeID];
                    SaveAsset(dialogue);
                }
            }
        }

        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;
            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogue.Add(dialogue);
            }
            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
                );
            createdDialogue.Add(node.ID, dialogue);
            SaveAsset(dialogue);
        }

        private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(
            List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChoiceData> dialogueChoice = new List<DSDialogueChoiceData>();
            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };
                dialogueChoice.Add(choiceData);
            }
            return dialogueChoice;
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoice(node.Choices);
            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices  = choices,
                Text  = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position,
            }; 
            graphData.Nodes.Add(nodeData);
        }

        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();
            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);
                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void UpdateOldGroups(List<string> CurrentgroupNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(CurrentgroupNames).ToList();
                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(CurrentgroupNames);
        }

        private static void RemoveFolder(string fullpath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullpath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullpath}/");

        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;
            
            CreateFolder($"{containerFolderPath}/Groups",  groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");
            
            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);
            
            dialogueGroup.Initialize(groupName);
            createdDialogueGroups.Add(group.ID, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());
            SaveAsset(dialogueGroup);
        }

        private static void SaveAsset( UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); 
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position,
            };
            graphData.Groups.Add(groupData);
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullpath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path , assetName);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullpath);
            }
            return asset;
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DSGroup);
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DSNode node )
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DSGroup group = (DSGroup)graphElement;
                    groups.Add(group);
                    return;
                }
            });
        }
        
        private static T LoadAsset<T>(string path , string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        #endregion

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/Project/Editor" , "DialogueSystem");
            CreateFolder("Assets/Project/Editor/DialogueSystem" , "Graphs");

            CreateFolder("Assets/Project" , "Dialogues");

            CreateFolder(dialoguesFolderPath , graphFileName);
            CreateFolder(containerFolderPath,"Global");
            CreateFolder(containerFolderPath,"Groups");
            CreateFolder($"{containerFolderPath}/Global" , "Dialogues");
        }

      
        #region Creation Methods
        private static void CreateFolder(string path , string foldername)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{foldername}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, foldername);
        }
        #endregion

        public static void Load()
        {
            DSGraphSaveDataSO graphData = LoadAsset<DSGraphSaveDataSO>(graphsFolderPath, graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not Load file", 
                    "The File at the following path could not be found : \n\n" +
                    $"{graphsFolderPath}/{graphFileName}\n\n"+
                    "Make sure you chose the right file  and it's placed at the folder path mentioned above.",
                    "Thanks!"
                    );
                return;
            }
            DSEditorWindow.UpdateFileName(graphData.FileName);
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections(); 
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DSNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;
                    if (string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue; 
                    }
                    DSNode  nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort =  (Port)nextNode.inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                    
                }
            }
        }

        private static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            foreach (DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoice(nodeData.Choices);
                DSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position , false );
                
                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                
                node.Draw();

                graphView.AddElement(node);
                
                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                {
                    continue; 
                }
                
                DSGroup group = loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
                
                
            }
        }

        private static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach (DSGroupSaveData groupData in groups)
            {
                DSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static List<DSChoiceSaveData> CloneNodeChoice(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices =   new List<DSChoiceSaveData>();

            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };
                choices.Add(choiceData);
            }
            return choices ;
        }
        
        
    }
}
#endif

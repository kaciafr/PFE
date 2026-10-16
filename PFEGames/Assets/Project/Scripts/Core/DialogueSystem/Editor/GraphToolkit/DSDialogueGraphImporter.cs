#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goblfin.Scripts.Data;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Goblfin.DialogueView.GraphToolkit
{
    // Remplace la partie "Save to ScriptableObject" de DSIOUtility :
    // a chaque sauvegarde du .dsgraph, Unity reimporte le fichier et genere
    // un DSDialogueContainerSO (asset principal) avec ses DSDialogueSO en sous-assets
    [ScriptedImporter(1, DSDialogueGraph.AssetExtension)]
    internal class DSDialogueGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            DSDialogueGraph graph = GraphDatabase.LoadGraphForImporter<DSDialogueGraph>(ctx.assetPath);

            if (graph == null)
            {
                Debug.LogError($"Failed to load Dialogue graph asset: {ctx.assetPath}");
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);

            DSDialogueContainerSO dialogueContainer = ScriptableObject.CreateInstance<DSDialogueContainerSO>();
            dialogueContainer.name = fileName;
            dialogueContainer.Initialize(fileName);

            List<DSDialogueNode> nodes = graph.GetNodes().OfType<DSDialogueNode>().ToList();
            Dictionary<DSDialogueNode, DSDialogueSO> createdDialogues = new Dictionary<DSDialogueNode, DSDialogueSO>();

            // 1er passage : un DSDialogueSO par node
            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                DSDialogueNode node = nodes[nodeIndex];

                string dialogueName = node.GetDialogueName();
                if (string.IsNullOrEmpty(dialogueName))
                {
                    dialogueName = $"Dialogue{nodeIndex}";
                }

                List<DSDialogueChoiceData> choices = node.GetChoiceTexts()
                    .Select(choiceText => new DSDialogueChoiceData() { Text = choiceText })
                    .ToList();

                DSDialogueSO dialogue = ScriptableObject.CreateInstance<DSDialogueSO>();
                dialogue.name = dialogueName;
                dialogue.Initialize(dialogueName, node.GetText(), choices, node.DialogueType, node.IsStartingNode());
                dialogue.QuestAction = node.GetQuestAction();
                dialogue.Quest = node.GetQuest();

                ctx.AddObjectToAsset(node.ID.ToString(), dialogue);
                dialogueContainer.UngroupedDialogue.Add(dialogue);
                createdDialogues.Add(node, dialogue);
            }

            // 2eme passage : relier chaque choix au dialogue suivant (equivalent de UpdateDialogueChoicesConnections)
            foreach (KeyValuePair<DSDialogueNode, DSDialogueSO> createdDialogue in createdDialogues)
            {
                List<IPort> choicePorts = createdDialogue.Key.GetChoiceOutputPorts();

                for (int choiceIndex = 0; choiceIndex < choicePorts.Count; choiceIndex++)
                {
                    DSDialogueNode nextNode = choicePorts[choiceIndex]?.FirstConnectedPort?.GetNode() as DSDialogueNode;

                    if (nextNode == null)
                    {
                        continue;
                    }

                    createdDialogue.Value.Choices[choiceIndex].NextDialogue = createdDialogues[nextNode];
                }
            }

            ctx.AddObjectToAsset("DialogueContainer", dialogueContainer);
            ctx.SetMainObject(dialogueContainer);
        }
    }
}
#endif

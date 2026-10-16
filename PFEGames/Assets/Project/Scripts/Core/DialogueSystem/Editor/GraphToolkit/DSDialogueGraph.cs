#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Goblfin.DialogueView.GraphToolkit
{
    // Remplace DSEditorWindow + DSGraphView + DSSearchWindow + le Save/Load de DSIOUtility :
    // Graph Toolkit gere la fenetre, la creation de nodes et la sauvegarde du fichier .dsgraph
    [Serializable]
    [Graph(AssetExtension)]
    internal class DSDialogueGraph : Graph
    {
        internal const string AssetExtension = "dsgraph";

        [MenuItem("Assets/Create/Dialogue System/Dialogue Graph")]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DSDialogueGraph>("DialogueGraph");
        }

        // Remplace le systeme d'erreurs de noms (DSErrorData, NameErrorAmount...)
        public override void OnGraphChanged(GraphLogger infos)
        {
            base.OnGraphChanged(infos);

            List<DSDialogueNode> nodes = GetNodes().OfType<DSDialogueNode>().ToList();

            if (nodes.Count == 0)
            {
                infos.LogWarning("Add at least one dialogue node.", this);
                return;
            }

            Dictionary<string, List<DSDialogueNode>> nodesByName = new Dictionary<string, List<DSDialogueNode>>();

            foreach (DSDialogueNode node in nodes)
            {
                string dialogueName = node.GetDialogueName();

                if (string.IsNullOrEmpty(dialogueName))
                {
                    infos.LogError("Dialogue Name cannot be empty.", node);
                    continue;
                }

                if (!nodesByName.ContainsKey(dialogueName))
                {
                    nodesByName.Add(dialogueName, new List<DSDialogueNode>());
                }

                nodesByName[dialogueName].Add(node);
            }

            foreach (KeyValuePair<string, List<DSDialogueNode>> nameNodes in nodesByName)
            {
                if (nameNodes.Value.Count < 2)
                {
                    continue;
                }

                foreach (DSDialogueNode node in nameNodes.Value)
                {
                    infos.LogError($"Dialogue Name \"{nameNodes.Key}\" is used by several nodes.", node);
                }
            }
        }
    }
}
#endif

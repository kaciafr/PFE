using Goblfin;
using UnityEditor;
using UnityEngine;

namespace Projet.Tools
{
    public class SetPnjDialogue : EditorWindow
    {
        const string DialogueTemplatePath = "Assets/Project/Scripts/Core/DialogueSystem/Dialogue/Template/Template_Dialogue.dsgraph";
        const string DialogueQuestTemplatePath = "Assets/Project/Scripts/Core/DialogueSystem/Dialogue/Template/Template_QuestDialogue.dsgraph";

        const string FolderpathDialogue = "Assets/Project/Scripts/Core/DialogueSystem/Dialogue";
        const string FolderpathQuestDialogue = "Assets/Project/Scripts/Core/DialogueSystem/DialogueQuest";
        const string FolderpathQuests = "Assets/Project/Scripts/Core/QuestSystem/Quests";
        const string FolderpathItems = "Assets/Project/Scripts/Core/QuestSystem/ItemsQuest";

        const string StartName = "Start";
        const string ActiveName = "Active";
        const string ReadyName = "Ready";
        const string CompletedName = "Completed";

        private static readonly string[] TemplateNames = { "Dialogue simple", "Dialogue avec quete" };
        private static readonly string[] TemplatePaths = { DialogueTemplatePath, DialogueQuestTemplatePath };
        private static readonly string[] TemplateFolders = { FolderpathDialogue, FolderpathQuestDialogue };

        private string speakerName = "";
        private string graphName = "";
        private int templateIndex;
        private DSDialogueContainerSO existingGraph;

        private string questName = "";
        private string itemName = "";
        private int itemAmount = 1;
        private QuestSO quest;

        [MenuItem("Tools/Goblfin/Setup PNJ (dialogue)")]
        static void Open()
        {
            SetPnjDialogue window = GetWindow<SetPnjDialogue>("Setup PNJ");
            window.FillFromSelection();
        }

        private void OnSelectionChange()
        {
            FillFromSelection();
            Repaint();
        }

        private void FillFromSelection()
        {
            if (Selection.activeGameObject == null) return;

            speakerName = Selection.activeGameObject.name;
            graphName = speakerName + "_Dialogue";
        }

        private void OnGUI()
        {
            GameObject pnj = Selection.activeGameObject;

            EditorGUILayout.LabelField("PNJ selectionne", pnj != null ? pnj.name : "(aucun)");
            speakerName = EditorGUILayout.TextField("Nom affiche en jeu", speakerName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quete (optionnel)", EditorStyles.boldLabel);
            quest = (QuestSO)EditorGUILayout.ObjectField("Quete du PNJ", quest, typeof(QuestSO), false);
            questName = EditorGUILayout.TextField("Nom de la quete", questName);
            itemName = EditorGUILayout.TextField("Objet a ramener", itemName);
            itemAmount = Mathf.Max(1, EditorGUILayout.IntField("Quantite", itemAmount));

            GUI.enabled = !string.IsNullOrWhiteSpace(questName) && !string.IsNullOrWhiteSpace(itemName);
            if (GUILayout.Button("Creer la quete et l'objet"))
                quest = CreateQuest(questName, itemName, itemAmount);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nouveau graph", EditorStyles.boldLabel);
            graphName = EditorGUILayout.TextField("Nom du graph", graphName);
            templateIndex = EditorGUILayout.Popup("Modele", templateIndex, TemplateNames);
            EditorGUILayout.HelpBox(
                $"Fichier cree : {TemplateFolders[templateIndex]}/{graphName}.dsgraph\n" +
                $"Nodes de depart a nommer : {StartName}, {ActiveName}, {ReadyName}, {CompletedName}",
                MessageType.None);

            GUI.enabled = pnj != null && !string.IsNullOrWhiteSpace(graphName);
            if (GUILayout.Button("Creer le graph et preparer le PNJ"))
                SetupPnj(pnj, null);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Graph existant", EditorStyles.boldLabel);
            existingGraph = (DSDialogueContainerSO)EditorGUILayout.ObjectField("Graph", existingGraph, typeof(DSDialogueContainerSO), false);

            GUI.enabled = pnj != null && existingGraph != null;
            if (GUILayout.Button("Brancher ce graph sur le PNJ"))
                SetupPnj(pnj, AssetDatabase.GetAssetPath(existingGraph));
            GUI.enabled = true;
        }

        private void SetupPnj(GameObject pnj, string graphPath)
        {
            int layer = LayerMask.NameToLayer("Interactable ");
            if (layer != -1)
            {
                Undo.RecordObject(pnj, "LayerPNJ");
                pnj.layer = layer;
            }

            if (!pnj.TryGetComponent(out Collider _))
                Undo.AddComponent<CapsuleCollider>(pnj);

            if (!pnj.TryGetComponent(out Rigidbody rb))
                rb = Undo.AddComponent<Rigidbody>(pnj);
            Undo.RecordObject(rb, "Rigidbody PNJ");
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (!pnj.TryGetComponent(out Animator _))
                Undo.AddComponent<Animator>(pnj);

            if (!pnj.TryGetComponent(out DialogueTrigger trigger))
                trigger = Undo.AddComponent<DialogueTrigger>(pnj);

            if (string.IsNullOrEmpty(graphPath))
                graphPath = CreateDialogueGraph(graphName);

            if (graphPath == null) return;

            LinkGraph(trigger, graphPath);
            Debug.Log($"[PNJ] {pnj.name} est pret avec {graphPath}.");
        }

        private string CreateDialogueGraph(string fileName)
        {
            string template = TemplatePaths[templateIndex];
            string folder = TemplateFolders[templateIndex];

            if (AssetDatabase.LoadMainAssetAtPath(template) == null)
            {
                Debug.LogWarning($"[PNJ] Modele introuvable ({template}), modele simple utilise.");
                template = DialogueTemplatePath;
            }

            EnsureFolder(folder);
            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}.dsgraph");

            if (!AssetDatabase.CopyAsset(template, newPath))
            {
                Debug.LogError($"[PNJ] Impossible de copier le modele : {template}");
                return null;
            }

            AssetDatabase.ImportAsset(newPath);
            return newPath;
        }

        private QuestSO CreateQuest(string newQuestName, string newItemName, int amount)
        {
            EnsureFolder(FolderpathItems);
            EnsureFolder(FolderpathQuests);

            QuestItemSO item = CreateInstance<QuestItemSO>();
            AssetDatabase.CreateAsset(item, AssetDatabase.GenerateUniqueAssetPath($"{FolderpathItems}/SO_{newItemName}.asset"));
            SerializedObject itemSo = new SerializedObject(item);
            itemSo.FindProperty("<itemName>k__BackingField").stringValue = newItemName;
            itemSo.ApplyModifiedPropertiesWithoutUndo();

            QuestSO newQuest = CreateInstance<QuestSO>();
            AssetDatabase.CreateAsset(newQuest, AssetDatabase.GenerateUniqueAssetPath($"{FolderpathQuests}/SO_{newQuestName}.asset"));
            SerializedObject questSo = new SerializedObject(newQuest);
            questSo.FindProperty("<QuestName>k__BackingField").stringValue = newQuestName;

            SerializedProperty requirements = questSo.FindProperty("<Requirements>k__BackingField");
            requirements.arraySize = 1;
            SerializedProperty requirement = requirements.GetArrayElementAtIndex(0);
            requirement.FindPropertyRelative("<Item>k__BackingField").objectReferenceValue = item;
            requirement.FindPropertyRelative("<Amount>k__BackingField").intValue = amount;
            questSo.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(newQuest);
            Debug.Log($"[PNJ] Quete \"{newQuestName}\" creee ({amount} x {newItemName}).");
            return newQuest;
        }

        private void LinkGraph(DialogueTrigger trigger, string graphPath)
        {
            DSDialogueSO start = null, active = null, ready = null, completed = null, firstStart = null;
            QuestSO graphQuest = null;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(graphPath))
            {
                if (asset is not DSDialogueSO dialogue) continue;

                if (graphQuest == null && dialogue.Quest != null) graphQuest = dialogue.Quest;

                if (!dialogue.IsStartingDialogue) continue;
                if (firstStart == null) firstStart = dialogue;

                switch (dialogue.DialogueName?.Trim())
                {
                    case StartName: start = dialogue; break;
                    case ActiveName: active = dialogue; break;
                    case ReadyName: ready = dialogue; break;
                    case CompletedName: completed = dialogue; break;
                }
            }

            if (start == null) start = firstStart;
            if (start == null) Debug.LogWarning($"[PNJ] Aucun dialogue de depart dans {graphPath}.");

            SerializedObject so = new SerializedObject(trigger);
            so.FindProperty("speakerName").stringValue = speakerName;
            so.FindProperty("startingDialogue").objectReferenceValue = start;
            so.FindProperty("questActiveDialogue").objectReferenceValue = active;
            so.FindProperty("questReadyDialogue").objectReferenceValue = ready;
            so.FindProperty("questCompletedDialogue").objectReferenceValue = completed;

            QuestSO questToUse = graphQuest != null ? graphQuest : quest;
            if (questToUse != null)
                so.FindProperty("quest").objectReferenceValue = questToUse;

            so.ApplyModifiedProperties();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string parent = System.IO.Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}

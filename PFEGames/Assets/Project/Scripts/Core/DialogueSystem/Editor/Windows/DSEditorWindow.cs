#if UNITY_EDITOR
using System.IO;
using Goblfin.DialogueView.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.DialogueView
{
    public class DSEditorWindow : EditorWindow
    {
        DSGraphView graphView;
        private readonly string defaultFileName = "DialogueFileName";
        private static TextField fileNameTextField;
        private Button saveButton;
        
        [MenuItem("Window/DS/DialogueGraph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("DialogueGraph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddStyles();
            AddToolBar();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            
            fileNameTextField = DSElementUtility.CreateTextField(defaultFileName, "FileName :" , callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            saveButton = DSElementUtility.CreateButton("Save", ()=>Save());
            Button loadButton = DSElementUtility.CreateButton("Load", ()=>Load());
            Button clearButton = DSElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = DSElementUtility.CreateButton("Reset", () => ResetGraph());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.AddStyleSheets($"{DSStyleUtilities.StylesFolderPath}/DSToolbar.uss");
            rootVisualElement.Add(toolbar);
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void Load()
        {
            string filePath =
                EditorUtility.OpenFilePanel("DialogueGraphs", DSIOUtility.graphsFolderPath, "asset");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            Clear();
            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid file name. ", 
                    "Please ensure the file name you've typed in is valid", 
                    "Roger!"
                    );
                return;
            }
            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets($"{DSStyleUtilities.StylesFolderPath}/DSVariables.uss");
        }

        private void AddGraphView()
        {
             graphView = new DSGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }
           private static T LoadAsset<T>(string path , string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

     
        
        
    }
}
#endif

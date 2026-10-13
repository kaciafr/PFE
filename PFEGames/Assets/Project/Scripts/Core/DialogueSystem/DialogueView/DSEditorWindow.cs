#if UNITY_EDITOR
using Goblfin.DialogueView.Utilities;
using UnityEditor;
using UnityEngine.UIElements;

namespace Goblfin.DialogueView
{
    public class DSEditorWindow : EditorWindow
    {
        [MenuItem("Window/DS/DialogueGraph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("DialogueGraph");
        }

        private void CreateGUI()
        {
            DSGraphView graphView = new DSGraphView(this);
            graphView.style.flexGrow = 1;
            rootVisualElement.Add(graphView);

            AddStyles();
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("Assets/Project/Scripts/Core/DialogueSystem/DialogueView/DSVariables.uss");
        }
    }
}
#endif

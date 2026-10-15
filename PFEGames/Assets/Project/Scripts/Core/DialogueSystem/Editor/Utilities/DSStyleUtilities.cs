#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Goblfin.DialogueView.Utilities
{
    public static class DSStyleUtilities
    {
        // Dossier qui contient tous les .uss du Dialogue System
        public const string StylesFolderPath = "Assets/Project/Scripts/Core/DialogueSystem/Editor/Styles";

        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetPaths)
        {
            foreach (string styleSheetPath in styleSheetPaths)
            {
                StyleSheet styleSheet = styleSheetPath.StartsWith("Assets/")
                    ? AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath)
                    : (StyleSheet) EditorGUIUtility.Load(styleSheetPath);

                if (styleSheet == null)
                {
                    Debug.LogError($"DSStyleUtilities : StyleSheet introuvable : {styleSheetPath}");
                    continue;
                }

                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }
}
#endif

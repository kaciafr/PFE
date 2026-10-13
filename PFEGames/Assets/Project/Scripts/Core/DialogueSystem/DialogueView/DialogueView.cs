#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueView : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    public static void ShowExample()
    {
      GetWindow<DialogueView>("DialogueGraph");
    }
}

#endif

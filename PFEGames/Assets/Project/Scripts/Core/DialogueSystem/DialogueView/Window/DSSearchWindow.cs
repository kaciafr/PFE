#if UNITY_EDITOR
using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Goblfin.DialogueView.Window
{
    public class DSSearchWindow : ScriptableObject , ISearchWindowProvider
    {
        private DSGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(DSGraphView dsGraphView)
        {
            graphView = dsGraphView;

            // Icône transparente pour décaler les entrées comme dans un menu
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    level = 2,
                    userData = DSDialogueType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multi Choice", indentationIcon))
                {
                    level = 2,
                    userData = DSDialogueType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Quest Node"), 1),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                },
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (searchTreeEntry.userData)
            {
                case DSDialogueType.SingleChoice:
                {
                    graphView.AddElement(graphView.CreateNode(DSDialogueType.SingleChoice, localMousePosition));
                    return true;
                }
                case DSDialogueType.MultipleChoice:
                {
                    graphView.AddElement(graphView.CreateNode(DSDialogueType.MultipleChoice, localMousePosition));
                    return true;
                }
                case Group _:
                {
                    graphView.AddElement(graphView.CreateGroup("DialogueGroup", localMousePosition));
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}
#endif

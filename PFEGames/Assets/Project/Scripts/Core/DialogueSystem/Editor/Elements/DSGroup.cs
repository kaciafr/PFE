#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Goblfin.Elements
{
    public class DSGroup : Group
    {
        public  string ID { get; set; } 
        private Color defaultBoderColor;
        private float defaultBorderWidth;
        public string oldTitle { get; set; } 

        public DSGroup(string groupTitle , Vector2 position )
        {
            ID = Guid.NewGuid().ToString();
            title = groupTitle;
            oldTitle = groupTitle;
            SetPosition(new Rect(position, Vector2.zero));
            defaultBoderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void  ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultBoderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}
#endif

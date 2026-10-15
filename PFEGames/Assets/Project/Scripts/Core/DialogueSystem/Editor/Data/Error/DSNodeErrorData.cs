#if UNITY_EDITOR
using System.Collections.Generic;
using Goblfin.Elements;

namespace Goblfin
{
    public class DSNodeErrorData
    {
        public DSErrorData ErrorData { get; set; }
        public List<DSNode> Nodes { get; set; }

        public DSNodeErrorData()
        {
            ErrorData = new DSErrorData();
            Nodes = new List<DSNode>();
        }
    }
}
#endif
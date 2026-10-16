#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using Unity.GraphToolkit.Editor;

namespace Goblfin.DialogueView.GraphToolkit
{
    // Equivalent de DSSingleChoiceNode : un seul port de sortie "Next Dialogue"
    [Serializable]
    [UseWithGraph(typeof(DSDialogueGraph))]
    internal class DSSingleChoiceDialogueNode : DSDialogueNode
    {
        public const string OUTPUT_PORT_NAME = "NextDialogue";

        public override DSDialogueType DialogueType => DSDialogueType.SingleChoice;

        public override void OnEnable()
        {
            base.OnEnable();

            Title = "Single Choice";
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            context.AddOutputPort(OUTPUT_PORT_NAME)
                .WithDisplayName("Next Dialogue")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }

        public override List<IPort> GetChoiceOutputPorts()
        {
            return new List<IPort> { GetOutputPortByName(OUTPUT_PORT_NAME) };
        }

        public override List<string> GetChoiceTexts()
        {
            return new List<string> { "Next Dialogue" };
        }
    }
}
#endif

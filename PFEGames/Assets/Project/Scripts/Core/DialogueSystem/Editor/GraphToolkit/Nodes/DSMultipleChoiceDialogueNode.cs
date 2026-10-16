#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using Unity.GraphToolkit.Editor;

namespace Goblfin.DialogueView.GraphToolkit
{
    // Equivalent de DSMultipleChoiceNode : le bouton "Add Choice" est remplace par l'option "Choice Count",
    // chaque choix a un champ texte (port d'entree string) et un port de sortie
    [Serializable]
    [UseWithGraph(typeof(DSDialogueGraph))]
    internal class DSMultipleChoiceDialogueNode : DSDialogueNode
    {
        public const string OPTION_CHOICE_COUNT = "ChoiceCount";
        public const string CHOICE_TEXT_PORT_PREFIX = "ChoiceText";
        public const string CHOICE_OUTPUT_PORT_PREFIX = "Choice";

        const int DefaultChoiceCount = 2;
        const string DefaultChoiceText = "Next Choice";

        public override DSDialogueType DialogueType => DSDialogueType.MultipleChoice;

        public override void OnEnable()
        {
            base.OnEnable();

            Title = "Multiple Choice";
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<int>(OPTION_CHOICE_COUNT)
                .WithDisplayName("Choice Count")
                .WithDefaultValue(DefaultChoiceCount)
                .Delayed()
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            int choiceCount = GetChoiceCount();

            for (int choiceIndex = 0; choiceIndex < choiceCount; choiceIndex++)
            {
                context.AddInputPort<string>(CHOICE_TEXT_PORT_PREFIX + choiceIndex)
                    .WithDisplayName($"Choice {choiceIndex + 1} Text")
                    .Build();

                context.AddOutputPort(CHOICE_OUTPUT_PORT_PREFIX + choiceIndex)
                    .WithDisplayName($"Choice {choiceIndex + 1}")
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }

        public override List<IPort> GetChoiceOutputPorts()
        {
            List<IPort> ports = new List<IPort>();
            int choiceCount = GetChoiceCount();

            for (int choiceIndex = 0; choiceIndex < choiceCount; choiceIndex++)
            {
                ports.Add(GetOutputPortByName(CHOICE_OUTPUT_PORT_PREFIX + choiceIndex));
            }

            return ports;
        }

        public override List<string> GetChoiceTexts()
        {
            List<string> texts = new List<string>();
            int choiceCount = GetChoiceCount();

            for (int choiceIndex = 0; choiceIndex < choiceCount; choiceIndex++)
            {
                IPort textPort = GetInputPortByName(CHOICE_TEXT_PORT_PREFIX + choiceIndex);
                string text = null;
                textPort?.TryGetValue(out text);

                texts.Add(string.IsNullOrEmpty(text) ? DefaultChoiceText : text);
            }

            return texts;
        }

        int GetChoiceCount()
        {
            int choiceCount = DefaultChoiceCount;
            GetNodeOptionByName(OPTION_CHOICE_COUNT)?.TryGetValue(out choiceCount);

            return Math.Max(1, choiceCount);
        }
    }
}
#endif

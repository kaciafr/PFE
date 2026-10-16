#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Goblfin.DialogueView.Enumeration;
using Goblfin.QuestSystem;
using Unity.GraphToolkit.Editor;

namespace Goblfin.DialogueView.GraphToolkit
{
    // Equivalent de DSNode : nom du dialogue, texte et port d'entree communs a tous les nodes
    [Serializable]
    internal abstract class DSDialogueNode : Node
    {
        public const string INPUT_PORT_NAME = "DialogueConnection";
        public const string OPTION_DIALOGUE_NAME = "DialogueName";
        public const string OPTION_TEXT = "Text";
        public const string QUEST_ACTION_PORT_NAME = "QuestAction";
        public const string QUEST_PORT_NAME = "Quest";

        public abstract DSDialogueType DialogueType { get; }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OPTION_DIALOGUE_NAME)
                .WithDisplayName("Dialogue Name")
                .WithDefaultValue("DialogueName")
                .Delayed()
                .Build();

            context.AddOption<string>(OPTION_TEXT)
                .WithDisplayName("Dialogue Text")
                .WithDefaultValue("Dialogue Text")
                .AsTextArea(3, 6)
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(INPUT_PORT_NAME)
                .WithDisplayName("Dialogue Connection")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            // Action de quete declenchee quand ce dialogue s'affiche
            context.AddInputPort<QuestAction>(QUEST_ACTION_PORT_NAME)
                .WithDisplayName("Quest Action")
                .Build();

            context.AddInputPort<QuestSO>(QUEST_PORT_NAME)
                .WithDisplayName("Quest")
                .Build();
        }

        public QuestAction GetQuestAction()
        {
            return GetInputPortValue<QuestAction>(QUEST_ACTION_PORT_NAME);
        }

        public QuestSO GetQuest()
        {
            return GetInputPortValue<QuestSO>(QUEST_PORT_NAME);
        }

        // Les ports de sortie, dans l'ordre des choix
        public abstract List<IPort> GetChoiceOutputPorts();

        // Les textes des choix, dans le meme ordre que GetChoiceOutputPorts()
        public abstract List<string> GetChoiceTexts();

        public string GetDialogueName()
        {
            return GetOptionValue<string>(OPTION_DIALOGUE_NAME);
        }

        public string GetText()
        {
            return GetOptionValue<string>(OPTION_TEXT);
        }

        // Comme DSNode.IsStartingNode() : un node sans connexion en entree est un debut de dialogue
        public bool IsStartingNode()
        {
            IPort inputPort = GetInputPortByName(INPUT_PORT_NAME);
            return inputPort == null || !inputPort.IsConnected;
        }

        protected T GetOptionValue<T>(string optionName)
        {
            T value = default;
            GetNodeOptionByName(optionName)?.TryGetValue(out value);
            return value;
        }

        // Lit la valeur d'un port d'entree : depuis une variable/constante connectee, sinon la valeur ecrite sur le port
        protected T GetInputPortValue<T>(string portName)
        {
            T value = default;
            IPort port = GetInputPortByName(portName);

            if (port == null)
            {
                return value;
            }

            if (port.IsConnected)
            {
                switch (port.FirstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.Variable.TryGetDefaultValue(out value);
                        break;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue(out value);
                        break;
                }

                return value;
            }

            port.TryGetValue(out value);
            return value;
        }
    }
}
#endif

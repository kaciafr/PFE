using Goblfin.CharactersSystem.Player.Input;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goblfin
{
    public class RebindButton : MonoBehaviour
    {
        public static bool IsRebinding { get; private set; }
        private static int lastRebindFrame = -1;
        public static bool BlockEscape => IsRebinding || Time.frameCount == lastRebindFrame;

        [SerializeField] private InputReader inputReader;
        [SerializeField] private RebindEnums rebind = RebindEnums.Jump;   
        [SerializeField] private TextMeshProUGUI txtLabel;               
        [SerializeField] private TextMeshProUGUI txtKey;               

        private string actionName;
        private string partName;

        private InputAction action;
        private int bindingIndex;
        private InputActionRebindingExtensions.RebindingOperation operation;

        void Start()
        {
            ApplyRebindChoice();

            action = inputReader.Actions.asset.FindAction(actionName);
            bindingIndex = FindKeyboardBinding();

            GetComponent<Button>().onClick.AddListener(StartRebind);
            RefreshText();
        }

        void ApplyRebindChoice()
        {
            string label = "";

            switch (rebind)
            {
                case RebindEnums.MoveUp:
                    actionName = "Move"; partName = "up"; label = "Avancer";
                    break;
                case RebindEnums.MoveDown:
                    actionName = "Move"; partName = "down"; label = "Reculer";
                    break;
                case RebindEnums.MoveLeft:
                    actionName = "Move"; partName = "left"; label = "Gauche";
                    break;
                case RebindEnums.MoveRight:
                    actionName = "Move"; partName = "right"; label = "Droite";
                    break;
                case RebindEnums.Jump:
                    actionName = "Jump"; partName = ""; label = "Sauter";
                    break;
                case RebindEnums.Crouch:
                    actionName = "Crouch"; partName = ""; label = "S'accroupir";
                    break;
                case RebindEnums.Sprint:
                    actionName = "Sprint"; partName = ""; label = "Courir";
                    break;
                case RebindEnums.Interact:
                    actionName = "Interact"; partName = ""; label = "Interagir / Grimper";
                    break;
                case RebindEnums.Grab:
                    actionName = "Grab"; partName = ""; label = "Pousser / tirer";
                    break;
                case RebindEnums.Throw:
                    actionName = "ThrowObject"; partName = ""; label = "Lancer";
                    break;
            }

            if (txtLabel != null) txtLabel.text = label;
        }

        int FindKeyboardBinding()
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding b = action.bindings[i];
                if (b.isComposite) continue;
                if (partName != "" && b.name != partName) continue;   // pour Move : la bonne direction
                if (b.path.StartsWith("<Keyboard>") || b.path.StartsWith("<Mouse>")) return i;
            }
            Debug.LogWarning("Pas de touche clavier trouvée pour " + actionName);
            return 0;
        }

        void StartRebind()
        {
            IsRebinding = true;
            txtKey.text = "Appuie sur une touche...";

            bool wasEnabled = action.enabled;
            action.Disable();

            // On retient l'ancienne touche pour pouvoir revenir en arrière si la nouvelle est déjà prise
            string previousOverride = action.bindings[bindingIndex].overridePath;

            operation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op => OnRebindComplete(wasEnabled, previousOverride))
                .OnCancel(op => Finish(wasEnabled, false))
                .Start();
        }

        // La touche est choisie : on vérifie qu'elle n'est pas déjà utilisée
        void OnRebindComplete(bool wasEnabled, string previousOverride)
        {
            if (!IsDuplicate())
            {
                Finish(wasEnabled, true);
                return;
            }

            // Déjà prise → on remet l'ancienne touche
            if (string.IsNullOrEmpty(previousOverride))
                action.RemoveBindingOverride(bindingIndex);
            else
                action.ApplyBindingOverride(bindingIndex, previousOverride);

            Finish(wasEnabled, false);

            // Petit message d'erreur, puis on réaffiche la touche (en temps réel : marche pendant la pause)
            txtKey.text = "Déjà utilisée !";
            Tween.Delay(1f, RefreshText, useUnscaledTime: true);
        }

        // Une autre action (ou une autre direction) a-t-elle déjà cette touche ?
        bool IsDuplicate()
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (InputAction other in action.actionMap.actions)
            {
                for (int i = 0; i < other.bindings.Count; i++)
                {
                    InputBinding b = other.bindings[i];
                    if (b.isComposite || b.id == newBinding.id) continue;       // la "boîte" ZQSD, ou soi-même
                    if (b.effectivePath != newBinding.effectivePath) continue;  // touche différente

                    // Interact / Grab / Climb partagent E dans le GDD → autorisé entre eux
                    if (SharedKey(action.name) && SharedKey(other.name)) continue;

                    return true;
                }
            }
            return false;
        }

        static bool SharedKey(string actionName) =>
            actionName == "Interact" || actionName == "Grab" || actionName == "Climb";

        void Finish(bool wasEnabled, bool save)
        {
            operation.Dispose();
            if (wasEnabled) action.Enable();


            if (save)
                PlayerPrefs.SetString("rebinds", action.actionMap.asset.SaveBindingOverridesAsJson());

            IsRebinding = false;
            lastRebindFrame = Time.frameCount;
            RefreshText();   // afficher la nouvelle touche
        }


        public void RefreshText()
        {
            txtKey.text = action.GetBindingDisplayString(bindingIndex);
        }
    }
}

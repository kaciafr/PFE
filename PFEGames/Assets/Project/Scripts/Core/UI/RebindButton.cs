using Goblfin.CharactersSystem.Player.Input;
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
        [SerializeField] private string actionName = "Jump";   
        [SerializeField] private string partName = "";       
        [SerializeField] private TextMeshProUGUI txtKey;      

        private InputAction action;
        private int bindingIndex;
        private InputActionRebindingExtensions.RebindingOperation operation;

        void Start()
        {
            action = inputReader.Actions.asset.FindAction(actionName);
            bindingIndex = FindKeyboardBinding();

            GetComponent<Button>().onClick.AddListener(StartRebind);
            RefreshText();
        }

        int FindKeyboardBinding()
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding b = action.bindings[i];
                if (b.isComposite) continue;                                       
                if (partName != "" && b.name != partName) continue;               
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

            operation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")  
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")  
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op => Finish(wasEnabled, true))
                .OnCancel(op => Finish(wasEnabled, false))
                .Start();
        }

        void Finish(bool wasEnabled, bool save)
        {
            operation.Dispose();  
            if (wasEnabled) action.Enable();

           
            if (save)
                PlayerPrefs.SetString("rebinds", action.actionMap.asset.SaveBindingOverridesAsJson());

            IsRebinding = false;
            lastRebindFrame = Time.frameCount;  
            RefreshText();
        }

        public void RefreshText()
        {
            txtKey.text = action.GetBindingDisplayString(bindingIndex);
        }
    }
}

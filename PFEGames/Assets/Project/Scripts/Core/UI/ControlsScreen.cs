using Goblfin.CharactersSystem.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goblfin
{
    public class ControlsScreen : UIScreen
    {
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnReset;
        [SerializeField] private InputReader inputReader;
        

        void Start()
        {
            btnBack.onClick.AddListener(UIManager.Instance.Pop);
            btnReset.onClick.AddListener(ResetBindings);

        }

        void ResetBindings()
        {
            inputReader.Actions.asset.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey("rebinds");

            foreach (RebindButton button in GetComponentsInChildren<RebindButton>())
                button.RefreshText();
        }
    }
}
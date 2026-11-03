using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class ControlsScreen : UIScreen
    {
        [SerializeField] private Button btnBack;

        void Start()
        {
            btnBack.onClick.AddListener(UIManager.Instance.Pop);
        }
    }
}
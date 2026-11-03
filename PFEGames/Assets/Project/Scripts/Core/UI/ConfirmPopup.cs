using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class ConfirmPopup : UIScreen
    {
        [SerializeField] private TextMeshProUGUI txtMessage;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Button btnCancel;

        private System.Action onConfirm;

        void Start()
        {
            btnCancel.onClick.AddListener(UIManager.Instance.Pop);
            btnConfirm.onClick.AddListener(Confirm);
        }

        public void Open(string message, System.Action action)
        {
            txtMessage.text = message;
            onConfirm = action;
            UIManager.Instance.Push(this);
        }

        void Confirm()
        {
            UIManager.Instance.Pop();   
            onConfirm?.Invoke();        
        }
    }
}
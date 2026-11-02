using UnityEngine;
using UnityEngine.UI;
namespace Goblfin
{
    public class PauseScreen : UIScreen
    {
        [SerializeField] private Button btnResume;
        [SerializeField] private Button btnQuit;
        void Start()
        {
            btnResume.onClick.AddListener(UIManager.Instance.Pop);
            btnQuit.onClick.AddListener(QuitGame);
        }

        void QuitGame()
        {
            Application.Quit();
        }
    }
}

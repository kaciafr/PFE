using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Goblfin
{
    public class PauseScreen : UIScreen
    {
        [SerializeField] private Button btnResume;
        [SerializeField] private Button btnOptions;
        [SerializeField] private Button btnQuit;
        [SerializeField] private UIScreen optionsScreen;
        [SerializeField] private ConfirmPopup confirmPopup;
        [SerializeField] private GameScene menuScene = GameScene.MenuScene;
        [SerializeField] private QuestJournal questJournal;

        void Start()
        {
            btnResume.onClick.AddListener(UIManager.Instance.Pop);
            
            btnOptions.onClick.AddListener(() => UIManager.Instance.Push(optionsScreen));
            
            
            btnQuit.onClick.AddListener(QuitGame);
        }

        void QuitGame()
        {
            confirmPopup.Open("Quitter la partir ?" , () =>
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(menuScene.ToString());
                }
                );
        }
        
        // À chaque ouverture de la pause, le carnet se met à jour
        public override void Show()
        {
            base.Show();
            questJournal.Refresh();
        }

    }
}

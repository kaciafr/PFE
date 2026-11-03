using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Goblfin
{
    public class VictoryScreen:  UIScreen
    {
        
        [SerializeField] private Button btnMenu;
        [SerializeField] private Transform title;   
        [SerializeField] private GameScene menuScene = GameScene.MenuScene ;


        public override bool CloseOnEscape => false;

        void Start()
        {
            btnMenu.onClick.AddListener(GoToMenu);
        }


        void Update()
        {
            if(!isOpen)return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                GoToMenu();
        }



        public void Open()
        {
            UIManager.Instance.Push(this);
        }

        public override void Show()
        {
            base.Show();
            title.localScale = Vector3.zero; 
            Tween.Scale(title, Vector3.one, 0.6f , Ease.OutBack, useUnscaledTime:true);
        }
        
        
        
        private void GoToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuScene.ToString());
        }
    }
}
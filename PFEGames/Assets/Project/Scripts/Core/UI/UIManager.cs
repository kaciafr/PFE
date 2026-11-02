using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goblfin
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private UIScreen pauseScreen;

        private Stack<UIScreen> stack = new Stack<UIScreen>();

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (stack.Count > 0) Pop();  
                else Push(pauseScreen);    
            }
        }

        public void Push(UIScreen screen)
        {
            if (stack.Count > 0) stack.Peek().Hide();

            stack.Push(screen);
            screen.Show();

            UpdatePause();
        }

        public void Pop()
        {
            if (stack.Count == 0) return;

            stack.Pop().Hide();

            if (stack.Count > 0) stack.Peek().Show();

            UpdatePause();
        }

        void UpdatePause()
        {
            bool menuOuvert = stack.Count > 0;

            
            Time.timeScale = menuOuvert ? 0f : 1f;

            Cursor.visible = menuOuvert;
            Cursor.lockState = menuOuvert ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}

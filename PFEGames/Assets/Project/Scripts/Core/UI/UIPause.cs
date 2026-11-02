using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Goblfin
{
    public class UIPause : MonoBehaviour
    {
        public Image panelPause;

        private bool isPaused = false;

        void Start()
        {
           HidePause();
        }

        void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("Echap !");
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                HidePause();
            }
            else
            {
                ShowPause();
            }
        }

        private void ShowPause()
        {
            isPaused = true;
            panelPause.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }

        public void HidePause()
        {
            isPaused = false;
            panelPause.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

    }
}

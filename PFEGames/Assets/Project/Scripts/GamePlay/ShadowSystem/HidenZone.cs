using Goblfin.Gameplay.Gameplay.Shader;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenZone : MonoBehaviour
    {
        private bool playerHiden = false;
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                playerHiden = true;
                ShaderManager.Instance.Hide(playerHiden);
                VisibilityManager.Instance.HideInTheShadow(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                playerHiden = false;
                VisibilityManager.Instance.HideInTheShadow(false);
                ShaderManager.Instance.Hide(playerHiden);
            }
        }
    
    }
}

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
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                playerHiden = false;
                ShaderManager.Instance.Hide(playerHiden);
            }
        }
    
    }
}

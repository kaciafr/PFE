using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenZone : MonoBehaviour
    {
        private NoiseRadius noiseRadius;
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                HidenManager.Instance.HideInTheShadow(true);
            }
            
            if (other.gameObject.GetComponent<NoiseRadius>() == null) return;
            noiseRadius = other.gameObject.GetComponent<NoiseRadius>();
            Debug.Log(noiseRadius.gameObject.name);
            if (noiseRadius.GetComponentInParent<CharacterController>()!=null)
            {
                HidenManager.Instance.HideSpoted(true);
            }
            else
            {
                HidenManager.Instance.HideSpoted(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                HidenManager.Instance.HideInTheShadow(false);
            }
            if (other.gameObject.GetComponent<NoiseRadius>()) 
                noiseRadius = null;
        }
    
    }
}

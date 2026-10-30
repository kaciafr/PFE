using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenZone : MonoBehaviour
    {
        private bool isPlayerHidden = false;
        private PlayerManager player;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerManager>())
            {
                player = other.GetComponent<PlayerManager>();
                isPlayerHidden = true;
                HidenManager.Instance.HideInTheShadow(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            
            if (isPlayerHidden && other.GetComponent<PlayerManager>())
            {
                if (player.isMakingNoise) 
                {
                    Debug.Log("Joueur repéré/fait du bruit -> Alerte Max");
                    HidenManager.Instance.HideSpoted(true);
                    return;
                }
            }

            if (other.GetComponent<MakeSound>() != null )
            {
                Debug.Log("Distraction sonore détectée -> Alerte Moyenne" + other.name);
                HidenManager.Instance.HideSpoted(false); 
            }
        }

        private void OnTriggerExit(Collider other)
        {
            
            if (other.GetComponent<PlayerManager>())
            {
                isPlayerHidden = false;
                HidenManager.Instance.HideInTheShadow(false);
                HidenManager.Instance.HideSpoted(false);
            }
        }
    }
}
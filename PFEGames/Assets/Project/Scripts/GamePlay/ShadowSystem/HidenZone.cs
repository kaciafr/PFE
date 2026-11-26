using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenZone : MonoBehaviour
    {
        private int distractions; 

        private void OnTriggerEnter(Collider other)
        {
            PlayerManager player = other.GetComponentInParent<PlayerManager>();
            if (player!=null)
            {
                
                Debug.Log(this.gameObject.name + ": hit " + other.gameObject.name);
                HidenManager.Instance.EnterZone(this, player);
            }
        }
        

        private void OnTriggerExit(Collider other)
        {
            PlayerManager player = other.GetComponentInParent<PlayerManager>();
            if (player != null)
            {
                Debug.Log("yoo");
                HidenManager.Instance.ExitZone(this);
            }
        }
        
       /* private void OnDisable()
        {
            if (HidenManager.Instance == null) return;

            HidenManager.Instance.ExitZone(this);
            for (; distractions > 0; distractions--)
                HidenManager.Instance.DistractionEnded();
            
            if (distractions > 0)
            {
                distractions = 0;
                HidenManager.Instance.DistractionEnded();
            }
        }*/
    }
}
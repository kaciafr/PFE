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
            if (other.TryGetComponent(out PlayerManager player))
            {
                HidenManager.Instance.EnterZone(this, player);
            }
            else if (other.TryGetComponent(out MakeSound _))
            {
                distractions++;
                HidenManager.Instance.DistractionStarted();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerManager _))
            {
                HidenManager.Instance.ExitZone(this);
            }
            else if (other.TryGetComponent(out MakeSound _) && distractions > 0)
            {
                distractions--;
                HidenManager.Instance.DistractionEnded();
            }
        }
        
        private void OnDisable()
        {
            if (HidenManager.Instance == null) return;

            HidenManager.Instance.ExitZone(this);
            for (; distractions > 0; distractions--)
                HidenManager.Instance.DistractionEnded();
        }
    }
}
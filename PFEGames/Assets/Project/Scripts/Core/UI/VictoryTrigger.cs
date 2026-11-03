using Goblfin.Player;
using UnityEngine;

namespace Goblfin
{
    public class VictoryTrigger : MonoBehaviour
    {
        [SerializeField] private VictoryScreen victoryScreen;
        private bool done;

        void OnTriggerEnter(Collider other)
        {
            if (done) return;
            
            if (other.GetComponentInParent<PlayerManager>() == null) return;
            
            done = true;
            victoryScreen.Open();
            
            
        }
    }
}
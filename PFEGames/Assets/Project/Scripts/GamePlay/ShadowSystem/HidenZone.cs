using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenZone : OverlapTrigger<PlayerManager>
    {
        private int distractions;

        protected override void OnZoneEnter(PlayerManager target)
        {
            base.OnZoneEnter(target);
            HidenManager.Instance.EnterZone(target);
        }

        protected override void OnZoneExit(PlayerManager target)
        {
            HidenManager.Instance.ExitZone(target);
        }

        /*private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerManager>();
            if (player!=null)
            {
                Debug.Log(this.gameObject.name + ": hit " + other.gameObject.name);
                HidenManager.Instance.EnterZone(player);
            }
        }
        

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<PlayerManager>();
            if (player!=null)
            {
                Debug.Log("yoo");
                HidenManager.Instance.ExitZone(player);
            }
        }*/
    }
}
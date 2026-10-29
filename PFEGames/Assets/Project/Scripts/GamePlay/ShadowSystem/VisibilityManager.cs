using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class VisibilityManager : MonoBehaviour
    {
        public static VisibilityManager Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void HideInTheShadow(bool playerHiden)
        {
            if (playerHiden)
            {
                foreach (var pnj in PnjRegistry.All)
                {
                    var vision = pnj.GetAptitude<VisionCone>();
                    vision.VisionNerf();
                } 
            }
            else
            {
                foreach (var pnj in PnjRegistry.All)
                {
                    var vision = pnj.GetAptitude<VisionCone>();
                    vision.VisionNormal();
                } 
            }
            
        }
    }
}
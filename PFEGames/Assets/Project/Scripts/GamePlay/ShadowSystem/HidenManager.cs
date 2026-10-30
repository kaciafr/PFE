using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenManager : MonoBehaviour
    {
        public static HidenManager Instance;
        private bool isQuiet;
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
            ShaderManager.Instance.Hide(playerHiden);
            if (playerHiden && isQuiet)
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

        public void HideSpoted(bool playerNoise)
        {
            isQuiet = false;
            if (playerNoise)
            {
                foreach (var pnj in PnjRegistry.All)
                {
                    var vision = pnj.GetAptitude<VisionCone>();
                    vision.PersonalVisionValue(GameMetrix.IsPlayerNoise);
                }
            }
            else
            {
                foreach (var pnj in PnjRegistry.All)
                {
                    var vision = pnj.GetAptitude<VisionCone>();
                    vision.PersonalVisionValue(GameMetrix.IsNotPlayerNoise);
                }
            }
        }
    }
}
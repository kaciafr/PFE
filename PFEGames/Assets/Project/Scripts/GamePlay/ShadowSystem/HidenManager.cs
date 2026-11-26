using System.Collections.Generic;
using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenManager : MonoBehaviour
    {
        public static HidenManager Instance { get; private set; }

        private PlayerManager player;
        private bool inShadow;

       
        private bool lastInShadow, lastNoisy, lastHidden;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }



        public void EnterZone(PlayerManager p)
        {
            player = p;
            inShadow = true;
            Refresh();
            ShaderManager.Instance.Hide(true);
        }

        public void ExitZone(PlayerManager p)
        {
            player = null;
            inShadow = false;
            Refresh();
            Debug.Log("Shadow exit");
            ShaderManager.Instance.Hide(false);
        }

        private void Refresh()
        {
            bool noisy = inShadow && player != null && player.isMakingNoise;
            bool hidden = inShadow && !noisy;

            if (inShadow == lastInShadow && noisy == lastNoisy && hidden == lastHidden) return;
            lastInShadow = inShadow;
            lastNoisy = noisy;
            lastHidden = hidden;

            float personal = noisy ? GameMetrix.IsPlayerNoise : GameMetrix.IsNotPlayerNoise;

            IReadOnlyList<BrainPnj> all = PnjRegistry.All;
            for (int i = 0; i < all.Count; i++)
            {
                VisionCone vision = all[i].GetAptitude<VisionCone>();
                if (vision == null) continue;

                if (!inShadow)       vision.VisionNormal();                                   
                else if (noisy)      vision.PersonalVisionValue(personal);  
                else if (hidden)     vision.VisionNerf();
                else                 vision.PersonalVisionValue(personal);
            }
        }
    }
}
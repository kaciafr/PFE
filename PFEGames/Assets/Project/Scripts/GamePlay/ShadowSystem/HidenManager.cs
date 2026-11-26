using System.Collections.Generic;
using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class HidenManager : MonoBehaviour
    {
        public static HidenManager Instance { get; private set; }

        private readonly HashSet<HidenZone> zones = new HashSet<HidenZone>();
        private PlayerManager player;
        private int distractions;

       
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

        private void Update()
        {
            
            if (zones.Count > 0) Refresh();
        }

        public void EnterZone(HidenZone zone, PlayerManager p)
        {
            player = p;
            
            zones.Add(zone); 
            Refresh();
            ShaderManager.Instance.Hide(true);
        }

        public void ExitZone(HidenZone zone)
        {
            zones.Remove(zone); 
            ShaderManager.Instance.Hide(zones.Count > 0);
            Refresh();

        }

        public void DistractionStarted()
        {
            distractions++; 
            Refresh();
        }

        public void DistractionEnded()
        {
            distractions = Mathf.Max(0, distractions - 1); 
            Refresh();
        }

        private void Refresh()
        {
            bool inShadow = zones.Count > 0;                      
            bool noisy = inShadow && player != null && player.isMakingNoise;
            bool hidden = inShadow && !noisy && distractions == 0;

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
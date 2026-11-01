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
        }

        public void ExitZone(HidenZone zone)
        {
            zones.Remove(zone); 
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

            if (ShaderManager.Instance != null)
                ShaderManager.Instance.Hide(inShadow);

            float personal = noisy ? GameMetrix.IsPlayerNoise : GameMetrix.IsNotPlayerNoise;

            IReadOnlyList<BrainPnj> all = PnjRegistry.All;
            for (int i = 0; i < all.Count; i++)
            {
                VisionCone vision = all[i].GetAptitude<VisionCone>();
                if (vision == null) continue;

                if (!inShadow)       vision.VisionNormal();                                    // hors de l'ombre
                else if (noisy)      vision.PersonalVisionValue(personal);     // le joueur fait du bruit : alerte max
                else if (hidden)     vision.VisionNerf();                                      // caché et silencieux
                else                 vision.PersonalVisionValue(personal);  // une distraction sonore est active
            }
        }
    }
}
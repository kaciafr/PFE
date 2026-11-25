using System;
using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.Player;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    
    public class InteractPoint : OverlapTrigger<BrainPnj>, IInteractable
    {
        [Header("Point Stop")]
        
        [Header("bool")]
        [SerializeField] private bool playerCanInteract;
        [field: SerializeField] public bool ItBoycott { get; set; }
        
        [Header("AnimInfo")]
        [SerializeField] private string animTriggerName;

        private BrainPnj pnj;
        [SerializeField] private KillZone killZone;
        public event Action IsBoycotted;
        

        public bool CanInteract => playerCanInteract;
        

        private void OnEnable()
        {
            killZone.OnDeath += Boycott;
        }

        private void OnDisable()
        {
            killZone.OnDeath -= Boycott;
        }

        private void Boycott()
        {
            Debug.Log("Boycott");
            ItBoycott = true;
            IsBoycotted?.Invoke();
        }

        protected override void OnZoneEnter(BrainPnj target)
        {
            base.OnZoneEnter(target);
            
            pnj = target;
        }

        protected override void OnZoneExit(BrainPnj target)
        {
            pnj = null;
        }

        public void Interact()
        {
            if(!ItBoycott) return;
            
            pnj.PnjGoTo(new KoState());
        }
    }
}

using System;
using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.Player;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class PointStop : OverlapTrigger<IBoycott>, IInteractable
    {
        [Header("Point Stop")]
        
        [Header("bool")]
        [SerializeField] private bool playerCanInteract;
        [field: SerializeField] public bool ItBoycott { get; set; }
        
        [Header("AnimInfo")]
        [SerializeField] private string animTriggerName;
        
        public event Action IsBoycotted;
        public event Action NotBoycottedInteract;
        public event Action BoycottedInteract;

        public bool CanInteract => playerCanInteract;

        protected override void OnZoneEnter(IBoycott target)
        {
            base.OnZoneEnter(target);
            IsBoycotted?.Invoke();
            ItBoycott =  true;
        }

        public void Interact()
        {
            if(ItBoycott)
                BoycottedInteract?.Invoke();
            else
                NotBoycottedInteract?.Invoke();
        }
    }
}

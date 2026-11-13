using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin
{
    public class DoorPnjDetector : OverlapTrigger<BrainPnj>
    {
        [SerializeField] private DoorInteract door;
        
        private bool IsBlocked => door.DoorState is DoorBlockedState;
        private void Reset() => door = GetComponentInParent<DoorInteract>();

        protected override void OnZoneEnter(BrainPnj pnj)
        {
            if (IsBlocked) 
                pnj.PnjGoTo(new UnBlockObjectState(door));
            else 
                TryOpen(pnj);
        }

        protected override void OnZoneStay(BrainPnj pnj)
        {
            if (!IsBlocked)
                TryOpen(pnj);
        }

        private void TryOpen(BrainPnj pnj)
        {
            if (door.isOpen) return;
            door.SetOpenSide(pnj.transform.position);
            door.Interact();
        }

#if UNITY_EDITOR
        protected override Color GizmoColor =>
            Application.isPlaying && door != null && IsBlocked ? Color.red : Color.green;
#endif
    }
}
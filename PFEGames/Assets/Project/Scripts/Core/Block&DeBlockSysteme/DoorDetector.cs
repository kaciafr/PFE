using System.Collections.Generic;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin
{
    public class DoorDetector : MonoBehaviour
    {
        [SerializeField] private DoorInteract door;
        [SerializeField] private string blockerLayerName = "Grabable";

        private readonly HashSet<Collider> blockers = new HashSet<Collider>();
        private int blockerLayer;

        private bool IsBlocked => door.DoorState is DoorBlockedState;

        private void Reset() => door = GetComponentInParent<DoorInteract>();
        private void Awake() => blockerLayer = LayerMask.NameToLayer(blockerLayerName);

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == blockerLayer)
            {
                blockers.Add(other);
                RefreshBlock();
                return;
            }

            var pnj = other.GetComponentInParent<BrainPnj>();
            if (pnj == null) return;

            if (IsBlockedFor(pnj))
                pnj.PnjGoTo(new UnBlockObjectState(door));
            else
                TryOpen(pnj);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == blockerLayer) return;

            var pnj = other.GetComponentInParent<BrainPnj>();
            if (pnj != null && !IsBlockedFor(pnj))
                TryOpen(pnj);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != blockerLayer) return;

            blockers.Remove(other);
            RefreshBlock();
        }

        private void RefreshBlock()
        {
            blockers.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            if (blockers.Count > 0)
            {
                if (!IsBlocked) door.Block();
            }
        }

        private bool IsBlockedFor(BrainPnj pnj)
        {
            if (!IsBlocked) return false;

            int pnjSide = door.GetSide(pnj.transform.position);
            foreach (var blocker in blockers)
            {
                if (blocker == null) continue;
                if (door.GetSide(blocker.bounds.center) != pnjSide)
                    return true;
            }
            return false;
        }

        private void TryOpen(BrainPnj pnj)
        {
            if (door.isOpen) return;

            door.SetOpenSide(pnj.transform.position);

            if (IsBlocked) door.OpenDirectly();
            else door.Interact();
        }
    }
}
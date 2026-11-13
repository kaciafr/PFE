using System;
using Goblfin.CharactersSystem.Player.Interface;
using UnityEngine;

namespace Goblfin
{
    public class DoorInteract : MonoBehaviour, IInteractable
    {
        public bool CanInteract => true;
        
        [Header("Ouverture")]
        [SerializeField] private float maxTimeOpen = 2f;
        [SerializeField] private float openDuration = 0.5f;
        [SerializeField] private Vector3 openEulerAngles = new Vector3(0f, 90f, 0f);

        [Header("Côté d'ouverture")]
        [SerializeField] private Vector3 doorNormal = Vector3.forward;
        [SerializeField] private bool invertOpenDirection;

        [Header("État initial")]
        [SerializeField] private bool startBlocked = true;
        
        [Header("Audio")]
        public AudioSource blockDoorSound;        
        public AudioSource openDoorSound;        
        public AudioSource closeDoorSound;        
        public bool isOpen;

        private Quaternion closedRotation;
        private Quaternion openRotation;
        private float openAmount; 
        private float openTimer;

        public event Action<IDoorState> OnStatesChanged;
        public IDoorState DoorState { get; private set; }

        private void Start()
        {
            closedRotation = transform.localRotation;
            ApplyOpenDirection(1f);
            DoorGoTo(startBlocked ? new DoorBlockedState() : new DoorNormalState());
        }

        private void Update()
        {
            DoorState.UpdateState(this);
            
            if (isOpen)
            {
                openTimer += Time.deltaTime;
                if (openTimer >= maxTimeOpen)
                {
                    isOpen = false;
                    openTimer = 0f;
                }
            }
            
            var target = isOpen ? 1f : 0f;
            CloseDoor(target);
        }

        private void CloseDoor(float target)
        {
            openAmount = Mathf.MoveTowards(openAmount, target, Time.deltaTime / openDuration);
            transform.localRotation = Quaternion.Slerp(closedRotation, openRotation, openAmount);
        }

        public void DoorGoTo(IDoorState doorState)
        {
            DoorState?.ExitState(this);
            DoorState = doorState;
            DoorState?.EnterState(this);
            OnStatesChanged?.Invoke(DoorState);
        }

        public void Block()
        {
            isOpen = false;
            openTimer = 0f;
            DoorGoTo(new DoorBlockedState());
        }

        public void Interact() => DoorState.InteractTypeState(this);
        public void SetOpenSide(Vector3 fromWorldPosition)
        {
            if (isOpen || openAmount > 0.001f) return;

            Quaternion parentRotation = transform.parent ? transform.parent.rotation : Quaternion.identity;
            Vector3 normal = parentRotation * closedRotation * doorNormal;
            Vector3 offset = fromWorldPosition - transform.position;
            float dot = Vector3.Dot(offset, normal);

            Debug.Log($"SetOpenSide : dot = {dot:F2}, normal = {normal}, offset = {offset}");

            float side = dot >= 0f ? 1f : -1f;
            Debug.Log(fromWorldPosition);
            if (invertOpenDirection) side = -side;

            ApplyOpenDirection(side);
        }

        private void ApplyOpenDirection(float direction)
        {
            openRotation = closedRotation * Quaternion.Euler(openEulerAngles * direction);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Quaternion parentRotation = transform.parent ? transform.parent.rotation : Quaternion.identity;
            Quaternion closed = Application.isPlaying ? closedRotation : transform.localRotation;
            Vector3 normal = (parentRotation * closed * doorNormal).normalized;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, normal * 1.5f);
            Gizmos.DrawRay(transform.position, -normal * 1.5f);
        }
#endif
    }
}
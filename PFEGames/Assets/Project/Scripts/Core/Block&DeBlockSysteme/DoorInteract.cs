using System;
using Goblfin.CharactersSystem.Player.Interface;
using UnityEngine;

namespace Goblfin
{
    public class DoorInteract : MonoBehaviour,IInteractable
    {
        public bool CanInteract => true;
        [SerializeField] private float maxTimeOpen = 2;
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private Quaternion radialRotation;
        
        
        public bool isOpen = false;
        private Quaternion startRotation;
        private Quaternion currentRotation;
        private float startTime;
        
        public event Action<IDoorState> OnStatesChanged;
        public IDoorState DoorState { get; private set; }

        private void Start()
        {
            startRotation = transform.rotation;
            DoorGoTo(new DoorNormalState());
        }

        private void Update()
        {
            DoorState.UpdateState(this);
            if (isOpen)
            {
                transform.localRotation  = Quaternion.Lerp(startRotation, radialRotation, rotationSpeed);
                currentRotation = transform.localRotation;
                startTime += Time.deltaTime;
            }

            if (isOpen && startTime >= maxTimeOpen)
            {
                isOpen = false;
                startTime = 0;
                transform.localRotation = Quaternion.Lerp(currentRotation,startRotation, rotationSpeed);
            }
        }

        public void DoorGoTo(IDoorState doorState)
        {
            DoorState?.ExitState(this);
            DoorState = doorState;
            DoorState?.EnterState(this);
            OnStatesChanged?.Invoke(DoorState);
        }
        public void Interact()
        {
            DoorState.InteractTypeState(this);
        }
    }
}

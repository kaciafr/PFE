using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input Reader")]
    public class InputReader : ScriptableObject, PlayerAction.IPlayerActions
    {
        public event Action<Vector2> MoveEvent;

        public event Action JumpEvent;
        public event Action JumpCanceledEvent;

        public event Action CrouchEvent;
        public event Action CrouchCanceledEvent;

        public event Action VaultEvent;
        public event Action VaultCanceledEvent;

        public event Action ClimbEvent;
        public event Action ClimbCanceledEvent;

        public event Action ThrowObjectEvent;
        public event Action ThrowObjectCanceledEvent;

        public event Action InteractEvent;
        public event Action InteractEventCanceledEvent;

        public Vector2 MoveValue { get; private set; }

        public bool JumpPressed => _playerInput != null && _playerInput.Player.Jump.WasPressedThisFrame();

        private PlayerAction _playerInput;

        public void EnablePlayerInput()
        {
            if (_playerInput == null)
            {
                _playerInput = new PlayerAction();
                _playerInput.Player.SetCallbacks(this);
            }
            _playerInput.Player.Enable();
        }

        public void DisableAllInput()
        {
            _playerInput?.Player.Disable();
        }

        private void OnDisable()
        {
            DisableAllInput();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveValue = context.ReadValue<Vector2>();
            MoveEvent?.Invoke(MoveValue);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                JumpEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                JumpCanceledEvent?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                CrouchEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                CrouchCanceledEvent?.Invoke();
        }

        public void OnVault(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                VaultEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                VaultCanceledEvent?.Invoke();
        }

        public void OnClimb(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                ClimbEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                ClimbCanceledEvent?.Invoke();
        }

        public void OnThrowObject(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                ThrowObjectEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                ThrowObjectCanceledEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                InteractEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)  
                InteractEventCanceledEvent?.Invoke();
        }
    }
}
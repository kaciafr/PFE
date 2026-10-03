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
        
        public event Action SprintEvent;
        
        public event Action SprintCanceledEvent; 
        
        public event Action GrabEvent;
        public event Action GrabCanceledEvent;
        
        

        public Vector2 MoveValue { get; private set; }

        public bool JumpPressed => _playerInput != null && _playerInput.Player.Jump.WasPressedThisFrame();
        
        public bool CrouchHeld => _playerInput != null && _playerInput.Player.Crouch.IsPressed();

        public bool SprintHeld => _playerInput != null && _playerInput.Player.Sprint.IsPressed();

        public bool GrabHeld => _playerInput != null && _playerInput.Player.Grab.IsPressed();

        public bool ThrowPressed => _playerInput != null && _playerInput.Player.ThrowObject.WasPressedThisFrame();
        
        public bool InteractPressed => _playerInput != null && _playerInput.Player.Interact.WasPressedThisFrame();

        // Molette : > 0 vers le haut, < 0 vers le bas, 0 si elle ne bouge pas cette frame
        public float AimScroll => _playerInput != null ? _playerInput.Player.Aim.ReadValue<float>() : 0f;
        
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

        public void OnGrab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                GrabEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                GrabCanceledEvent?.Invoke();        }

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

        public void OnAim(InputAction.CallbackContext context) { }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                SprintEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                SprintCanceledEvent?.Invoke();
        }
    }
}
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

        public Vector2 MoveValue { get; private set; }

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
    }
}

using UnityEngine;

namespace Characters
{
    public class PlayerStateMachine : MonoBehaviour, IDie
    {
        public InputReader inputAction;
        public PlayerMovement movement;
        public PlayerAnimator animator;

        private PlayerState currentState;

        private void OnEnable()
        {
            inputAction.EnablePlayerInput();
        }

        private void OnDisable()
        {
            inputAction.DisableAllInput();
        }

        private void Start()
        {
            SwitchState(new PlayerIdleState(this));
        }

        private void Update()
        {
            currentState.Tick();
        }

        public void SwitchState(PlayerState nextState)
        {
            currentState?.Exit();     
            currentState = nextState;
            currentState.Enter();
        }
    }
}
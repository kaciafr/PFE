using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerStateMachine : MonoBehaviour
    {
        public PlayerManager Player { get; private set; }
        public PlayerState Current { get; private set; }

        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerSprintState SprintState { get; private set; }
        public PlayerCrouchState CrouchState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerClimbState ClimbState { get; private set; }
        public PlayerClimbTopState ClimbTopState { get; private set; }
        public PlayerGrabState GrabState { get; private set; }
        public PlayerInteractObjectState InteractState { get; private set; }
        public PlayerThrowObjectState ThrowState { get; private set; }

        public PlayerDieState DieState { get; private set; }

        private void Awake()
        {
            Player = GetComponent<PlayerManager>();

            IdleState           = new PlayerIdleState();
            MoveState           = new PlayerMoveState();
            SprintState         = new PlayerSprintState();
            CrouchState         = new PlayerCrouchState();
            JumpState           = new PlayerJumpState();
            ClimbState          = new PlayerClimbState();
            ClimbTopState       = new PlayerClimbTopState();
            GrabState           = new PlayerGrabState();
            InteractState = new PlayerInteractObjectState();
            DieState            = new PlayerDieState();
            ThrowState = new PlayerThrowObjectState(); 
        }

        private void Start()
        {
            SwitchState(IdleState);
        }

        private void Update()
        {
            Current?.Tick(this);
        }

        private void FixedUpdate()
        {
            Current?.FixedTick(this);
        }

        public void SwitchState(PlayerState nextState)
        {
            if (nextState == null || nextState == Current) return;

            Current?.Exit(this);
            Current = nextState;
            Current.Enter(this);
        }
    }
}
using Goblfin.CharactersSystem.Player.Component;
using Goblfin.CharactersSystem.Player.Data;
using Goblfin.CharactersSystem.Player.Input;
using Goblfin.CharactersSystem.Player.States;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player
{
    public class PlayerManager : MonoBehaviour, IDie,IDetected,ISondDetected
    {
        #region References
        public Transform Transform => transform;
        public float speed => rb.linearVelocity.magnitude;
        [SerializeField] private Rigidbody rb;
        [field: SerializeField] public PlayerSettings Settings { get; private set; }
        [field: SerializeField] public InputReader Input { get; private set; }
        [field: SerializeField]public PlayerStateMachine StateMachine { get; private set; }
        [field: SerializeField]public PlayerMotor Motor { get; private set; }
        [field: SerializeField]public PlayerSensor Sensor { get; private set; }
        [field: SerializeField]public PlayerStance Stance { get; private set; }
        [field: SerializeField]public CrateGrabber Grabber { get; private set; }
        [field: SerializeField]public ThrowObject Thrower { get; private set; }
        [field: SerializeField]public PlayerAnimator Animator { get; private set; }
        [field: SerializeField]public PlayerAnimationEvents AnimEvents { get; private set; }
        [field: SerializeField] public Grab RagdollGrab { get; private set; }

        #endregion

        #region Unity Functions

        private void Awake()
        {
	        rb           = GetComponent<Rigidbody>();
            StateMachine = GetComponent<PlayerStateMachine>();
            Motor        = GetComponent<PlayerMotor>();
            Sensor       = GetComponent<PlayerSensor>();
            Stance       = GetComponent<PlayerStance>();
            Grabber      = GetComponent<CrateGrabber>();
            Thrower      = GetComponent<ThrowObject>();
            Animator     = GetComponentInChildren<PlayerAnimator>();
            AnimEvents   = GetComponentInChildren<PlayerAnimationEvents>();
        }

        private void OnEnable()  => Input.EnablePlayerInput();
        private void OnDisable() => Input.DisableAllInput();

        #endregion

        #region Infos

        public Vector2 MoveInput      => Input.MoveValue;
        public Vector3 MoveDirection  => Motor.GetMoveDirection(Input.MoveValue);
        public Vector3 Facing         => Motor.Forward;
        public bool IsGrounded        => Sensor.IsGrounded;
        public bool CanStand          => Stance.CanStand;
        public bool LadderInFront     => Sensor.LadderInFront;
        public Vector3 LadderNormal   => Sensor.LadderNormal;
        public bool CanGrab           => Sensor.IsGrounded && Sensor.CrateInFront != null;
        public bool IsGrabbing        => Grabber.IsGrabbing;
        public float VerticalVelocity => Motor.Velocity.y;

        #endregion

        #region Movement

        public void Move(Vector2 input, float speed)    => Motor.Move(input, speed, !Grabber.IsGrabbing);
        public void MoveWorld(Vector3 dir, float speed) => Motor.MoveWorld(dir, speed, false);
        public void Jump()                              => Motor.Jump(Settings.Jump.Force);
        public void Stop()                              => Motor.Stop();

        #endregion

        #region Posture

        public void Crouch()   => Stance.SetCrouched(true);
        public void UnCrouch() => Stance.SetCrouched(false);

        #endregion

        #region Échelle

        public void StartClimb()       => Motor.StartClimb(Sensor.LadderPoint, Sensor.LadderNormal);
        public void Climb(float input) => Motor.Climb(input * Settings.Climb.Speed);
        public void UnClimb()          => Motor.StopClimb();

        public void StartClimbTop()    => Motor.Freeze();
        public void FinishClimbTop()   => Motor.ClimbOverTop(Sensor.LadderNormal, Sensor.LadderTopY);

        private const float ClimbCooldown = 0.5f;
        private float climbBlockedUntil;
        public bool CanClimb => Time.time >= climbBlockedUntil;

        public void EndClimbTop()
        {
            Motor.Unfreeze();
            climbBlockedUntil = Time.time + ClimbCooldown;
        }

        #endregion

        #region Grab

        public void RagdollGrabing() => RagdollGrab.TryGrab();
        public void UnRagdollGrabing() => RagdollGrab.Release();
        public void Grab()   => Grabber.Grab(Sensor.CrateInFront);
        public void UnGrab() => Grabber.UnGrab();

        #endregion

        #region Interaction

        public bool CanInteract => Sensor.InteractableInFront != null && Sensor.InteractableInFront.CanInteract;
        public void Interact()  => Sensor.InteractableInFront?.Interact();

        #endregion

        #region Throwing

        public Rigidbody ThrowableInFront => Sensor.ThrowableInFront;
        public bool CanPickUp             => Sensor.IsGrounded && Sensor.ThrowableInFront != null && !Thrower.IsHolding;
        public bool IsHolding             => Thrower.IsHolding;

        public void PickUp(Rigidbody obj) => Thrower.Hold(obj);
        public void SetAiming(bool aim)   => Thrower.SetAiming(aim);
        public void Throw()               => Thrower.Release();
        public void DropObject()          => Thrower.Drop();
        public void AdjustThrowPower(float scroll) => Thrower.AdjustPower(scroll);

        #endregion

        #region États

        public PlayerState CurrentState            => StateMachine.Current;
        public bool IsInState(PlayerState state)   => StateMachine.Current == state;
        public void SwitchState(PlayerState state) => StateMachine.SwitchState(state);

        public void Die() => StateMachine.SwitchState(StateMachine.DieState);

        #endregion
    }
}
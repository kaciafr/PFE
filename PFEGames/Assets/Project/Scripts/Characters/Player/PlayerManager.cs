using Characters.Component;
using UnityEditor;
using UnityEngine;
using PlayerSettings = Characters.Data.PlayerSettings;

namespace Characters
{ public class PlayerManager : MonoBehaviour, IDie
    {
        #region References

        [field: SerializeField] public PlayerSettings Settings { get; private set; }
        [field: SerializeField] public InputReader Input { get; private set; }   // ScriptableObject : à assigner dans l'inspecteur

        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public PlayerSensor Sensor { get; private set; }
        public PlayerStance Stance { get; private set; }
        public CrateGrabber Grabber { get; private set; }
        public PlayerAnimator Animator { get; private set; }

        #endregion

        #region Unity Functions

     

        private void OnEnable()  => Input.EnablePlayerInput();
        private void OnDisable() => Input.DisableAllInput();

        #endregion

        #region Infos

        public Vector2 MoveInput      => Input.MoveValue;
        public Vector3 MoveDirection  => Motor.GetMoveDirection(Input.MoveValue);
        public bool IsGrounded        => Sensor.IsGrounded;
        public bool CanStand          => Stance.CanStand;
        public bool LadderInFront     => Sensor.LadderInFront;
        public Vector3 LadderNormal   => Sensor.LadderNormal;
        public bool CanGrab           => Sensor.IsGrounded && Sensor.CrateInFront != null;
        public bool IsGrabbing        => Grabber.IsGrabbing;
        public float VerticalVelocity => Motor.Velocity.y;

        #endregion

        #region Mouvement

        public void Move(Vector2 input, float speed) => Motor.Move(input, speed, !Grabber.IsGrabbing);
        public void Jump()                           => Motor.Jump(Settings.Jump.Force);
        public void Stop()                           => Motor.Stop();

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
        public void FinishClimbTop()   => Motor.ClimbOverTop(Sensor.LadderNormal);
        public void EndClimbTop()      => Motor.Unfreeze();

        #endregion

        #region Caisse

        public void Grab()   => Grabber.Grab(Sensor.CrateInFront);
        public void UnGrab() => Grabber.UnGrab();

        #endregion

        #region États

        public PlayerState CurrentState             => StateMachine.Current;
        public bool IsInState(PlayerState state)    => StateMachine.Current == state;
        public void SwitchState(PlayerState state)  => StateMachine.SwitchState(state);


        #endregion
    }
}
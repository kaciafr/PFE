using Goblfin.CharactersSystem.Player.Component;
using Goblfin.CharactersSystem.Player.Data;
using Goblfin.CharactersSystem.Player.Input;
using Goblfin.CharactersSystem.Player.States;
using Goblfin.PnjSystem;
using Goblfin.PnjSystem.WhatTheyDetect;
using Goblfin.RagdollSystem;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player
{
    public class PlayerManager : MonoBehaviour, IDie, IDetected, ISondDetected
    {
        #region References
        public Transform Transform => transform;
        public float speed => NoiseLevel;

        public float NoiseLevel
        {
            get
            {
                float noise = HorizontalSpeed;

                if (Stance.IsCrouched)
                    noise *= crouchNoiseMultiplier;

                if (landNoiseTimer > 0f)
                    noise = Mathf.Max(noise, landNoise * (landNoiseTimer / landNoiseDuration));

                return noise;
            }
        }

        [SerializeField] private Rigidbody rb;
        [field: SerializeField] public PlayerSettings Settings { get; private set; }
        [field: SerializeField] public InputReader Input { get; private set; }
        [field: SerializeField] public PlayerStateMachine StateMachine { get; private set; }
        [field: SerializeField] public PlayerMotor Motor { get; private set; }
        [field: SerializeField] public PlayerSensor Sensor { get; private set; }
        [field: SerializeField] public PlayerStance Stance { get; private set; }
        [field: SerializeField] public CrateGrabber Grabber { get; private set; }
        [field: SerializeField] public ThrowObject Thrower { get; private set; }
        [field: SerializeField] public PlayerAnimator Animator { get; private set; }
        [field: SerializeField] public PlayerAnimationEvents AnimEvents { get; private set; }
        [field: SerializeField] public Grab RagdollGrab { get; private set; }
        [field: SerializeField] public PlayerJuice Juice { get; private set; }

        [field: SerializeField] public Vector3 startPosition { get; private set; }
        [field: SerializeField] public Quaternion startRotation { get; private set; }
        
        [field: SerializeField] public PlayerVFX VFX { get; private set; }
        
        [Header("Bruit")]
        
        [SerializeField, Range(0, 1), Tooltip("Bruit en crouch (0 = silence total).")]
        private float crouchNoiseMultiplier = 0.2f;
        [SerializeField, Tooltip("Bruit d'une grosse réception (même échelle qu'une vitesse en m/s).")]
        private float landNoise = 8f;
        [SerializeField, Min(0.01f), Tooltip("Durée du pic de bruit après une grosse réception.")]
        private float landNoiseDuration = 0.5f;
        private float landNoiseTimer;   



        #endregion

        #region Unity Functions

        public void Start()
        {
            startPosition = rb.position;
            startRotation = rb.rotation;
        }


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            StateMachine = GetComponent<PlayerStateMachine>();
            Motor = GetComponent<PlayerMotor>();
            Sensor = GetComponent<PlayerSensor>();
            Stance = GetComponent<PlayerStance>();
            Grabber = GetComponent<CrateGrabber>();
            Thrower = GetComponent<ThrowObject>();
            Animator = GetComponentInChildren<PlayerAnimator>();
            AnimEvents = GetComponentInChildren<PlayerAnimationEvents>();
            if (!TryGetComponent(out PlayerJuice juice))
                juice = gameObject.AddComponent<PlayerJuice>();
            Juice = juice;
        }

        private void Update()
        {
            if (Sensor.IsGrounded)
                lastGroundedTime = Time.time;

            if (Input.JumpPressed)
                lastJumpPressedTime = Time.time;

            // Le pic de bruit de réception diminue avec le temps
            if (landNoiseTimer > 0f)
                landNoiseTimer -= Time.deltaTime;
        }

        // Appelé à l'atterrissage : bruit seulement pour les grosses chutes
        public void MakeLandNoise(float fallSpeed)
        {
            if (fallSpeed >= Settings.Jump.HardLandSpeed)
                landNoiseTimer = landNoiseDuration;
        }

        private void FixedUpdate()
        {
            Motor.ApplyExtraGravity(Input.JumpHeld);
        }

        private void OnEnable() => Input.EnablePlayerInput();
        private void OnDisable() => Input.DisableAllInput();

        #endregion

        #region Infos

        public Vector2 MoveInput => Input.MoveValue;
        public Vector3 MoveDirection => Motor.GetMoveDirection(Input.MoveValue);
        public Vector3 Facing => Motor.Forward;
        public bool IsGrounded => Sensor.IsGrounded;
        public bool CanStand => Stance.CanStand;
        public bool LadderInFront => Sensor.LadderInFront;
        public Vector3 LadderNormal => Sensor.LadderNormal;
        public bool CanGrab => Time.time >= grabBlockedUntil && Sensor.IsGrounded && Sensor.CrateInFront != null;
        public bool IsGrabbing => Grabber.IsGrabbing;
        public float VerticalVelocity => Motor.Velocity.y;
        public float HorizontalSpeed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

        #endregion

        #region Movement

        public void Move(Vector2 input, float speed , float rotationMultiplier = 1f) => Motor.Move(input, speed, !Grabber.IsGrabbing, 1f, true , rotationMultiplier );
        public void MoveWorld(Vector3 dir, float speed) => Motor.MoveWorld(dir, speed, false);

        public void MoveinAir(Vector2 input, float speed) => Motor.Move(input, speed, true, Settings.Jump.AirControl , false , Settings.Movement.AirRotationMultiplier);
        
        
        
        public void Jump() => Motor.Jump(Settings.Jump.Force);

        private float lastGroundedTime = -1f;
        private float lastJumpPressedTime = -1f;

        public bool WantsJump =>
            Time.time - lastJumpPressedTime <= Settings.Jump.JumpBuffer &&
            Time.time - lastGroundedTime <= Settings.Jump.CoyoteTime;

        public void ConsumeJump()
        {
            lastJumpPressedTime = -1f;
            lastGroundedTime = -1f;
        }
        public void Stop() => Motor.Stop();
        public void SlowDown() => Motor.SlowDown();

        public async void Respawn(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            if (this == null) return;

            PlayerRagdoll ragdoll = GetComponentInChildren<PlayerRagdoll>();
            if (ragdoll != null) ragdoll.Revive();

            Vector3 position = CheckPointSystem.HasCheckPoint ? CheckPointSystem.Position : startPosition;
            Quaternion rotation = CheckPointSystem.HasCheckPoint ? CheckPointSystem.Rotation : startRotation;

            if (CheckPointSystem.HasCheckPoint && TryGetComponent(out CapsuleCollider capsule))
                position += Vector3.up * ((capsule.height * 0.5f - capsule.center.y) * transform.lossyScale.y);

            rb.linearVelocity = Vector3.zero;
            rb.position = position;
            transform.position = position;
            Motor.Face(rotation * Vector3.forward);

            StateMachine.SwitchState(StateMachine.IdleState);
        }

        #endregion

        #region Posture

        public void Crouch() => Stance.SetCrouched(true);
        public void UnCrouch() => Stance.SetCrouched(false);

        #endregion

        #region Échelle

        public bool IsClimbing { get; private set; }

        public void StartClimb()
        {
            IsClimbing = true;
            Motor.StartClimb(Sensor.LadderPoint, Sensor.LadderNormal);
        }

        public void Climb(float input) => Motor.Climb(input * Settings.Climb.Speed);

        public void UnClimb()
        {
            IsClimbing = false;
            Motor.StopClimb();
        }

        public void StartClimbTop() => Motor.Freeze();

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

        public bool CanGrabRagdoll => RagdollGrab != null && RagdollGrab.HasTarget;
        public void RagdollGrabing() => RagdollGrab.TryGrab();
        public void UnRagdollGrabing() => RagdollGrab.Release();
        public void Grab() => Grabber.Grab(Sensor.CrateInFront, Sensor.CrateHitPoint, Sensor.CrateHitNormal);
        public void UnGrab() => Grabber.UnGrab();

        private const float GrabCooldown = 0.5f;
        private float grabBlockedUntil;
        public void BlockGrab() => grabBlockedUntil = Time.time + GrabCooldown;

        #endregion

        #region Interaction

        public bool CanInteract => Sensor.InteractableInFront != null && Sensor.InteractableInFront.CanInteract;
        public void Interact() => Sensor.InteractableInFront?.Interact();

        #endregion

        #region Throwing

        public Rigidbody ThrowableInFront => Sensor.ThrowableInFront;
        public bool CanPickUp => Sensor.IsGrounded && Sensor.ThrowableInFront != null && !Thrower.IsHolding;
        public bool IsHolding => Thrower.IsHolding;

        public void PickUp(Rigidbody obj) => Thrower.Hold(obj);
        public void SetAiming(bool aim) => Thrower.SetAiming(aim);
        public void Throw() => Thrower.Release();
        public void DropObject() => Thrower.Drop();
        public void AdjustThrowPower(float scroll) => Thrower.AdjustPower(scroll);

        #endregion

        #region États

        public PlayerState CurrentState => StateMachine.Current;
        public bool IsInState(PlayerState state) => StateMachine.Current == state;
        public void SwitchState(PlayerState state) => StateMachine.SwitchState(state);

        public void Die() => StateMachine.SwitchState(StateMachine.DieState);

        #endregion
    }
}
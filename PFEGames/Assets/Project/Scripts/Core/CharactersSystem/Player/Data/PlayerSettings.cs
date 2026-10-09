using System;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Data
{
    #region PlayerSettings

    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Characters/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [field: SerializeField] public MovementSettings Movement { get; private set; } = MovementSettings.Default;
        [field: SerializeField] public JumpSettings Jump { get; private set; } = JumpSettings.Default;
        [field: SerializeField] public CrouchSettings Crouch { get; private set; } = CrouchSettings.Default;
        [field: SerializeField] public GroundSettings Ground { get; private set; } = GroundSettings.Default;
        [field: SerializeField] public ClimbSettings Climb { get; private set; } = ClimbSettings.Default;
        [field: SerializeField] public GrabSettings Grab { get; private set; } = GrabSettings.Default;
        [field: SerializeField] public ThrowSettings Throw { get; private set; } = ThrowSettings.Default;
        
        [field: SerializeField] public InteractSettings Interact { get; private set; } = InteractSettings.Default;

    }

    #endregion

    #region MovementSettings

    [Serializable]
    public struct MovementSettings
    {
        #region Données

        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; }
        [field: SerializeField, Min(0f)] public float SprintSpeed { get; private set; }
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public MovementSettings(float walkSpeed, float sprintSpeed, float rotationSpeed)
        {
            WalkSpeed = walkSpeed;
            SprintSpeed = sprintSpeed;
            RotationSpeed = rotationSpeed;
        }

        public static MovementSettings Default => new(5f, 8f, 10f);

        #endregion
    }

    #endregion

    #region JumpSettings

    [Serializable]
    public struct JumpSettings
    {
        #region Données

        [field: SerializeField, Min(0f)] public float Force { get; private set; }
        [field: SerializeField, Min(0f)] public float JumpSpeed { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public JumpSettings(float force, float jumpSpeed)
        {
            Force = force;
            JumpSpeed = jumpSpeed;
        }

        public static JumpSettings Default => new(6f, 3f);

        #endregion
    }

    #endregion

    #region CrouchSettings

    [Serializable]
    public struct CrouchSettings
    {
        #region Données

        [field: SerializeField, Min(0f)] public float Speed { get; private set; }
        [field: SerializeField, Min(0.1f)] public float Height { get; private set; }
        [field: SerializeField, Min(0f)] public float TransitionSpeed { get; private set; }

        [field: SerializeField, Tooltip("Empeche de ce relever")]
        public LayerMask CeilingLayer { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public CrouchSettings(float speed, float height, float transitionSpeed, LayerMask ceilingLayer)
        {
            Speed = speed;
            Height = height;
            TransitionSpeed = transitionSpeed;
            CeilingLayer = ceilingLayer;
        }

        public static CrouchSettings Default => new(2.5f, 1.5f, 8f, default);

        #endregion
    }

    #endregion

    #region GroundSettings

    [Serializable]
    public struct GroundSettings
    {
        #region Données

        [field: SerializeField] public LayerMask GroundLayer { get; private set; }
        [field: SerializeField, Min(0f)] public float RaycastDistance { get; private set; }
        [field: SerializeField, Min(0f)] public float CheckOffset { get; private set; }

        [field: SerializeField, Min(0)]
        public int RayCount { get; private set; }

        [field: SerializeField, Min(0f)]
        public float RayRadius { get; private set; }

        [field: SerializeField, Range(0f, 90f)]
        public float MaxSlopeAngle { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public GroundSettings(LayerMask groundLayer, float raycastDistance, float checkOffset,
                              int rayCount, float rayRadius, float maxSlopeAngle)
        {
            GroundLayer = groundLayer;
            RaycastDistance = raycastDistance;
            CheckOffset = checkOffset;
            RayCount = rayCount;
            RayRadius = rayRadius;
            MaxSlopeAngle = maxSlopeAngle;
        }

        public static GroundSettings Default => new(default, 1.1f, 0.1f, 8, 0.4f, 50f);

        #endregion
    }

    #endregion

    #region ClimbSettings

    [Serializable]
    public struct ClimbSettings
    {
        #region Données

        [Header("Détection")]
        [field: SerializeField] public LayerMask Layer { get; private set; }
        [field: SerializeField] public float RayHeight { get; private set; }
        [field: SerializeField, Min(0f)] public float DetectDistance { get; private set; }

        [field: SerializeField]
        public float TopCheckHeight { get; private set; }

        [field: SerializeField, Range(0f, 1f)]
        public float ApproachThreshold { get; private set; }

        [Header("Montée")]
        [field: SerializeField, Min(0f)] public float Speed { get; private set; }
        [field: SerializeField, Min(0f)] public float LadderDistance { get; private set; }
        [field: SerializeField, Min(0f)] public float ExitPush { get; private set; }

        [Header("Sortie par le haut")]
        [field: SerializeField, Min(0f)] public float TopDuration { get; private set; }
        [field: SerializeField, Min(0f)] public float TopHeight { get; private set; }
        [field: SerializeField, Min(0f)] public float TopForward { get; private set; }

        [field: SerializeField] public float climbTilt { get; private set; }
        [field: SerializeField] public float climbVisualOffset { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public ClimbSettings(LayerMask layer, float rayHeight, float detectDistance,
                             float topCheckHeight, float approachThreshold,
                             float speed, float ladderDistance, float exitPush,
                             float topDuration, float topHeight, float topForward,
                             float climbTilt, float climbVisualOffset)
        {
            Layer = layer;
            RayHeight = rayHeight;
            DetectDistance = detectDistance;
            TopCheckHeight = topCheckHeight;
            ApproachThreshold = approachThreshold;
            Speed = speed;
            LadderDistance = ladderDistance;
            ExitPush = exitPush;
            TopDuration = topDuration;
            TopHeight = topHeight;
            TopForward = topForward;
            this.climbTilt = climbTilt;
            this.climbVisualOffset = climbVisualOffset;
        }

        public static ClimbSettings Default => new(
            default, 0f, 1f,
            0.5f, 0.5f,
            3f, 0.4f, 2f,
            2f, 1.5f, 0.6f,
            15f, 0.2f);

        #endregion
    }

    #endregion

    #region GrabSettings

    [Serializable]
    public struct GrabSettings
    {
        #region Données

        [field: SerializeField] public LayerMask Layer { get; private set; }
        [field: SerializeField, Min(0f)] public float DetectDistance { get; private set; }
        [field: SerializeField] public float RayHeight { get; private set; }
        [field: SerializeField, Min(0f)] public float PushPullSpeed { get; private set; }

        [field: SerializeField] public float BreakDistance { get; private set; }
        [field: SerializeField] public float MaxGrabMass { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public GrabSettings(LayerMask layer, float detectDistance, float rayHeight, float pushPullSpeed,
                            float breakDistance, float maxGrabMass)
        {
            Layer = layer;
            DetectDistance = detectDistance;
            RayHeight = rayHeight;
            PushPullSpeed = pushPullSpeed;
            BreakDistance = breakDistance;
            MaxGrabMass = maxGrabMass;
        }

        public static GrabSettings Default => new(default, 1.2f, -0.5f, 1.5f, 3f, 50f);

        #endregion
    }

    #endregion

    #region ThrowSettings

    [Serializable]
    public struct ThrowSettings
    {
        #region Données

        [field: SerializeField] public LayerMask LayerObject { get; private set; }
        [field: SerializeField, Min(0f)] public float DetectRadius { get; private set; }
        [field: SerializeField] public float RayHeight { get; private set; }
        [field: SerializeField, Min(0f)] public float PickupDuration { get; private set; }

        [field: SerializeField, Min(0f)]
        public float ThrowDuration { get; private set; }

        #endregion

        #region Constructeur et valeurs par défaut

        public ThrowSettings(LayerMask layerObject, float detectRadius, float rayHeight,
                             float pickupDuration, float throwDuration)
        {
            LayerObject = layerObject;
            DetectRadius = detectRadius;
            RayHeight = rayHeight;
            PickupDuration = pickupDuration;
            ThrowDuration = throwDuration;
        }

        public static ThrowSettings Default => new(default, 1.2f, 0f, 0.5f, 1f);

        #endregion
        
        }
    
        
        
    [Serializable]
    public struct InteractSettings
    {
        public float InteractHeight;
        public float InteractRadius ;
        public LayerMask Layer;

        public InteractSettings(LayerMask layer, float interactHeight, float interactRadius)
        {
            InteractHeight = interactHeight;
            Layer = layer;
            InteractRadius = interactRadius;
        }
        // Hauteur 0 = sphère au niveau du centre du joueur, elle touche aussi les leviers bas
        public static InteractSettings Default => new(default, 0f, 0.8f);

    }

    #endregion
}
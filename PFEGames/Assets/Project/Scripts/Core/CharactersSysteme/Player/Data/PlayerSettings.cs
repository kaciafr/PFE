using System;
using UnityEngine;
using Characters.Data;
namespace Characters.Data
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Characters/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [field: SerializeField] public MovementSettings Movement { get; private set; } = new();
        [field: SerializeField] public JumpSettings Jump { get; private set; } = new();
        [field: SerializeField] public CrouchSettings Crouch { get; private set; } = new();
        [field: SerializeField] public GroundSettings Ground { get; private set; } = new();
        [field: SerializeField] public ClimbSettings Climb { get; private set; } = new();
        [field: SerializeField] public GrabSettings Grab { get; private set; } = new();
        [field: SerializeField] public ThrowSettings Throw  { get; private set; } = new();

    }

    [Serializable]
    public class MovementSettings
    {
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float SprintSpeed { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; } = 10f;
    }
    [Serializable]
    public class JumpSettings
    {
        [field: SerializeField, Min(0f)] public float Force { get; private set; } = 6f; 
        [field: SerializeField, Min(0f)] public float JumpSpeed { get; private set; } = 3f;

    }
    
    [Serializable]
    public class CrouchSettings
    {
        [field: SerializeField, Min(0f)] public float Speed { get; private set; } = 2.5f;
        [field: SerializeField, Min(0.1f)] public float Height { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float TransitionSpeed { get; private set; } = 8f;
 
        [field: SerializeField, Tooltip("Empeche de ce relever")]
        public LayerMask CeilingLayer { get; private set; } 
    }
    [Serializable]
    public class GroundSettings
    {
        [field: SerializeField] public LayerMask GroundLayer { get; private set; }
        [field: SerializeField, Min(0f)] public float RaycastDistance { get; private set; } = 1.1f;
        [field: SerializeField, Min(0f)] public float CheckOffset { get; private set; } = 0.1f;

        [field: SerializeField, Min(0), Tooltip("Nombre de rayons autour du rayon central.")]
        public int RayCount { get; private set; } = 8;

        [field: SerializeField, Min(0f), Tooltip("Distance des rayons autour du centre (un peu moins que le rayon de la capsule).")]
        public float RayRadius { get; private set; } = 0.4f;

        [field: SerializeField, Range(0f, 90f), Tooltip("Au-delà de cet angle, la surface ne compte pas comme du sol.")]
        public float MaxSlopeAngle { get; private set; } = 50f;
    }
    
    [Serializable]
    public class ClimbSettings
    {
        [Header("Détection")]
        [field: SerializeField] public LayerMask Layer { get; private set; }
        [field: SerializeField] public float RayHeight { get; private set; } = 0f;
        [field: SerializeField, Min(0f)] public float DetectDistance { get; private set; } = 1f;
 
        [field: SerializeField, Range(0f, 1f), Tooltip("À quel point il faut avancer face à l'échelle pour l'attraper (dot product).")]
        public float ApproachThreshold { get; private set; } = 0.5f;
 
        [Header("Montée")]
        [field: SerializeField, Min(0f)] public float Speed { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float LadderDistance { get; private set; } = 0.4f;
        [field: SerializeField, Min(0f)] public float ExitPush { get; private set; } = 2f;
 
        [Header("Sortie par le haut")]
        [field: SerializeField, Min(0f)] public float TopDuration { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float TopHeight { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float TopForward { get; private set; } = 0.6f;
    }


        [Serializable]
        public class GrabSettings
        {
            [field: SerializeField] public LayerMask Layer { get; private set; }
            [field: SerializeField, Min(0f)] public float DetectDistance { get; private set; } = 1.2f;
            [field: SerializeField] public float RayHeight { get; private set; } = -0.5f;
            [field: SerializeField, Min(0f)] public float PushPullSpeed { get; private set; } = 1.5f;
 
            [field: SerializeField, Min(0f), Tooltip("Distance au-delà de laquelle la caisse est lâchée automatiquement.")]
            public float BreakDistance { get; private set; } = 3f;

            public float MaxGrabMass = 50f; 

        }
        
        [Serializable]
        public class ThrowSettings
        {
            [field: SerializeField] public LayerMask LayerObject { get; private set; }
            [field: SerializeField, Min(0f)] public float DetectRadius { get; private set; } = 1.2f;
            [field: SerializeField] public float RayHeight { get; private set; } = 0f;
            [field: SerializeField, Min(0f)] public float PickupDuration { get; private set; } = 0.5f;

            [field: SerializeField, Min(0f), Tooltip("Durée totale de l'anim Throw avant de revenir en Idle.")]
            public float ThrowDuration { get; private set; } = 1f;

        }
}
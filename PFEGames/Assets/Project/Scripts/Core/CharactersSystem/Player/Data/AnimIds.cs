using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Data
{
    public static class AnimIds
    {
        public static readonly int Idle       = Animator.StringToHash("Idle");
        public static readonly int Move       = Animator.StringToHash("Walk");
        public static readonly int Run        = Animator.StringToHash("Sprint");
        public static readonly int Jump       = Animator.StringToHash("Jump");
        public static readonly int Crouch     = Animator.StringToHash("Crouch");
        public static readonly int CrouchIdle = Animator.StringToHash("CrouchIdle");
        public static readonly int Climb      = Animator.StringToHash("Climb");
        public static readonly int ClimbTop   = Animator.StringToHash("ClimbTop");
        public static readonly int Grab       = Animator.StringToHash("Grab");
        public static readonly int Pull       = Animator.StringToHash("Pull");
        public static readonly int Interact   = Animator.StringToHash("InteractObject");
        public static readonly int Hold       = Animator.StringToHash("Hold");
        public static readonly int HoldWalk   = Animator.StringToHash("HoldWalk");
        public static readonly int Throw      = Animator.StringToHash("Throw");
        public static readonly int Die        = Animator.StringToHash("Die");
    }

    public static class AnimsParams
    {
        public static readonly int DirX       = Animator.StringToHash("DirX");
        public static readonly int DirY       = Animator.StringToHash("DirY");
        public static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");
        public static readonly int GrabSpeed  = Animator.StringToHash("GrabSpeed");
    }
}
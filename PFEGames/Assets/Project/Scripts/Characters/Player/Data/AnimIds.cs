using UnityEngine;


    public static class AnimIds
    {
        public static readonly int Idle  = Animator.StringToHash("Idle");
        public static readonly int Move = Animator.StringToHash("Walk"); 
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Climb = Animator.StringToHash("Climb");
        public static readonly int Run = Animator.StringToHash("Run");
        public static readonly int ClimbTop = Animator.StringToHash("ClimbTop"); 
        public static readonly int Crouch = Animator.StringToHash("Crouch");
        public static readonly int CrouchIdle = Animator.StringToHash("CrouchIdle");
        public static readonly int Die = Animator.StringToHash("Die");
        public static readonly int Grab = Animator.StringToHash("Grab");
        public static readonly int InteractObject = Animator.StringToHash("InteractObject");
    }

    public static class AnimsParam
    {
        public static readonly int DirX       = Animator.StringToHash("DirX");
        public static readonly int DirY       = Animator.StringToHash("DirY");
        public static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");
        public static readonly int GrabSpeed  = Animator.StringToHash("GrabSpeed");    
    }

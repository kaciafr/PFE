using UnityEngine;

namespace Characters
{
    public class PlayerAnimator : MonoBehaviour
    {
        public Animator animator; 
        
        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            direction = direction.normalized;
            animator.SetFloat("DirX", direction.x);
            animator.SetFloat("DirY", direction.y);
        }

        public void Play(string stateName)
        {
            animator.CrossFadeInFixedTime(stateName,0.15f);
        }
        
        public void SetClimbSpeed(float speed) 
        {
            animator.SetFloat("ClimbSpeed", speed); 
        }
        
        
    }
}
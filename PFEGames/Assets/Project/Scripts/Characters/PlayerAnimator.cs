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
    }
}
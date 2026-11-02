using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player
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

        public void Play(int stateId , float fade)
        {
            animator.CrossFadeInFixedTime(stateId, fade);
        }
        
        public void SetClimbSpeed(float speed) 
        {
            animator.SetFloat(AnimsParams.ClimbSpeed, speed); 
        }

        public void SetGrabSpeed(float speed) => animator.SetFloat(AnimsParams.GrabSpeed, speed);

        public void SetMoveSpeed(float ratio) => animator.SetFloat(AnimsParams.MoveSpeed, ratio, 0.1f, Time.deltaTime);
        
        
    }
}
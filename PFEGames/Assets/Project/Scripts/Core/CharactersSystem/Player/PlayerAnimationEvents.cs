using System;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action ThrowRelease;
        public event Action Footstep; 
        
        public void OnFootstep() => Footstep?.Invoke();

        public void OnThrowRelease() => ThrowRelease?.Invoke();
    }
}
